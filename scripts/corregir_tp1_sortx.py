#!/usr/bin/env python3

from __future__ import annotations

import argparse
import csv
import os
import re
import subprocess
import sys
import tempfile
import unicodedata
from dataclasses import dataclass
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CSV = REPO_ROOT / "alumnos" / "tp1.csv"
DEFAULT_PRACTICOS = REPO_ROOT / "practicos"
MIN_PRESENTED_LINES = 20

EMPLEADOS_CSV = """nombre,apellido,edad,salario,departamento
Carlos,García,35,85000,Ingeniería
Ana,Martínez,28,72000,Diseño
Luis,Rodríguez,42,120000,Gerencia
María,López,31,88000,Ingeniería
Pedro,Sánchez,25,65000,Diseño
Laura,González,38,95000,Gerencia
"""

ORDENADO_POR_APELLIDO = """nombre,apellido,edad,salario,departamento
Carlos,García,35,85000,Ingeniería
Laura,González,38,95000,Gerencia
María,López,31,88000,Ingeniería
Ana,Martínez,28,72000,Diseño
Luis,Rodríguez,42,120000,Gerencia
Pedro,Sánchez,25,65000,Diseño
"""

ORDENADO_POR_SALARIO_DESC = """nombre,apellido,edad,salario,departamento
Luis,Rodríguez,42,120000,Gerencia
Laura,González,38,95000,Gerencia
María,López,31,88000,Ingeniería
Carlos,García,35,85000,Ingeniería
Ana,Martínez,28,72000,Diseño
Pedro,Sánchez,25,65000,Diseño
"""

ORDENADO_POR_DEPTO_Y_SALARIO = """nombre,apellido,edad,salario,departamento
Ana,Martínez,28,72000,Diseño
Pedro,Sánchez,25,65000,Diseño
Luis,Rodríguez,42,120000,Gerencia
Laura,González,38,95000,Gerencia
María,López,31,88000,Ingeniería
Carlos,García,35,85000,Ingeniería
"""


@dataclass(frozen=True)
class TestCase:
    name: str
    args: tuple[str, ...]
    expected_exit: int | None = 0
    stdin_text: str | None = None
    input_file_name: str | None = "empleados.csv"
    expected_stdout: str | None = None
    output_file_name: str | None = None
    expected_output_file: str | None = None
    require_stdout_nonempty: bool = False
    require_stderr_nonempty: bool = False


@dataclass(frozen=True)
class Evaluation:
    legajo: int
    resultado: str
    detalle: str = ""


def build_test_cases() -> list[TestCase]:
    return [
        TestCase(
            name="help",
            args=("--help",),
            expected_exit=0,
            input_file_name=None,
            require_stdout_nonempty=True,
        ),
        TestCase(
            name="apellido",
            args=("empleados.csv", "-b", "apellido"),
            expected_exit=0,
            expected_stdout=ORDENADO_POR_APELLIDO,
        ),
        TestCase(
            name="salario-desc",
            args=("empleados.csv", "-b", "salario:num:desc"),
            expected_exit=0,
            expected_stdout=ORDENADO_POR_SALARIO_DESC,
        ),
        TestCase(
            name="departamento-salario",
            args=("empleados.csv", "-b", "departamento", "-b", "salario:num:desc"),
            expected_exit=0,
            expected_stdout=ORDENADO_POR_DEPTO_Y_SALARIO,
        ),
        TestCase(
            name="salida-archivo",
            args=("empleados.csv", "-b", "apellido:alpha:asc", "-o", "salida.csv"),
            expected_exit=0,
            output_file_name="salida.csv",
            expected_output_file=ORDENADO_POR_APELLIDO,
        ),
        TestCase(
            name="stdin",
            args=("-b", "apellido"),
            expected_exit=0,
            stdin_text=EMPLEADOS_CSV,
            input_file_name=None,
            expected_stdout=ORDENADO_POR_APELLIDO,
        ),
        TestCase(
            name="columna-inexistente",
            args=("empleados.csv", "-b", "columnaInexistente"),
            expected_exit=None,
            require_stderr_nonempty=False,
        ),
    ]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Compila y prueba todas las entregas TP1/sortx.cs dentro de practicos."
    )
    parser.add_argument(
        "--csv",
        type=Path,
        default=DEFAULT_CSV,
        help=f"Archivo con legajos. Default: {DEFAULT_CSV}",
    )
    parser.add_argument(
        "--practicos",
        type=Path,
        default=DEFAULT_PRACTICOS,
        help=f"Carpeta raiz de practicos. Default: {DEFAULT_PRACTICOS}",
    )
    parser.add_argument(
        "--legajo",
        dest="legajos",
        action="append",
        type=int,
        help="Evaluar solo uno o varios legajos. Repetible.",
    )
    parser.add_argument(
        "--build-timeout",
        type=int,
        default=20,
        help="Timeout en segundos para compilar cada entrega. Default: 20",
    )
    parser.add_argument(
        "--run-timeout",
        type=int,
        default=5,
        help="Timeout en segundos para cada prueba. Default: 5",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Incluye el detalle de compilacion o de la prueba fallida.",
    )
    return parser.parse_args()


def load_legajos(csv_path: Path, practicos_dir: Path) -> list[int]:
    found: dict[int, None] = {}

    if csv_path.is_file():
        with csv_path.open("r", encoding="utf-8-sig", newline="") as handle:
            reader = csv.reader(handle)
            for row in reader:
                if not row:
                    continue
                token = row[0].strip()
                if token.isdigit():
                    found.setdefault(int(token), None)

    if practicos_dir.is_dir():
        for child in practicos_dir.iterdir():
            if child.is_dir() and child.name.isdigit():
                found.setdefault(int(child.name), None)

    return sorted(found)


def normalize_text(text: str) -> str:
    normalized = unicodedata.normalize("NFC", text)
    normalized = normalized.replace("\r\n", "\n").replace("\r", "\n")
    return normalized.rstrip("\n")


def count_lines(path: Path) -> int:
    with path.open("r", encoding="utf-8", errors="replace") as handle:
        return sum(1 for _ in handle)


def short_output(text: str, limit: int = 12) -> str:
    lines = normalize_text(text).splitlines()
    if not lines:
        return "(sin salida)"
    if len(lines) <= limit:
        return " | ".join(lines)
    head = " | ".join(lines[:limit])
    return f"{head} | ..."


def run_command( cmd: list[str], *, cwd: Path, timeout: int, stdin_text: str | None = None, ) -> subprocess.CompletedProcess[str]:
    env = os.environ.copy()
    env.setdefault("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
    env.setdefault("DOTNET_NOLOGO", "1")
    env.setdefault("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1")

    return subprocess.run(
        cmd,
        cwd=cwd,
        input=stdin_text,
        text=True,
        encoding="utf-8",
        errors="replace",
        capture_output=True,
        timeout=timeout,
        env=env,
        check=False,
    )


def build_submission( source_file: Path, build_dir: Path, timeout: int, ) -> tuple[bool, Path | None, str]:
    command = [ "dotnet", "build", str(source_file), "-o", str(build_dir), "-v:q", "--nologo", ]

    try:
        completed = run_command(command, cwd=source_file.parent, timeout=timeout)
    except subprocess.TimeoutExpired:
        return False, None, "timeout compilando"

    dll_path = build_dir / f"{source_file.stem}.dll"
    if completed.returncode != 0 or not dll_path.is_file():
        details = completed.stderr.strip() or completed.stdout.strip() or "compilacion fallida"
        return False, None, short_output(details)

    return True, dll_path, ""


def prepare_case_dir(case_dir: Path, case: TestCase) -> None:
    case_dir.mkdir(parents=True, exist_ok=True)

    if case.input_file_name:
        (case_dir / case.input_file_name).write_text(EMPLEADOS_CSV, encoding="utf-8")

    if case.output_file_name:
        output_file = case_dir / case.output_file_name
        if output_file.exists():
            output_file.unlink()


def evaluate_case( dll_path: Path, case: TestCase, case_dir: Path, timeout: int, ) -> tuple[bool, str]:
    prepare_case_dir(case_dir, case)

    command = ["dotnet", "exec", str(dll_path), *case.args]

    try:
        completed = run_command(
            command,
            cwd=case_dir,
            timeout=timeout,
            stdin_text=case.stdin_text,
        )
    except subprocess.TimeoutExpired:
        return False, f"{case.name}: timeout"

    if case.expected_exit is None:
        if completed.returncode == 0:
            return False, f"{case.name}: esperaba codigo de salida distinto de 0"
    elif completed.returncode != case.expected_exit:
        return False, (
            f"{case.name}: codigo de salida {completed.returncode}, "
            f"esperado {case.expected_exit}"
        )

    if case.require_stdout_nonempty and not completed.stdout.strip():
        return False, f"{case.name}: salida estandar vacia"

    if case.require_stderr_nonempty and not completed.stderr.strip():
        return False, f"{case.name}: salida de error vacia"

    if case.expected_stdout is not None:
        actual_stdout = normalize_text(completed.stdout)
        expected_stdout = normalize_text(case.expected_stdout)
        if actual_stdout != expected_stdout:
            return False, (
                f"{case.name}: stdout inesperado. "
                f"recibido={short_output(completed.stdout)}"
            )

    if case.output_file_name and case.expected_output_file is not None:
        output_file = case_dir / case.output_file_name
        if not output_file.is_file():
            return False, f"{case.name}: no genero {case.output_file_name}"
        actual_output = normalize_text(output_file.read_text(encoding="utf-8", errors="replace"))
        expected_output = normalize_text(case.expected_output_file)
        if actual_output != expected_output:
            return False, (
                f"{case.name}: {case.output_file_name} inesperado. "
                f"recibido={short_output(actual_output)}"
            )

    return True, ""


def evaluate_submission( legajo: int, practicos_dir: Path, temp_root: Path, build_timeout: int, run_timeout: int, test_cases: list[TestCase], ) -> Evaluation:
    source_file = practicos_dir / str(legajo) / "TP1" / "sortx.cs"
    if not source_file.is_file():
        return Evaluation(legajo, "no-presentado")

    line_count = count_lines(source_file)
    if line_count < MIN_PRESENTED_LINES:
        return Evaluation(legajo, "no-presentado", f"archivo con {line_count} lineas")

    build_dir = temp_root / f"build-{legajo}"
    build_dir.mkdir(parents=True, exist_ok=True)

    compiled, dll_path, build_details = build_submission(source_file, build_dir, build_timeout)
    if not compiled or dll_path is None:
        return Evaluation(legajo, "no-compila", build_details)

    for index, case in enumerate(test_cases, start=1):
        case_dir = temp_root / f"case-{legajo}-{index:02d}-{slugify(case.name)}"
        ok, details = evaluate_case(dll_path, case, case_dir, run_timeout)
        if not ok:
            return Evaluation(legajo, "falla", details)

    return Evaluation(legajo, "funciona")


def slugify(value: str) -> str:
    value = value.lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-") or "caso"


def emit_results(results: list[Evaluation], verbose: bool) -> None:
    writer = csv.writer(sys.stdout, lineterminator="\n")
    headers = ["legajo", "resultado"]
    if verbose:
        headers.append("detalle")
    writer.writerow(headers)

    for item in results:
        row = [item.legajo, item.resultado]
        if verbose:
            row.append(item.detalle)
        writer.writerow(row)


def main() -> int:
    args = parse_args()
    if args.legajos:
        legajos = sorted(set(args.legajos))
    else:
        legajos = load_legajos(args.csv, args.practicos)

    if not legajos:
        print("No se encontraron legajos para evaluar.", file=sys.stderr)
        return 1

    test_cases = build_test_cases()

    with tempfile.TemporaryDirectory(prefix="tp1-sortx-") as tmp:
        temp_root = Path(tmp)
        results = [
            evaluate_submission( legajo=legajo, practicos_dir=args.practicos, temp_root=temp_root, build_timeout=args.build_timeout, run_timeout=args.run_timeout, test_cases=test_cases, )
            for legajo in legajos
        ]

    emit_results(results, args.verbose)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
