
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

try
{
    var config = ParseArgs(args);
    var input = ReadInput(config);
    var rows = ParseDelimited(input, config);
    var sorted = SortRows(rows, config);
    var output = Serialize(sorted, config);
    WriteOutput(output, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}

//ParseArgs
AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();

    int i = 0;
    if (i < args.Length && !args[i].StartsWith("-"))
    {
        input = args[i++];
    }
    if (i < args.Length && !args[i].StartsWith("-"))
    {
        output = args[i++];
    }

    while (i < args.Length)
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
                var d = args[++i];A
                delimiter = d == "\\t" ? "\t" : d;
                break;

            case "-nh":
            case "--no-header":
                noHeader = true;
                break;
            case "-b":
            case "--by":
                var value = args[++i];
                var parts = value.Split(':');

                string name = parts[0];
                bool numeric = parts.Length > 1 && parts[1] == "num";
                bool descending = parts.Length > 2 && parts[2] == "desc";

        sortFields.Add(new SortField(name, numeric, descending));
                break;
            case "-h":
            case "--help":
        ShowHelp();
            Environment.Exit(0);
                break;

            default:
                throw new Exception($"Argumento desconocido: {arg}");
        }
        i++;
    }

    return new AppConfig(input, output, delimiter, noHeader, sortFields);
}

void ShowHelp()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("sortx [input [output]] [opciones]");
    Console.WriteLine("");
    Console.WriteLine("Opciones:");
    Console.WriteLine("-b, --by campo[:tipo[:orden]]");
    Console.WriteLine("-i, --input archivo");
    Console.WriteLine("-o, --output archivo");
    Console.WriteLine("-d, --delimiter delimitador");
    Console.WriteLine("-nh, --no-header");
    Console.WriteLine("-h, --help");
}

string ReadInput(AppConfig config)
{
    if (!string.IsNullOrEmpty(config.InputFile))
    {
        return File.ReadAllText(config.InputFile);
    }

    return Console.In.ReadToEnd();
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);