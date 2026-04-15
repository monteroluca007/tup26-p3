using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

try
{
    var config = ParseArgs(args);
    string input = ReadInput(config.InputFile);
    var rows = ParseDelimited(input, config.Delimiter, config.NoHeader, out var headers);
    var sortedRows = SortRows(rows, config.SortFields);
    string output = Serialize(sortedRows, headers, config.Delimiter, config.NoHeader);
    WriteOutput(config.OutputFile, output);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();
    var positionalArgs = new List<string>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-h":
            case "--help":
                PrintHelp();
                Environment.Exit(0);
                break;
            case "-nh":
            case "--no-header":
                noHeader = true;
                break;
            case "-d":
            case "--delimiter":
                if (i + 1 >= args.Length) throw new Exception("Falta valor para el delimitador.");
                delimiter = args[++i];
                if (delimiter == "\\t") delimiter = "\t";
                break;
            case "-i":
            case "--input":
                if (i + 1 >= args.Length) throw new Exception("Falta valor para input.");
                input = args[++i];
                break;
            case "-o":
            case "--output":
                if (i + 1 >= args.Length) throw new Exception("Falta valor para output.");
                output = args[++i];
                break;
            case "-b":
            case "--by":
                if (i + 1 >= args.Length) throw new Exception("Falta valor para --by.");
                string[] parts = args[++i].Split(':');
                string name = parts[0];
                bool numeric = parts.Length > 1 && parts[1].ToLower() == "num";
                bool descending = parts.Length > 2 && parts[2].ToLower() == "desc";
                sortFields.Add(new SortField(name, numeric, descending));
                break;
            default:
                positionalArgs.Add(args[i]);
                break;
        }
    }

    if (input == null && positionalArgs.Count > 0) input = positionalArgs[0];
    if (output == null && positionalArgs.Count > 1) output = positionalArgs[1];

    return new AppConfig(input, output, delimiter, noHeader, sortFields);

    void PrintHelp()
    {
        Console.WriteLine("Uso: sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...");
        Console.WriteLine("Opciones:");
        Console.WriteLine("  -b, --by          Campo por el que ordenar (ej. salario:num:desc).");
        Console.WriteLine("  -i, --input       Archivo de entrada (default: stdin).");
        Console.WriteLine("  -o, --output      Archivo de salida (default: stdout).");
        Console.WriteLine("  -d, --delimiter   Carácter delimitador (default: ','). Usar '\\t' para tabulación.");
        Console.WriteLine("  -nh, --no-header  Indica que el archivo no tiene fila de encabezado.");
        Console.WriteLine("  -h, --help        Muestra esta ayuda.");
    }
}
