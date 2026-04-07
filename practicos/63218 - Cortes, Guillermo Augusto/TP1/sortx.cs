using System;
using System.Collections.Generic;
using System.IO;

try
{
    var config = ParseArgs(args);

    var text = ReadInput(config);

    var rows = ParseDelimited(text, config);

    var sorted = SortRows(rows, config);

    var output = Serialize(sorted, config);

    WriteOutput(output, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

// FUNCIONES PRINCIPALES

AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();

    var positionals = new List<string>();

    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        switch (arg)
        {
            case "-i":
            case "--input":
                input = args[++i];
                break;

            case "-o":
            case "--output":
                output = args[++i];
                break;

            case "-d":
            case "--delimiter":
                delimiter = args[++i] == "\\t" ? "\t" : args[i];
                break;

            case "-nh":
            case "--no-header":
                noHeader = true;
                break;

            case "-h":
            case "--help":
                ShowHelp();
                Environment.Exit(0);
                break;

            case "-b":
            case "--by":
                var spec = args[++i];
                sortFields.Add(ParseSortField(spec));
                break;

            default:
                if (arg.StartsWith("-"))
                    throw new Exception($"Opción desconocida: {arg}");

                positionals.Add(arg);
                break;
        }
    }

    // Posicionales
    if (positionals.Count > 0 && input == null)
        input = positionals[0];

    if (positionals.Count > 1 && output == null)
        output = positionals[1];

    if (sortFields.Count == 0)
        throw new Exception("Debe especificar al menos un campo de ordenamiento con -b");

    return new AppConfig(input, output, delimiter, noHeader, sortFields);
}

// HELP

void ShowHelp()
{
    Console.WriteLine(@"
Uso:
  sortx [input [output]] -b campo[:tipo[:orden]]...

Opciones:
  -b, --by           Campo de ordenamiento
  -i, --input        Archivo de entrada
  -o, --output       Archivo de salida
  -d, --delimiter    Delimitador (default ,)
  -nh, --no-header   Sin encabezado
  -h, --help         Mostrar ayuda

Ejemplos:
  sortx empleados.csv -b apellido
  sortx empleados.csv -b salario:num:desc
");
}

// PARSE SORT FIELD

SortField ParseSortField(string spec)
{
    var parts = spec.Split(':');

    var name = parts[0];

    bool numeric = false;
    bool desc = false;

    if (parts.Length > 1)
    {
        numeric = parts[1] switch
        {
            "num" => true,
            "alpha" => false,
            _ => throw new Exception($"Tipo inválido: {parts[1]}")
        };
    }

    if (parts.Length > 2)
    {
        desc = parts[2] switch
        {
            "desc" => true,
            "asc" => false,
            _ => throw new Exception($"Orden inválido: {parts[2]}")
        };
    }

    return new SortField(name, numeric, desc);
}

    
record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);