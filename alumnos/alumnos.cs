using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Flags]
enum Estado: int {
    Vacio       = 0,
    Aprobado    = 1,
    Pendiente   = 2,
    Revision    = 4,
    Desaprobado = 8,
}

static class EstadoExtensions {
    public static string ToEmoji(this Estado estado) {
        return estado switch {
            Estado.Desaprobado  => "🔴",
            Estado.Revision     => "🟠",
            Estado.Pendiente    => "🟡",
            Estado.Aprobado     => "🟢",
            Estado.Vacio        => "⚪️",
            _ => string.Empty
        };
    }

    public static Estado Parse(string? valor) {
        string? v = valor?.Trim().ToUpperInvariant();

        return v switch {
            "🔴" or "D" => Estado.Desaprobado,
            "🟠" or "R" => Estado.Revision,
            "🟡" or "P" => Estado.Pendiente,
            "🟢" or "A" => Estado.Aprobado,
            "" or "-" or "⚪️" or null => Estado.Vacio,
            _ => Estado.Vacio,
        };
    }
}

class Alumno {
    public int Legajo;
    public string Comision = string.Empty;
    public string Nombre = string.Empty;
    public string Apellido = string.Empty;
    public string Telefono = string.Empty;
    public bool TieneFoto;
    public string GitHub = string.Empty;
    public List<Estado> practicos = new();
    public List<Estado> examenes = new();

    public string CarpetaNombre => $"{Legajo} - {Apellido}, {Nombre}";
    public string TelefonoId => FormatearTelefonoId(Telefono);

    public void Practico(int numero, Estado estado){
        if (practicos == null) { practicos = new(); }

        while(practicos.Count < numero){
            practicos.Add(Estado.Vacio);
        }

        practicos[numero - 1] = estado;
    }
    
    public void Examen(int numero, Estado estado){
        if (examenes == null) { examenes = new(); }

        while(examenes.Count < numero){
            examenes.Add(Estado.Vacio);
        }

        examenes[numero - 1] = estado;
    }

    static string FormatearTelefonoId(string telefono) {
        string digitos = Regex.Replace(telefono.Trim(), @"\D", string.Empty);

        if (string.IsNullOrWhiteSpace(digitos)) {
            return string.Empty;
        }

        if (digitos.StartsWith("549")) {
            return digitos;
        }

        if (digitos.StartsWith("54")) {
            return $"549{digitos.Substring(2)}";
        }

        if (digitos.StartsWith("0")) {
            digitos = digitos.Substring(1);
        }

        return $"549{digitos}";
    }

    public bool ConGithub => !string.IsNullOrWhiteSpace(GitHub) && GitHub.Length > 3;  
    public bool ConFoto => TieneFoto;
    public bool ConTelefono => !string.IsNullOrWhiteSpace(Telefono);
    public string NombreCompleto => $"{Apellido}, {Nombre}".Trim().Trim(',');
}

class Alumnos: IEnumerable<Alumno> {
    public List<Alumno> Lista { get; set; } = new();

    public int Count => Lista.Count;

    public Alumno this[int index] {
        get { return Lista[index]; }
    }

    public Alumnos(IEnumerable<Alumno> alumnos) {
        Lista = alumnos?.ToList() ?? new();
    }   

    public void Agregar(Alumno alumno) {
        Lista.Add(alumno);
    }

    public Alumnos ConGithub(bool tiene = true) =>
        new Alumnos(Lista.Where(alumno => tiene == alumno.ConGithub));

    public Alumnos ConPractico(int numero, Estado estado) =>
        new Alumnos(Lista.Where(alumno =>
            alumno.practicos != null &&
            alumno.practicos.Count >= numero &&
            alumno.practicos[numero - 1].HasFlag(estado)));
        
    public Alumnos ConTelefono(bool tiene = true) =>
        new Alumnos(Lista.Where(alumno => tiene == alumno.ConTelefono));

    public Alumnos ConFotos(bool tiene = true) =>
        new Alumnos(Lista.Where(alumno => tiene == alumno.ConFoto));

    public Alumnos EnComision(string comision) =>
        new Alumnos(Lista.Where(alumno => string.Equals(alumno.Comision, comision, StringComparison.OrdinalIgnoreCase)));

    public Alumnos ParaAgregar() =>
        new Alumnos(Lista.Where(alumno => alumno.GitHub == "(agregar)"));

    public IEnumerator<Alumno> GetEnumerator() {
        return Lista.GetEnumerator();
    }  

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

class AlumnosManager {
    const int LongitudMaximaLineaVcard = 75;

    public static Alumnos CargarAlumnos(string rutaArchivo) {
        Alumnos alumnos = new Alumnos(Array.Empty<Alumno>());
        string comisionActual = string.Empty;
        
        try {
            string[] lineas = File.ReadAllLines(rutaArchivo, Encoding.UTF8);
            
            foreach (string linea in lineas) {
                if (string.IsNullOrWhiteSpace(linea)) {
                    continue;
                }

                if (linea.StartsWith("## ")) {
                    comisionActual = linea.Substring(3).Trim();
                    continue;
                }

                if (linea.StartsWith("#") || linea.StartsWith("```") || linea.StartsWith("Legajo") || linea.StartsWith("------")) {
                    continue;
                }

                Alumno? alumno = null;

                if (linea.Contains("|")) {
                    alumno = ExtraerAlumnoFormatoLegado(linea);
                } else {
                    alumno = ExtraerAlumnoFormatoMarkdown(linea, comisionActual);
                }

                if (alumno != null) {
                    alumnos.Agregar(alumno);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error al leer el archivo: {ex.Message}");
        }
        
        return alumnos;
    }


    static Alumno? ExtraerAlumno(string linea, string separador = "|") {
        List<string> datos = Regex.Split(linea.TrimEnd(), @"\s*" + Regex.Escape(separador) + @"\s*").ToList();

        while(datos.Count < 9){
            datos.Add(string.Empty);
        }

        if (!int.TryParse(datos[0].Trim(), out int legajo)) {
            return null;
        }

        return new Alumno {
            Legajo    = legajo,
            Comision  = LimpiarCampo(datos[1]),
            Nombre    = LimpiarCampo(datos[2]),
            Apellido  = LimpiarCampo(datos[3]),
            Telefono  = ExtraerTelefono(datos[4]),
            TieneFoto = ExtraerFoto(datos[5]),
            GitHub    = ExtraerGitHub(datos[6]),
            practicos = ExtraerPracticos(datos[7]),
            examenes  = ExtraerExamenes(datos[8])
        };
    }

    static Alumno? ExtraerAlumnoFormatoLegado(string linea) {
        return ExtraerAlumno(linea, "|");
    }

    static Alumno? ExtraerAlumnoFormatoMarkdown(string linea, string comisionActual) {
        List<string> columnas = Regex.Split(linea.TrimEnd(), @"\s{2,}").ToList();

        while (columnas.Count < 7) {
            columnas.Add(string.Empty);
        }

        if (!int.TryParse(columnas[0].Trim(), out int legajo)) {
            return null;
        }

        (string apellido, string nombre) = ExtraerApellidoNombre(columnas[1]);

        return new Alumno {
            Legajo = legajo,
            Comision = LimpiarCampo(comisionActual),
            Nombre = nombre,
            Apellido = apellido,
            Telefono = ExtraerTelefono(columnas[2]),
            TieneFoto = ExtraerFoto(columnas[3]),
            GitHub = ExtraerGitHub(columnas[4]),
            practicos = ExtraerPracticos(columnas[5]),
            examenes = ExtraerExamenes(columnas[6])
        };
    }

    
    public static void Guardar(Alumnos alumnos, string rutaArchivo) {
        try {
            List<Alumno> alumnosOrdenados = new(alumnos);
            alumnosOrdenados.Sort(CompararAlumnos);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# TUP 2026 - Programación III");
            sb.AppendLine();
            
            string? comisionActual = null;

            foreach (Alumno alumno in alumnosOrdenados) {
                string comisionAlumno = ObtenerComision(alumno);

                if (comisionActual != comisionAlumno) {
                    if (comisionActual != null) {
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }

                    comisionActual = comisionAlumno;
                    sb.AppendLine($"## {comisionActual}");
                    sb.AppendLine("```text");
                    sb.AppendLine("Legajo  Nombre y Apellido           Telefono         Foto  Github                   Practicos           Examenes         ");
                    sb.AppendLine("------  --------------------------  ---------------  ----  -----------------------  ------------------  ------------------");
                }

                sb.AppendLine(FormatearFila(alumno));
            }

            if (comisionActual != null) {
                sb.AppendLine("```");
            }
            
            File.WriteAllText(rutaArchivo, sb.ToString().TrimEnd() + Environment.NewLine, Encoding.UTF8);
            Console.WriteLine($"Alumnos guardados en: {rutaArchivo}");
        } catch (Exception ex) {
            Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
        }
    }

    public static void Listar(Alumnos alumnos, string titulo = "Listado de Alumnos") {
        if (alumnos == null || alumnos.Count == 0) {
            Console.WriteLine("No hay alumnos para mostrar.");
            return;
        }

        List<Alumno> alumnosOrdenados = new(alumnos);
        alumnosOrdenados.Sort(CompararAlumnos);

        string encabezado = FormatearFilaTabla("Legajo", "Nombre y Apellido", "Telefono", "Foto", "GitHub", "Comision", "Practicos", "Examenes");
        string separador = new string('-', encabezado.Length);
        ConsoleColor colorAnterior = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(titulo.ToUpper());
        Console.ForegroundColor = colorAnterior;
        Console.WriteLine(separador);
        Console.WriteLine(encabezado);
        Console.WriteLine(separador);

        foreach (Alumno alumno in alumnosOrdenados) {
            Console.WriteLine(FormatearFilaTabla(
                alumno.Legajo.ToString(),
                ObtenerNombreApellido(alumno),
                FormatearTexto(alumno.Telefono),
                alumno.TieneFoto ? "Si" : "No",
                FormatearGitHub(alumno.GitHub),
                ObtenerComision(alumno),
                FormatearEstados(alumno.practicos),
                FormatearEstados(alumno.examenes)));
        }

        Console.WriteLine(separador);
        Console.WriteLine($"Total de alumnos: {alumnos.Count}");
        Console.WriteLine();
    }

    static string FormatearFilaTabla(string legajo, string nombreApellido, string telefono, string foto, string gitHub, string comision, string pruebas, string examenes) {
        string colLegajo         = AjustarColumna(legajo, 6);
        string colNombreApellido = AjustarColumna(nombreApellido, 26);
        string colTelefono       = AjustarColumna(telefono, 15);
        string colFoto           = AjustarColumna(foto, 4);
        string colGitHub         = AjustarColumna(gitHub, 25);
        string colComision       = AjustarColumna(comision, 10);
        string colPruebas        = AjustarColumna(pruebas, 20);
        string colExamenes       = AjustarColumna(examenes, 20);

        return $"{colLegajo}  {colNombreApellido}  {colTelefono}  {colFoto}  {colGitHub}  {colComision}  {colPruebas}  {colExamenes}";
    }

    
    static int CompararAlumnos(Alumno alumnoA, Alumno alumnoB) {
        int comparacion = string.Compare(ObtenerComision(alumnoA), ObtenerComision(alumnoB), StringComparison.OrdinalIgnoreCase);
        if (comparacion != 0) {
            return comparacion;
        }

        comparacion = string.Compare(FormatearTexto(alumnoA.Apellido), FormatearTexto(alumnoB.Apellido), StringComparison.OrdinalIgnoreCase);
        if (comparacion != 0) {
            return comparacion;
        }

        comparacion = string.Compare(FormatearTexto(alumnoA.Nombre), FormatearTexto(alumnoB.Nombre), StringComparison.OrdinalIgnoreCase);
        if (comparacion != 0) {
            return comparacion;
        }

        return alumnoA.Legajo.CompareTo(alumnoB.Legajo);
    }

    static string ObtenerComision(Alumno alumno) {
        return FormatearTexto(alumno.Comision);
    }

    static string FormatearFila(Alumno alumno) {
        string legajo         = AjustarColumna(alumno.Legajo.ToString(), 6);
        string nombreApellido = AjustarColumna(ObtenerNombreApellido(alumno), 26);
        string telefono       = AjustarColumna(FormatearTexto(alumno.Telefono), 15);
        string foto           = AjustarColumna(alumno.TieneFoto ? "Si" : "No", 4);
        string gitHub         = AjustarColumna(FormatearGitHub(alumno.GitHub), 23);
        string pruebas        = AjustarColumna(FormatearEstados(alumno.practicos, 10));
        string examenes       = AjustarColumna(FormatearEstados(alumno.examenes, 10));

        return $"{legajo}  {nombreApellido}  {telefono}  {foto}  {gitHub}  {pruebas}  {examenes}";
    }

    static string ObtenerNombreApellido(Alumno alumno) {
        string apellido = FormatearTexto(alumno.Apellido);
        string nombre   = FormatearTexto(alumno.Nombre);

        if (apellido == "—") {
            return nombre;
        }

        if (nombre == "—") {
            return apellido;
        }

        return $"{apellido}, {nombre}";
    }

    static string AjustarColumna(string texto, int ancho=20) {
        string valor = FormatearTexto(texto);

        if (valor.Length > ancho) {
            return valor.Substring(0, ancho);
        }

        return valor.PadRight(ancho);
    }

    static string FormatearTexto(string texto) {
        if (string.IsNullOrWhiteSpace(texto)) {
            return "—";
        }

        return texto.Trim();
    }

    static string LimpiarCampo(string texto) {
        string valor = texto.Trim();
        if (valor == "—") { return string.Empty; }
        return valor;
    }

    static string ExtraerTelefono(string texto) {
        return LimpiarCampo(texto);
    }

    static (string, string) ExtraerApellidoNombre(string nombreCompleto) {
        string apellido , nombre;
        var partes = nombreCompleto.Split(',', 2);

        if (partes.Length == 2) {
            apellido = LimpiarCampo(partes[0]);
            nombre   = LimpiarCampo(partes[1]);
        } else {
            apellido = LimpiarCampo(nombreCompleto);
            nombre = "";
        }

        return (apellido, nombre);
    }

    static string ExtraerGitHub(string texto) {
        string valor = LimpiarCampo(texto);
        if (string.Equals(valor, "No", StringComparison.OrdinalIgnoreCase)) { return string.Empty; }
        return valor;
    }

    static string FormatearGitHub(string gitHub) {
        if (string.IsNullOrWhiteSpace(gitHub)) { return "-"; }
        return gitHub.Trim();
    }

    static string FormatearEstados(List<Estado> estados, int maxEstados = 20) {
        string valor = "";
        if (estados != null && estados.Count > 0) {
            valor = string.Join("", estados.Select(e => e.ToEmoji()));
        }
        valor = valor.Replace(" ", "⚪️");
        while (StringInfo.ParseCombiningCharacters(valor).Length < maxEstados) {
            valor += "⚪️";         
        }
        return valor;
    }

    static List<Estado> ParsearEstados(string texto) {
        string valor = texto.Trim();

        if (string.IsNullOrWhiteSpace(valor) || valor == "-" || valor == "—") {
            return new List<Estado>();
        }

        List<Estado> estados = new();
        TextElementEnumerator enumerador = StringInfo.GetTextElementEnumerator(valor);

        while (enumerador.MoveNext()) {
            string elemento = enumerador.GetTextElement();
            Estado estado = EstadoExtensions.Parse(elemento);

            if (estado != Estado.Vacio) {
                estados.Add(estado);
            }
        }

        return estados;
    }

    static bool ExtraerFoto(string texto) {
        return string.Equals(texto.Trim(), "Si", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(texto.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    }

    static List<Estado> ExtraerPracticos(string pruebas){
        return ParsearEstados(pruebas);
    } 

    static List<Estado> ExtraerExamenes(string examenes){
        return ParsearEstados(examenes);
    }

    static bool TieneMismoLegajo(string nombreCarpeta, int legajo) {
        return nombreCarpeta.StartsWith($"{legajo} - ") || nombreCarpeta.StartsWith($"{legajo}_");
    }

    static List<string> BuscarCarpetasMismoLegajo(string rutaBase, int legajo) {
        List<string> carpetasMismoLegajo = new();

        if (!Directory.Exists(rutaBase)) {
            return carpetasMismoLegajo;
        }

        foreach (string carpetaExistente in Directory.GetDirectories(rutaBase)) {
            string nombreCarpetaExistente = Path.GetFileName(carpetaExistente);

            if (TieneMismoLegajo(nombreCarpetaExistente, legajo)) {
                carpetasMismoLegajo.Add(carpetaExistente);
            }
        }

        return carpetasMismoLegajo;
    }

    public static void CrearCarpetas(Alumnos alumnos) {
        string rutaBase = Path.Combine("..", "practicos");
        Directory.CreateDirectory(rutaBase);

        foreach (Alumno alumno in alumnos) {
            string nombreCarpeta = alumno.CarpetaNombre;
            string rutaCarpeta = Path.Combine(rutaBase, nombreCarpeta);

            try {
                List<string> carpetasMismoLegajo = BuscarCarpetasMismoLegajo(rutaBase, alumno.Legajo);

                if (carpetasMismoLegajo.Count == 0) {
                    Directory.CreateDirectory(rutaCarpeta);
                    Console.WriteLine($"Carpeta creada: {rutaCarpeta}");
                    continue;
                }

                if (carpetasMismoLegajo.Count > 1) {
                    Console.WriteLine($"Hay múltiples carpetas para el legajo {alumno.Legajo}. Revisar manualmente antes de renombrar.");
                    continue;
                }

                string rutaCarpetaExistente = carpetasMismoLegajo[0];
                string nombreCarpetaExistente = Path.GetFileName(rutaCarpetaExistente);

                if (nombreCarpetaExistente == nombreCarpeta) {
                    Console.WriteLine($"Carpeta existente correcta: {rutaCarpetaExistente}");
                    continue;
                }

                Directory.Move(rutaCarpetaExistente, rutaCarpeta);
                Console.WriteLine($"Carpeta renombrada: {rutaCarpetaExistente} -> {rutaCarpeta}");
            } catch (Exception ex) {
                Console.WriteLine($"Error al crear la carpeta para {nombreCarpeta}: {ex.Message}");
            }
        }
    }

    public static void CopiarFotoPerfil(Alumnos alumnos, string rutaFotos) {
        string rutaBase = Path.Combine("..", "practicos");

        if (!Directory.Exists(rutaBase)) {
            Console.WriteLine($"No existe la carpeta base de prácticos: {rutaBase}");
            return;
        }

        foreach (Alumno alumno in alumnos) {
            if (string.IsNullOrWhiteSpace(alumno.TelefonoId)) {
                continue;
            }

            string rutaFotoOrigen = Path.Combine(rutaFotos, alumno.TelefonoId, "foto.png");
            if (!File.Exists(rutaFotoOrigen)) {
                continue;
            }

            List<string> carpetasMismoLegajo = BuscarCarpetasMismoLegajo(rutaBase, alumno.Legajo);
            if (carpetasMismoLegajo.Count != 1) {
                continue;
            }

            string rutaCarpetaAlumno = carpetasMismoLegajo[0];
            if (!Directory.Exists(rutaCarpetaAlumno)) {
                continue;
            }

            string rutaFotoDestino = Path.Combine(rutaCarpetaAlumno, "foto.png");

            if (File.Exists(rutaFotoDestino)) {
                continue;
            }

            try {
                File.Copy(rutaFotoOrigen, rutaFotoDestino);
                Console.WriteLine($"Foto copiada: {rutaFotoOrigen} -> {rutaFotoDestino}");
            } catch (Exception ex) {
                Console.WriteLine($"Error al copiar la foto para {alumno.CarpetaNombre}: {ex.Message}");
            }
        }
    }

    public static void CopiarEnunciadoPracticos(Alumnos alumnos, string practico){
        string nombrePractico = practico.Trim();
        string rutaOrigen = Path.Combine("..", "enunciados", nombrePractico);
        string rutaBasePracticos = Path.Combine("..", "practicos");
        string carpetaPractico = nombrePractico.ToLower();

        if (string.IsNullOrWhiteSpace(nombrePractico)) {
            Console.WriteLine("Debe indicar el nombre del práctico a copiar.");
            return;
        }

        if (!Directory.Exists(rutaOrigen)) {
            Console.WriteLine($"No existe la carpeta del enunciado: {rutaOrigen}");
            return;
        }

        if (!Directory.Exists(rutaBasePracticos)) {
            Console.WriteLine($"No existe la carpeta base de prácticos: {rutaBasePracticos}");
            return;
        }

        foreach (Alumno alumno in alumnos) {
            string rutaAlumno = Path.Combine(rutaBasePracticos, alumno.CarpetaNombre);

            if (!Directory.Exists(rutaAlumno)) {
                Console.WriteLine($"No existe la carpeta del alumno: {rutaAlumno}");
                continue;
            }

            string rutaDestino = Path.Combine(rutaAlumno, carpetaPractico);

            try {
                CopiarContenidoDirectorio(rutaOrigen, rutaDestino);
                Console.WriteLine($"Enunciado copiado: {rutaOrigen} -> {rutaDestino}");
            } catch (Exception ex) {
                Console.WriteLine($"Error al copiar el enunciado para {alumno.CarpetaNombre}: {ex.Message}");
            }
        }
    }

    static void CopiarContenidoDirectorio(string rutaOrigen, string rutaDestino) {
        Directory.CreateDirectory(rutaDestino);

        foreach (string archivoOrigen in Directory.GetFiles(rutaOrigen)) {
            string nombreArchivo = Path.GetFileName(archivoOrigen);
            string archivoDestino = Path.Combine(rutaDestino, nombreArchivo);
            File.Copy(archivoOrigen, archivoDestino, overwrite: true);
        }

        foreach (string subdirectorioOrigen in Directory.GetDirectories(rutaOrigen)) {
            string nombreSubdirectorio = Path.GetFileName(subdirectorioOrigen);
            string subdirectorioDestino = Path.Combine(rutaDestino, nombreSubdirectorio);
            CopiarContenidoDirectorio(subdirectorioOrigen, subdirectorioDestino);
        }
    }

    public static void ActualizarDesdePerfiles(Alumnos alumnos, string rutaPerfiles) {
        Dictionary<int, Alumno> porLegajo = new Dictionary<int, Alumno>();

        foreach (Alumno alumno in alumnos) {
            porLegajo[alumno.Legajo] = alumno;
        }

        foreach (string carpetaPerfil in Directory.GetDirectories(rutaPerfiles)) {
            string rutaPerfil = Path.Combine(carpetaPerfil, "perfil.md");
            if (!File.Exists(rutaPerfil)) {
                continue;
            }

            try {
                int legajo = 0;
                string gitHub = string.Empty;

                foreach (string linea in File.ReadAllLines(rutaPerfil, Encoding.UTF8)) {
                    string l = linea.Trim();

                    if (l.StartsWith("- Legajo:")) {
                        string valor = l.Substring("- Legajo:".Length).Trim();
                        int.TryParse(valor, out legajo);
                    } else if (l.StartsWith("- Github:")) {
                        string valor = l.Substring("- Github:".Length).Trim();

                        if (!string.IsNullOrWhiteSpace(valor) &&
                            !valor.StartsWith("No", StringComparison.OrdinalIgnoreCase)) {
                            gitHub = valor;
                        }
                    }
                }

                if (legajo == 0 || !porLegajo.ContainsKey(legajo)) {
                    continue;
                }

                Alumno alumno = porLegajo[legajo];
                bool actualizado = false;

                if (!string.IsNullOrWhiteSpace(gitHub) && alumno.GitHub != gitHub) {
                    Console.WriteLine($"  GitHub actualizado {alumno.CarpetaNombre}: '{alumno.GitHub}' -> '{gitHub}'");
                    alumno.GitHub = gitHub;
                    actualizado = true;
                }

                if (!actualizado) {
                    Console.WriteLine($"  Sin cambios: {alumno.CarpetaNombre}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error al leer perfil {rutaPerfil}: {ex.Message}");
            }
        }
    }

    public static void GuardarJSON(Alumnos alumnos, string rutaArchivo) {
        try {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");

            for (int i = 0; i < alumnos.Count; i++) {
                Alumno alumno = alumnos[i];
                sb.AppendLine("  {");
                sb.AppendLine($"    \"legajo\": {alumno.Legajo},");
                sb.AppendLine($"    \"comision\": \"{EscaparJson(alumno.Comision)}\",");
                sb.AppendLine($"    \"nombre\": \"{EscaparJson(alumno.Nombre)}\",");
                sb.AppendLine($"    \"apellido\": \"{EscaparJson(alumno.Apellido)}\",");
                sb.AppendLine($"    \"telefono\": \"{EscaparJson(alumno.Telefono)}\",");
                sb.AppendLine($"    \"tieneFoto\": {alumno.TieneFoto.ToString().ToLowerInvariant()},");
                sb.AppendLine($"    \"gitHub\": \"{EscaparJson(alumno.GitHub)}\",");
                sb.AppendLine($"    \"practicos\": [ {string.Join(", ", alumno.practicos.Select(p => string.Concat(((char)34).ToString(), p.ToEmoji(), ((char)34).ToString())))} ],");
                sb.AppendLine($"    \"examenes\": [ {string.Join(", ", alumno.examenes.Select(e => string.Concat(((char)34).ToString(), e.ToEmoji(), ((char)34).ToString())))} ]");
                sb.Append("  }");

                if (i < alumnos.Count - 1) {
                    sb.Append(',');
                }

                sb.AppendLine();
            }

            sb.AppendLine("]");
            File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);
            Console.WriteLine($"Alumnos guardados en JSON: {rutaArchivo}");
        } catch (Exception ex) {
            Console.WriteLine($"Error al guardar el archivo JSON: {ex.Message}");
        }
    }

    public static void GuardarVCard(Alumnos alumnos, string rutaArchivo) {
        try {
            List<Alumno> alumnosConTelefono = alumnos
                .Where(alumno => !string.IsNullOrWhiteSpace(alumno.TelefonoId))
                .OrderBy(ObtenerComision)
                .ThenBy(alumno => FormatearTexto(alumno.Apellido), StringComparer.OrdinalIgnoreCase)
                .ThenBy(alumno => FormatearTexto(alumno.Nombre), StringComparer.OrdinalIgnoreCase)
                .ThenBy(alumno => alumno.Legajo)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (Alumno alumno in alumnosConTelefono) {
                AppendVCardContacto(sb, alumno);
            }

            File.WriteAllText(rutaArchivo, sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine($"Contactos vCard guardados en: {rutaArchivo}");
        } catch (Exception ex) {
            Console.WriteLine($"Error al guardar el archivo vCard: {ex.Message}");
        }
    }

    static void AppendVCardContacto(StringBuilder sb, Alumno alumno) {
        string apellido = FormatearTextoVcard(alumno.Apellido);
        string nombre = FormatearTextoVcard(alumno.Nombre);
        string nombreCompleto = FormatearTextoVcard(ObtenerNombreVisible(alumno));
        string comision = FormatearTextoVcard(ObtenerComision(alumno));
        string etiquetaBusqueda = FormatearTextoVcard(ObtenerEtiquetaBusqueda(alumno));
        string etiquetaVisible = FormatearTextoVcard(ObtenerEtiquetaVisible(alumno));
        string telefonoE164 = $"+{alumno.TelefonoId}";

        sb.AppendLine("BEGIN:VCARD");
        sb.AppendLine("VERSION:3.0");
        sb.AppendLine($"N:{apellido};{nombre};;;");
        sb.AppendLine($"FN:{nombreCompleto} | {etiquetaVisible}");
        sb.AppendLine($"NICKNAME:{etiquetaVisible}");
        sb.AppendLine($"ORG:TUP 2026 - Programacion III");
        sb.AppendLine($"CATEGORIES:{etiquetaBusqueda}");
        sb.AppendLine($"NOTE:Legajo {alumno.Legajo} | Comision {comision} | Etiqueta {etiquetaBusqueda} | GitHub {ObtenerGitHubVisible(alumno)}");
        sb.AppendLine($"X-TUP-LEGAJO:{alumno.Legajo}");
        sb.AppendLine($"X-TUP-COMISION:{comision}");
        sb.AppendLine($"TEL;TYPE=CELL;TYPE=VOICE:{telefonoE164}");

        sb.AppendLine("END:VCARD");
    }

    static string FormatearTextoVcard(string texto) {
        string valor = FormatearTexto(texto);
        return valor
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
    }

    static string ObtenerNombreVisible(Alumno alumno) {
        string apellido = FormatearTexto(alumno.Apellido);
        string nombre = FormatearTexto(alumno.Nombre);

        if (apellido == "—") {
            return nombre;
        }

        if (nombre == "—") {
            return apellido;
        }

        return $"{nombre} {apellido}";
    }

    static string ObtenerGitHubVisible(Alumno alumno) {
        string gitHub = FormatearGitHub(alumno.GitHub);
        return gitHub == "-" ? "sin GitHub" : gitHub;
    }

    static string ObtenerEtiquetaBusqueda(Alumno alumno) {
        return $"TUP26-P3-{ObtenerComision(alumno)}";
    }

    static string ObtenerEtiquetaVisible(Alumno alumno) {
        return $"{ObtenerEtiquetaBusqueda(alumno)}-{alumno.Legajo}";
    }

    static string EscaparJson(string texto) {
        return texto
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}


class WAppService {
    readonly string? store;
    readonly TimeSpan timeout;
    bool gruposHabilitados = true;
    List<GrupoWhatsApp> grupos = new();

    public WAppService(string? store = null, TimeSpan? timeout = null, bool refrescarGrupos = true) {
        this.store = store;
        this.timeout = timeout ?? TimeSpan.FromMinutes(5);

        try {
            CargarGrupos(refrescarGrupos);
        } catch (InvalidOperationException ex) when (EsErrorAutenticacionWacli(ex)) {
            gruposHabilitados = false;
            grupos = new();
            Console.WriteLine("Aviso: wacli no está autenticado; se omite la carga de grupos.");
        }
    }

    public void EnviarMensajeAGrupo(string grupo, string mensaje, bool json = false) {
        string grupoJid = ResolverJidGrupo(grupo);
        EnviarTexto(grupoJid, mensaje, json);
    }

    public void EnviarTexto(string destinatario, string mensaje, bool json = false) {
        if (string.IsNullOrWhiteSpace(destinatario)) {
            throw new ArgumentException("El destinatario no puede estar vacío.", nameof(destinatario));
        }

        if (string.IsNullOrWhiteSpace(mensaje)) {
            throw new ArgumentException("El mensaje no puede estar vacío.", nameof(mensaje));
        }

        List<string> argumentos = new() { "send", "text", "--to", destinatario, "--message", mensaje };

        Ejecutar(json, argumentos);
    }

    public void InvitarParticipanteAGrupo(string grupo, params string[] usuarios) {
        InvitarParticipantesAGrupo(grupo, false, usuarios);
    }

    public void InvitarParticipantesAGrupo(string grupo, bool json, params string[] usuarios) {
        if (string.IsNullOrWhiteSpace(grupo)) {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        if (usuarios is null || usuarios.Length == 0) {
            throw new ArgumentException("Debes indicar al menos un participante.", nameof(usuarios));
        }

        string grupoJid = ResolverJidGrupo(grupo);

        List<string> argumentos = new() { "groups", "participants", "add", "--jid", grupoJid };

        foreach (string usuario in usuarios) {
            if (string.IsNullOrWhiteSpace(usuario)) { continue; }

            argumentos.Add("--user");
            argumentos.Add(usuario);
        }

        if (!argumentos.Any(argumento => argumento == "--user")) {
            throw new ArgumentException("Debes indicar al menos un participante válido.", nameof(usuarios));
        }

        Ejecutar(json, argumentos);
    }

    public void InvitarGrupoComision(IEnumerable<Alumno> alumnos, bool json = false) {
        if (alumnos is null) {
            throw new ArgumentNullException(nameof(alumnos));
        }

        List<Alumno> alumnosValidos = alumnos
            .Where(alumno =>
                !string.IsNullOrWhiteSpace(alumno.Comision) &&
                !string.IsNullOrWhiteSpace(alumno.TelefonoId))
            .ToList();

        foreach (IGrouping<string, Alumno> grupoComision in alumnosValidos
            .GroupBy(alumno => ObtenerReferenciaGrupoPorComision(alumno.Comision))
            .OrderBy(grupo => grupo.Key, StringComparer.OrdinalIgnoreCase)) {
            string[] usuarios = grupoComision
                .Select(alumno => alumno.TelefonoId)
                .Where(telefono => !string.IsNullOrWhiteSpace(telefono))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (usuarios.Length == 0) {
                continue;
            }

            Console.WriteLine($"Invitando {usuarios.Length} alumno(s) al grupo {grupoComision.Key}...");
            InvitarParticipantesAGrupo(grupoComision.Key, json, usuarios);
        }
    }

    public record GrupoWhatsApp(string Nombre, string Jid, DateTime? Creado);
    public record ContactoWhatsApp(string Jid, string Name, string PhoneNumber);
    
    public List<ContactoWhatsApp> ListarParticipantesGrupo(string grupo, bool refrescar = false) {
        if (string.IsNullOrWhiteSpace(grupo)) {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        string grupoJid = ResolverJidGrupoParaLectura(grupo, refrescar);

        if (refrescar && gruposHabilitados) {
            try {
                EjecutarYObtenerSalida(false, new() { "groups", "info", "--jid", grupoJid });
            } catch (InvalidOperationException ex) when (EsErrorAutenticacionWacli(ex)) {
                gruposHabilitados = false;
                grupos = new();
                Console.WriteLine("Aviso: wacli no está autenticado; se usan los participantes de la base local.");
            }
        }

        return ListarParticipantesDesdeBaseLocal(grupoJid);
    }

    public string? BuscarJidGrupoPorDescripcion(string descripcion, bool refrescar = false) {
        if (string.IsNullOrWhiteSpace(descripcion)) {
            throw new ArgumentException("La descripción no puede estar vacía.", nameof(descripcion));
        }

        GrupoWhatsApp? grupo = BuscarGrupoPorReferencia(descripcion, refrescar);
        return grupo?.Jid;
    }

    public void RecargarGrupos() {
        if (!gruposHabilitados) {
            Console.WriteLine("Aviso: no se pueden recargar grupos porque wacli no está autenticado.");
            return;
        }

        CargarGrupos(true);
    }

    public List<GrupoWhatsApp> ListarGrupos(bool refrescar = false) {
        if (!gruposHabilitados) {
            return new();
        }

        if (refrescar || grupos.Count == 0) {
            CargarGrupos(refrescar);
        }

        return new List<GrupoWhatsApp>(grupos);
    }

    string ResolverJidGrupo(string grupo) {
        if (string.IsNullOrWhiteSpace(grupo)) {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        string referencia = grupo.Trim();

        if (EsJidGrupo(referencia)) {
            return referencia;
        }

        GrupoWhatsApp? coincidencia = BuscarGrupoPorReferencia(referencia, false);
        if (coincidencia != null) {
            return coincidencia.Jid;
        }

        coincidencia = BuscarGrupoPorReferencia(referencia, true);
        if (coincidencia != null) {
            return coincidencia.Jid;
        }

        throw new InvalidOperationException($"No se encontró ningún grupo para '{referencia}'.");
    }

    string ResolverJidGrupoParaLectura(string grupo, bool refrescar) {
        string referencia = grupo.Trim();

        if (EsJidGrupo(referencia)) {
            return referencia;
        }

        if (gruposHabilitados) {
            GrupoWhatsApp? coincidencia = BuscarGrupoPorReferencia(referencia, refrescar);
            if (coincidencia != null) {
                return coincidencia.Jid;
            }
        }

        string? grupoJid = BuscarJidGrupoEnBaseLocal(referencia);
        if (!string.IsNullOrWhiteSpace(grupoJid)) {
            return grupoJid;
        }

        throw new InvalidOperationException($"No se encontró ningún grupo para '{referencia}'.");
    }

    string? BuscarJidGrupoEnBaseLocal(string referencia) {
        string valor = EscaparSqlite(referencia.Trim());

        List<string> coincidenciasExactas = EjecutarSqlite(
            $"SELECT jid FROM groups WHERE lower(name) = lower('{valor}') OR lower(jid) = lower('{valor}') ORDER BY name, jid;");

        if (coincidenciasExactas.Count == 1) {
            return coincidenciasExactas[0];
        }

        if (coincidenciasExactas.Count > 1) {
            throw new InvalidOperationException($"La referencia exacta '{referencia}' coincide con varios grupos locales.");
        }

        List<string> coincidenciasParciales = EjecutarSqlite(
            $"SELECT jid FROM groups WHERE lower(name) LIKE lower('%{valor}%') OR lower(jid) LIKE lower('%{valor}%') ORDER BY name, jid;");

        if (coincidenciasParciales.Count == 1) {
            return coincidenciasParciales[0];
        }

        if (coincidenciasParciales.Count > 1) {
            throw new InvalidOperationException($"La búsqueda '{referencia}' coincide con varios grupos locales.");
        }

        return null;
    }

    List<ContactoWhatsApp> ListarParticipantesDesdeBaseLocal(string grupoJid) {
        string jid = EscaparSqlite(grupoJid);
        string rutaSessionDb = EscaparSqlite(Path.Combine(ObtenerDirectorioStore(), "session.db"));

        List<string> filas = EjecutarSqlite(
            $@"ATTACH DATABASE '{rutaSessionDb}' AS session;
                SELECT
                    gp.user_jid || char(9) ||
                    COALESCE(
                        NULLIF(ca.alias, ''),
                        NULLIF(c.full_name, ''),
                        NULLIF(c.push_name, ''),
                        NULLIF(c.business_name, ''),
                        NULLIF(c.first_name, ''),
                        NULLIF(sc.full_name, ''),
                        NULLIF(sc.push_name, ''),
                        NULLIF(sc.business_name, ''),
                        NULLIF(sc.first_name, ''),
                        gp.user_jid
                    ) || char(9) ||
                    COALESCE(
                        NULLIF(c.phone, ''),
                        NULLIF(sc.redacted_phone, ''),
                        NULLIF(lm.pn, ''),
                        ''
                    )
                FROM group_participants gp
                LEFT JOIN session.whatsmeow_lid_map lm
                    ON replace(gp.user_jid, '@lid', '') = lm.lid
                LEFT JOIN contacts c
                    ON c.phone = lm.pn
                    OR c.jid = lm.pn || '@s.whatsapp.net'
                LEFT JOIN contact_aliases ca
                    ON ca.jid = gp.user_jid
                    OR ca.jid = lm.pn || '@s.whatsapp.net'
                LEFT JOIN session.whatsmeow_contacts sc
                    ON sc.their_jid = gp.user_jid
                    OR sc.their_jid = lm.pn || '@s.whatsapp.net'
                WHERE gp.group_jid = '{jid}'
                ORDER BY CASE gp.role WHEN 'superadmin' THEN 0 WHEN 'admin' THEN 1 ELSE 2 END, gp.user_jid;");

        return filas.Select(ParsearContactoWhatsApp).ToList();
    }

    static ContactoWhatsApp ParsearContactoWhatsApp(string fila) {
        string[] partes = fila.Split('\t');

        string jid = partes.Length > 0 ? partes[0].Trim() : string.Empty;
        string nombre = partes.Length > 1 ? partes[1].Trim() : string.Empty;
        string telefono = partes.Length > 2 ? partes[2].Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(nombre)) {
            nombre = jid;
        }

        if (string.IsNullOrWhiteSpace(telefono)) {
            telefono = ExtraerTelefonoDesdeJid(jid);
        }

        return new ContactoWhatsApp(jid, nombre, telefono);
    }

    static string ExtraerTelefonoDesdeJid(string jid) {
        if (string.IsNullOrWhiteSpace(jid)) {
            return string.Empty;
        }

        int separador = jid.IndexOf('@');
        string candidato = separador >= 0 ? jid.Substring(0, separador) : jid;

        return candidato.All(char.IsDigit) ? candidato : string.Empty;
    }

    List<string> EjecutarSqlite(string query) {
        string rutaDb = Path.Combine(ObtenerDirectorioStore(), "wacli.db");

        if (!File.Exists(rutaDb)) {
            Console.WriteLine($"Aviso: no existe la base local de wacli: {rutaDb}");
            return new();
        }

        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "sqlite3",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(rutaDb);
        startInfo.ArgumentList.Add(query);

        using Process proceso = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar sqlite3.");

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error = proceso.StandardError.ReadToEnd().Trim();

        proceso.WaitForExit();

        if (proceso.ExitCode != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            throw new InvalidOperationException($"sqlite3 falló con código {proceso.ExitCode}: {detalle}");
        }

        return salida.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(linea => linea.Trim())
                     .Where(linea => !string.IsNullOrWhiteSpace(linea))
                     .ToList();
    }

    string ObtenerDirectorioStore() {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (string.IsNullOrWhiteSpace(store)) {
            return Path.Combine(home, ".wacli");
        }

        if (store == "~") {
            return home;
        }

        if (store.StartsWith("~/", StringComparison.Ordinal)) {
            return Path.Combine(home, store.Substring(2));
        }

        return Environment.ExpandEnvironmentVariables(store);
    }

    static string EscaparSqlite(string valor) {
        return valor.Replace("'", "''");
    }

    
    GrupoWhatsApp? BuscarGrupoPorReferencia(string referencia, bool refrescar) {
        string textoBuscado = referencia.Trim();
        List<GrupoWhatsApp> gruposDisponibles = ListarGrupos(refrescar);

        List<GrupoWhatsApp> coincidenciasExactas = gruposDisponibles
            .Where(grupo =>
                string.Equals(grupo.Nombre, textoBuscado, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(grupo.Jid, textoBuscado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (coincidenciasExactas.Count == 1) {
            return coincidenciasExactas[0];
        }

        if (coincidenciasExactas.Count > 1) {
            string nombres = string.Join(", ", coincidenciasExactas.Select(grupo => $"{grupo.Nombre} ({grupo.Jid})"));
            throw new InvalidOperationException($"La referencia exacta '{textoBuscado}' coincide con varios grupos: {nombres}");
        }

        List<GrupoWhatsApp> coincidenciasParciales = gruposDisponibles
            .Where(grupo =>
                grupo.Nombre.Contains(textoBuscado, StringComparison.OrdinalIgnoreCase) ||
                grupo.Jid.Contains(textoBuscado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (coincidenciasParciales.Count == 1) {
            return coincidenciasParciales[0];
        }

        if (coincidenciasParciales.Count > 1) {
            string nombres = string.Join(", ", coincidenciasParciales.Select(grupo => $"{grupo.Nombre} ({grupo.Jid})"));
            throw new InvalidOperationException($"La búsqueda '{textoBuscado}' coincide con varios grupos: {nombres}");
        }

        return null;
    }

    void CargarGrupos(bool refrescar) {
        if (!gruposHabilitados) {
            grupos = new();
            return;
        }

        if (refrescar) {
            Ejecutar(false, new() { "groups", "refresh" });
        }

        string salida = EjecutarYObtenerSalida(false, new() { "groups", "list" });
        grupos = ParsearGrupos(salida);
    }

    static bool EsJidGrupo(string texto) {
        return texto.EndsWith("@g.us", StringComparison.OrdinalIgnoreCase);
    }

    static string ObtenerReferenciaGrupoPorComision(string comision) {
        string valor = comision.Trim();

        if (string.IsNullOrWhiteSpace(valor)) {
            throw new ArgumentException("La comisión no puede estar vacía.", nameof(comision));
        }

        if (EsJidGrupo(valor)) {
            return valor;
        }

        if (valor.StartsWith("TUP26-P3-", StringComparison.OrdinalIgnoreCase)) {
            return valor;
        }

        return $"TUP26-P3-{valor}";
    }

    static List<GrupoWhatsApp> ParsearGrupos(string salida) {
        List<GrupoWhatsApp> grupos = new();

        foreach (string linea in salida.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)) {
            string actual = linea.Trim();

            if (string.IsNullOrWhiteSpace(actual) || actual.StartsWith("NAME", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            string[] columnas = Regex.Split(actual, @"\s{2,}");
            if (columnas.Length < 3) {
                continue;
            }

            string nombre = columnas[0].Trim();
            string jid = columnas[1].Trim();
            string creado = columnas[2].Trim();

            if (!jid.EndsWith("@g.us", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            DateTime? fechaCreacion = null;
            if (DateTime.TryParse(creado, out DateTime fecha)) {
                fechaCreacion = fecha;
            }

            grupos.Add(new GrupoWhatsApp(nombre, jid, fechaCreacion));
        }

        return grupos;
    }

    void Ejecutar(bool json, List<string> argumentos) {
        string salida = EjecutarYObtenerSalida(json, argumentos);

        if (!string.IsNullOrWhiteSpace(salida)) {
            Console.WriteLine(salida);
        }
    }

    string EjecutarYObtenerSalida(bool json, List<string> argumentos) {
        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "wacli",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (json) {
            startInfo.ArgumentList.Add("--json");
        }

        if (!string.IsNullOrWhiteSpace(store)) {
            startInfo.ArgumentList.Add("--store");
            startInfo.ArgumentList.Add(store);
        }

        startInfo.ArgumentList.Add("--timeout");
        startInfo.ArgumentList.Add(FormatearDuracionWacli(timeout));

        foreach (string argumento in argumentos) {
            startInfo.ArgumentList.Add(argumento);
        }

        using Process proceso = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar wacli.");
        proceso.StandardInput.Close();

        if (!proceso.WaitForExit((int)timeout.TotalMilliseconds)) {
            try {
                proceso.Kill(entireProcessTree: true);
            } catch {
                // Ignoramos errores al intentar detener el proceso.
            }

            throw new TimeoutException($"wacli no terminó dentro de {timeout}.");
        }

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error = proceso.StandardError.ReadToEnd().Trim();

        if (proceso.ExitCode != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            throw new InvalidOperationException($"wacli falló con código {proceso.ExitCode}: {detalle}");
        }

        return salida;
    }

    static string FormatearDuracionWacli(TimeSpan duracion) {
        if (duracion <= TimeSpan.Zero) {
            return "1s";
        }

        if (duracion.TotalSeconds < 60 && duracion.TotalSeconds == Math.Floor(duracion.TotalSeconds)) {
            return $"{(int)duracion.TotalSeconds}s";
        }

        if (duracion.TotalMinutes < 60 && duracion.TotalSeconds % 60 == 0) {
            return $"{(int)duracion.TotalMinutes}m";
        }

        int horas = (int)duracion.TotalHours;
        int minutos = duracion.Minutes;
        int segundos = duracion.Seconds;

        StringBuilder sb = new StringBuilder();

        if (horas > 0) {
            sb.Append($"{horas}h");
        }

        if (minutos > 0) {
            sb.Append($"{minutos}m");
        }

        if (segundos > 0 || sb.Length == 0) {
            sb.Append($"{segundos}s");
        }

        return sb.ToString();
    }

    static bool EsErrorAutenticacionWacli(Exception ex) {
        return ex.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("auth", StringComparison.OrdinalIgnoreCase);
    }
}

record GrupoWhatsApp(string Nombre, string Jid, DateTime? Creado);


class GitHub {
    readonly string owner;
    readonly string repo;

    public GitHub(string owner = "AlejandroDiBattista", string repo = "tup26-p3") {
        this.owner = owner;
        this.repo = repo;
    }

    public bool AgregarColaborador(string usuario) {
        (string salida, string error, int codigoSalida) = EjecutarGh(new[] {
            "api", "--method", "PUT", $"repos/{owner}/{repo}/collaborators/{usuario}", "-f", "permission=push"
        });

        if (codigoSalida != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            Console.WriteLine($"Error al agregar colaborador '{usuario}': {detalle}");
            return false;
        }

        return true;
    }

    public List<string> ListarColaboradores() {
        (string salida, string error, int codigoSalida) = EjecutarGh(new[] {
            "api", $"repos/{owner}/{repo}/collaborators", "--jq", ".[] | select(.permissions.push == true) | .login"
        });

        if (codigoSalida != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            Console.WriteLine($"Error al listar colaboradores: {detalle}");
            return new List<string>();
        }

        return LeerLineas(salida);
    }

    public List<string> ListarInvitacionesPendientes() {
        (string salida, string error, int codigoSalida) = EjecutarGh(new[] {
            "api", $"repos/{owner}/{repo}/invitations", "--paginate", "--jq", ".[].invitee.login"
        });

        if (codigoSalida != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            Console.WriteLine($"Error al listar invitaciones pendientes: {detalle}");
            return new List<string>();
        }

        return LeerLineas(salida);
    }

    (string Salida, string Error, int CodigoSalida) EjecutarGh(IEnumerable<string> argumentos) {
        ProcessStartInfo startInfo = new ProcessStartInfo { FileName = "gh", RedirectStandardOutput = true, RedirectStandardError = true };
        
        foreach (string argumento in argumentos) {
            startInfo.ArgumentList.Add(argumento);
        }

        using Process proceso = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar gh.");

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error  = proceso.StandardError.ReadToEnd().Trim();
 
        proceso.WaitForExit();

        return (salida, error, proceso.ExitCode);
    }

    static List<string> LeerLineas(string texto) {
        return texto.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(linea => linea.Trim())
                    .Where(linea => !string.IsNullOrWhiteSpace(linea))
                    .ToList();
    }
}


class Program {

    static void MensajeGithubErroneo(){
        var alumnos = AlumnosManager.CargarAlumnos("alumnos.md");
        foreach (var comision in new[] { "C7", "C9" }) {
            var lista = alumnos.ConGithub(true).EnComision(comision).ConPractico(1, Estado.Revision);
            if(lista.Count == 0) { continue; }
            Console.WriteLine("""
            *GitHub Erroreos ⁉️*
            
            Estos alumnos me informaron un usuario pero no los encuentro en de GitHub.

            Podria ser que haya un error de tipeo o de carga. 
            Si es así, por favor envien el usuario correcto junto con su legajo para que los autorice a publicar el trabajo práctico.

            ```
            """);
            foreach(var a in lista) {
                Console.WriteLine($"{a.Legajo}: {a.NombreCompleto,20} {a.GitHub}");
            }

            Console.WriteLine("""
            Envien el usuario correcto junto con su legajo por este grupo

            """);
        }
    }
    
    static void MensajeSinGithub(){
        var alumnos = AlumnosManager.CargarAlumnos("alumnos.md");
        foreach (var comision in new[] { "C7", "C9" }) {
            var lista = alumnos.ConGithub(false).EnComision(comision);
            if(lista.Count == 0) { continue; }
            Console.WriteLine("""
            *Sin Usuario GitHub ❓❓*
            
            Los siguientes alumnos no me informaron cual es su usuario en GitHub.
            Es necesario que tengan un usuario de GitHub para poder publicar el trabajo práctico y que yo pueda corregirlo.

            ```
            """);

            foreach(var a in lista) {
                Console.WriteLine($"{a.Legajo}: {a.NombreCompleto}");
            }

            Console.WriteLine("""
            Envien el usuario junto con su legajo por este grupo 

            p.e "63241 josias57455"

            """);
        }
    }

    static void Main(string[] args) {
        Alumnos alumnos = AlumnosManager.CargarAlumnos( "alumnos.md");

        foreach(var a in alumnos.ConGithub()){
            a.Practico(1, Estado.Revision);
        }
        foreach(var a in alumnos.ConGithub(false)){
            a.Practico(1, Estado.Desaprobado);
        }

        // alumnos[10].Practico(5, Estado.EnProgreso);   
        // AlumnosManager.Listar(alumnos);
        AlumnosManager.Guardar(alumnos, "alumnos.md");
        // AlumnosManager.Listar(alumnos.ConFotos(false), "Alumnos sin foto");
        // AlumnosManager.ActualizarDesdePerfiles(alumnos, "../material-docente/perfil/perfiles");
        // // AlumnosManager.Guardar(alumnos, "alumnos.md");
        // // AlumnosManager.GuardarJSON(alumnos, "alumnos.json");
        // // AlumnosManager.GuardarVCard(alumnos, "alumnos.vcf");
        // // AlumnosManager.CrearCarpetas(alumnos);
        // AlumnosManager.CopiarFotoPerfil(alumnos, rutaPerfiles);

        // // var sinFoto = alumnos.SinFotos();
        // // AlumnosManager.Guardar(sinFoto, "alumnos-sin-foto.md");

        // // var sinTelefono = alumnos.SinTelefono();
        // // AlumnosManager.Guardar(sinTelefono, "alumnos-sin-telefono.md");

        // // var sinGitHub = alumnos.FiltrarSinGithub();
        // // AlumnosManager.Guardar(sinGitHub, "alumnos-sin-github.md");


        // Console.WriteLine("Enviando mensaje al grupo de WhatsApp...");

        // Alumnos alumnosConTelefonoComision7 = alumnos.EnComision("C7").SinTelefono();
        // AlumnosManager.Listar(alumnosConTelefonoComision7);

        // Alumnos alumnosConTelefonoComision9 = alumnos.EnComision("C9").SinTelefono();
        
        // AlumnosManager.Listar(alumnosConTelefonoComision9);
        // // wapp.InvitarGrupoComision(alumnos);

        // AlumnosManager.Listar(alumnos.ParaAgregar());

        // // AlumnosManager.GuardarVCard(alumnos.ParaAgregar().EnComision("C7"), "alumnos-agregar-c7.vcf");
        // // AlumnosManager.GuardarVCard(alumnos.ParaAgregar().EnComision("C9"), "alumnos-agregar-c9.vcf");
        // // wapp.InvitarGrupoComision(alumnos.ParaAgregar());
        // AlumnosManager.CopiarEnunciadoPracticos(alumnos, "tp1");
        GitHub gh = new GitHub();

        // var C7 = [63415, 63456, 63268, 63402, 63419, 63776, 63399, 63211, 63350, 61581, 63647, 63420, 63354, 63393, 63208, 63387, 63547, 63447, 61490, 63397, 63696];
        // var C9 = [63385, 63217, 63313, 63222, 61801, 63150, 63461, 64016, 61641, 63737, 61057, 63717, 61161, 62844, 63231, 63425, 61907, 63219, 63297, 63388, 63494, 63418, 63412, 63205, 63220, 63232, 63216, 61026];
        // var pedidos = C7 + C9;

        var colaboradores = gh.ListarColaboradores();
        var invitaciones  = gh.ListarInvitacionesPendientes();
        foreach(var a in alumnos) {
            if(a.ConGithub) {
                var usuario = a.GitHub;
                if(colaboradores.Contains(usuario, StringComparer.OrdinalIgnoreCase)) {
                    Console.WriteLine($"🟢: {a.GitHub}");
                    a.Practico(1, Estado.Aprobado);
                } else if(invitaciones.Contains(usuario, StringComparer.OrdinalIgnoreCase)) {
                    Console.WriteLine($"🟡: {a.GitHub}");
                    a.Practico(1, Estado.Pendiente);
                } else if(gh.AgregarColaborador(a.GitHub)) {
                    Console.WriteLine($"⚪: {a.GitHub}");
                    a.Practico(1, Estado.Vacio);
                } else {
                    Console.WriteLine($"🟠: {a.GitHub}");
                    a.Practico(1, Estado.Revision);
                }
            } else {
                Console.WriteLine($"🔴: {a.GitHub}");
                a.Practico(1, Estado.Desaprobado);
            }
        }

        AlumnosManager.Guardar(alumnos, "alumnos.md");
        
        MensajeSinGithub();
        // MensajeGithubErroneo();


        // WAppService wapp = new WAppService();
        // foreach(var p in wapp.ListarParticipantesGrupo("TUP26-P3-C7")) {
        //     Console.WriteLine($"Participante del grupo C7: {p.Name} | {p.PhoneNumber} | {p.Jid}");
        // }
// 
    }
}

