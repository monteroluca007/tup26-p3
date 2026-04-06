#!/usr/bin/env python3
"""Renombra archivos Markdown del tipo aa.bb-*.md.

- Preserva aa.
- Reasigna bb en orden ascendente dentro de cada grupo aa.
- La nueva numeración empieza en 10 y avanza de 10 en 10.
- Usa un renombrado en dos pasos para evitar colisiones.
"""

from __future__ import annotations

import argparse
import re
import sys
import uuid
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

PATTERN = re.compile(r"^(?P<aa>\d{2,})\.(?P<bb>\d{2,})-(?P<rest>.+)\.md$")


@dataclass(frozen=True)
class Match:
    path: Path
    aa: str
    bb: int
    rest: str


def find_matches(root: Path) -> list[Match]:
    matches: list[Match] = []
    for path in root.rglob("*.md"):
        if not path.is_file():
            continue

        match = PATTERN.match(path.name)
        if match is None:
            continue

        matches.append(
            Match(
                path=path,
                aa=match.group("aa"),
                bb=int(match.group("bb")),
                rest=match.group("rest"),
            )
        )

    return matches


def build_renames(matches: Iterable[Match]) -> list[tuple[Path, Path]]:
    renames: list[tuple[Path, Path]] = []

    grouped: dict[str, list[Match]] = {}
    for match in matches:
        grouped.setdefault(match.aa, []).append(match)

    for aa in sorted(grouped):
        grupo = sorted(grouped[aa], key=lambda item: (int(item.bb), item.rest.lower(), item.path.name.lower()))
        for index, item in enumerate(grupo, start=1):
            new_bb = index * 10
            new_name = f"{aa}.{new_bb:03d}-{item.rest}.md"
            renames.append((item.path, item.path.with_name(new_name)))

    return renames


def rename_safely(renames: list[tuple[Path, Path]], dry_run: bool) -> None:
    if not renames:
        print("No se encontraron archivos que coincidan con el patrón.")
        return

    destinations = {dst.resolve() for _, dst in renames}
    if len(destinations) != len(renames):
        print("Hay nombres de destino repetidos; no se puede continuar.", file=sys.stderr)
        sys.exit(1)

    existing_sources = [src for src, _ in renames if src.exists()]
    if len(existing_sources) != len(renames):
        faltantes = [str(src) for src, _ in renames if not src.exists()]
        print("Algunos archivos de origen ya no existen:", file=sys.stderr)
        for item in faltantes:
            print(f"  - {item}", file=sys.stderr)
        sys.exit(1)

    temp_moves: list[tuple[Path, Path]] = []
    for src, dst in renames:
        temp_name = src.with_name(f".{src.name}.{uuid.uuid4().hex}.tmp")
        temp_moves.append((src, temp_name))
        print(f"{src.name} -> {dst.name}")

    if dry_run:
        return

    # Primera pasada: mover a nombres temporales.
    for src, tmp in temp_moves:
        src.rename(tmp)

    # Segunda pasada: mover al nombre final.
    for (_, dst), (_, tmp) in zip(renames, temp_moves, strict=True):
        tmp.rename(dst)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Renombra archivos Markdown aa.bb-*.md preservando aa y numerando bb de 10 en 10."
    )
    parser.add_argument(
        "root",
        nargs="?",
        default=".",
        help="Directorio raíz donde buscar (por defecto: directorio actual).",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Muestra los cambios sin renombrar nada.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    root = Path(args.root).resolve()

    if not root.exists() or not root.is_dir():
        print(f"La ruta '{root}' no existe o no es un directorio.", file=sys.stderr)
        return 1

    matches = find_matches(root)
    renames = build_renames(matches)
    rename_safely(renames, dry_run=args.dry_run)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
