using System;
using System.Collections.Generic;

// Punto de Entrada

try
{
    AppConfig config = ParseArgs(args);

    Console.WriteLine("Parseo exitoso");
    Console.WriteLine($"Archivo de entrada: {config.InputFile ?? "Ninguno (stdin)"}");
    Console.WriteLine($"Delimitador: '{config.Delimiter}'");
    Console.WriteLine($"Cantidad de filtros: {config.SortFields.Count}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

// Definicion de Funciones

AppConfig ParseArgs(string[] arguments)
{
    string? input = null;
    string? output = null;
    string delim = ",";
    bool noHeader = false;
    List<SortField> sortFields = new();

    for (int i = 0; i < arguments.Length; i++)
    {
        string arg = arguments[i];

        switch (arg)
        {
            case "-h":
            case "--help":
                ShowHelp();
                Environment.Exit(0);
                break;

            case "-d":
            case "--delimiter":
                delim = GetNextArg(arguments, ref i, "delimitador");
                break;

            case "-nh":
            case "--no-header":
                noHeader = true;
                break;

            case "-b":
            case "--by":
                string rawBy = GetNextArg(arguments, ref i, "campo de orden");
                sortFields.Add(ParseSortField(rawBy));
                break;

            case "-i":
            case "--input":
                input = GetNextArg(arguments, ref i, "archivo de entrada");
                break;

            case "-o":
            case "--output":
                output = GetNextArg(arguments, ref i, "archivo de salida");
                break;

            default:
                if (!arg.StartsWith("-"))
                {
                    if (input == null) input = arg;
                    else if (output == null) output = arg;
                }
                else
                {
                    throw new ArgumentException($"Opción desconocida: {arg}");
                }
                break;
        }
    }

    return new AppConfig(input, output, delim, noHeader, sortFields);
}

string GetNextArg(string[] args, ref int index, string nombre)
{
    if (index + 1 >= args.Length)
        throw new ArgumentException($"Falta valor para {nombre}");

    return args[++index];
}

SortField ParseSortField(string raw)
{
    string[] parts = raw.Split(':');

    if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
        throw new ArgumentException($"Campo inválido en -b: {raw}");

    string name = parts[0];
    bool isNumeric = parts.Length > 1 && parts[1].ToLower() == "num";
    bool isDesc = parts.Length > 2 && parts[2].ToLower() == "desc";

    return new SortField(name, isNumeric, isDesc);
}

void ShowHelp()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  sortx [input] [output] [opciones]");
    Console.WriteLine("");
    Console.WriteLine("Opciones:");
    Console.WriteLine("  -b campo:tipo:orden   Ej: edad:num:desc");
    Console.WriteLine("  -d delimitador        Ej: , | ;");
    Console.WriteLine("  -nh                   Sin cabecera");
}

// Definicion de Estructuras de Datos

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);