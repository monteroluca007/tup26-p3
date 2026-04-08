using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

try
{
    var config = ParseArgs(args);
    var input = ReadInput(config);
    var parsed = ParseDelimited(input, config);
    var sorted = SortRows(parsed.Rows, config);
    var output = Serialize(parsed.Header, sorted, config);
    WriteOutput(output, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] arguments)
{
    string? inputFile = null;
    string? outputFile = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();
    var positionalArgs = new List<string>();

    for (int i = 0; i < arguments.Length; i++)
    {
        string arg = arguments[i];
        if (arg == "-h" || arg == "--help")
        {
            Console.WriteLine("Uso: sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...");
            Environment.Exit(0);
        }
        else if (arg == "-i" || arg == "--input") inputFile = arguments[++i];
        else if (arg == "-o" || arg == "--output") outputFile = arguments[++i];
        else if (arg == "-d" || arg == "--delimiter") delimiter = arguments[++i] == "\\t" ? "\t" : arguments[i];
        else if (arg == "-nh" || arg == "--no-header") noHeader = true;
        else if (arg == "-b" || arg == "--by")
        {
            var parts = arguments[++i].Split(':');
            sortFields.Add(new SortField(parts[0], parts.Length > 1 && parts[1] == "num", parts.Length > 2 && parts[2] == "desc"));
        }
        else if (arg.StartsWith("-")) throw new Exception($"Opción desconocida: {arg}");
        else positionalArgs.Add(arg);
    }

    if (inputFile == null && positionalArgs.Count > 0) inputFile = positionalArgs[0];
    if (outputFile == null && positionalArgs.Count > 1) outputFile = positionalArgs[1];

    return new AppConfig(inputFile, outputFile, delimiter, noHeader, sortFields);
}