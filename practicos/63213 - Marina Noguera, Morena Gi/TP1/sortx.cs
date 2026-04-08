using System;
using System.Collections.Generic;

//Flujo Principal del Programa
try
{
    var config = ParseArgs(args);
    Console.WriteLine("Configuración cargada correctamente.");
    Console.WriteLine($" > Entrada: {config.InputFile ?? "(teclado/stdin)"}");
    Console.WriteLine($" > Salida:  {config.OutputFile ?? "(pantalla/stdout)"}");
    Console.WriteLine($" > Separador: '{config.Delimiter}'");
    Console.WriteLine($" > Reglas de orden: {config.SortFields.Count}");
    string textoCrudo = ReadInput(config);
    Console.WriteLine("Lectura exitosa.");
    Console.WriteLine($" > Caracteres leídos: {textoCrudo.Length}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\n¡Ups! Algo salió mal: {ex.Message}");
    Environment.Exit(1);
}

//funcion ParseArgs
AppConfig ParseArgs(string[] argumentos)
{
    string? input = null;
    string? output = null;
    string separador = ",";
    bool sinEncabezado = false;
    var reglas = new List<SortField>();

    for (int i = 0; i < argumentos.Length; i++)
    {
        string actual = argumentos[i];
        switch (actual)
        {
            case "-h":
            case "--help":
                MostrarAyuda();
                Environment.Exit(0);
                break;

            case "-d":
            case "--delimiter":
                separador = argumentos[++i].Replace("\\t", "\t");
                break;

            case "-nh":
            case "--no-header":
                sinEncabezado = true;
                break;

            case "-b":
            case "--by":
                // Desglosamos el formato campo:tipo:orden
                var partes = argumentos[++i].Split(':');
                string nombreColumna = partes[0];
                bool esNumerico = partes.Length > 1 && partes[1].ToLower() == "num";
                bool esDescendente = partes.Length > 2 && partes[2].ToLower() == "desc";

                reglas.Add(new SortField(nombreColumna, esNumerico, esDescendente));
                break;

            case "-i":
            case "--input":
                input = argumentos[++i];
                break;

            case "-o":
            case "--output":
                output = argumentos[++i];
                break;

            default:
                if (!actual.StartsWith("-"))
                {
                    if (input == null) input = actual;
                    else if (output == null) output = actual;
                }
                else
                {
                    throw new ArgumentException($"La opción '{actual}' no me suena de nada. Usá --help para ver qué puedo hacer.");
                }
                break;
        }
    }

    return new AppConfig(input, output, separador, sinEncabezado, reglas);

    // Una mini-función local para no ensuciar el switch
    void MostrarAyuda()
    {
        Console.WriteLine("Herramienta sortx");
        Console.WriteLine("Modo de uso: sortx [entrada] [salida] [opciones]");
        Console.WriteLine("\nOpciones:");
        Console.WriteLine("  -b, --by <campo:tipo:orden>");
        Console.WriteLine("  -d, --delimiter");
        Console.WriteLine("  -nh, --no-header");
        Console.WriteLine("  -h, --help");
    }
}
string ReadInput(AppConfig config)
{
    // Se pasó una ruta de archivo
    if (!string.IsNullOrEmpty(config.InputFile))
    {
        if (!File.Exists(config.InputFile))
        {
            throw new FileNotFoundException($"No pude encontrar el archivo: {config.InputFile}");
        }
        return File.ReadAllText(config.InputFile);
    }
    
    // No se paso un archivo, pero si datos de consola
    if (Console.IsInputRedirected)
    {
        return Console.In.ReadToEnd();
    }
    
    // No paso nada para leer
    throw new InvalidOperationException("No se pasó ningún archivo ni datos para leer.");
}

//Modelo de Datos
record SortField(string Name, bool Numeric, bool Descending);
record AppConfig(string? InputFile, string? OutputFile, string Delimiter, bool NoHeader, List<SortField> SortFields);