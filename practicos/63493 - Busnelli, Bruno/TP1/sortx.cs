
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

            default:
                throw new Exception($"Argumento desconocido: {arg}");
        }
        i++;
    }

    return new AppConfig(input, output, delimiter, noHeader, sortFields);
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