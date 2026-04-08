using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

try
{
    var config = ParseArgs(args);
    if (config == null) return 0;

    string inputText = ReadInput(config);
    var (header, rows) = ParseDelimited(inputText, config);
    var sortedRows = SortRows(rows, config);
    string outputText = Serialize(header, sortedRows, config);
    WriteOutput(outputText, config);

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

AppConfig? ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();
    int positionalIndex = 0;

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg == "-h" || arg == "--help")
        {
            Console.WriteLine("Uso: sortx [input [output]] [-b|--by campo[:tipo[:orden]]]... [-i|--input] [-o|--output] [-d|--delimiter] [-nh|--no-header] [-h|--help]");
            return null;
        }
        else if (arg == "-d" || arg == "--delimiter")
        {
            string d = args[++i];
            delimiter = d == "\\t" ? "\t" : d;
        }
        else if (arg == "-nh" || arg == "--no-header")
        {
            noHeader = true;
        }
        else if (arg == "-i" || arg == "--input")
        {
            input = args[++i];
        }
        else if (arg == "-o" || arg == "--output")
        {
            output = args[++i];
        }
        else if (arg == "-b" || arg == "--by")
        {
            string[] parts = args[++i].Split(':');
            string name = parts[0];
            bool isNum = parts.Length > 1 && parts[1].ToLower() == "num";
            bool isDesc = parts.Length > 2 && parts[2].ToLower() == "desc";
            sortFields.Add(new SortField(name, isNum, isDesc));
        }
        else if (!arg.StartsWith("-"))
        {
            // Resolución de argumentos posicionales
            if (positionalIndex == 0 && input == null) input = arg;
            else if (positionalIndex == 1 && output == null) output = arg;
            positionalIndex++;
        }
    }

    string ReadInput(AppConfig config)
{
    if (!string.IsNullOrEmpty(config.InputFile))
    {
        if (!File.Exists(config.InputFile))
            throw new Exception($"El archivo de entrada no existe: {config.InputFile}");
        return File.ReadAllText(config.InputFile);
    }

    // Si no hay archivo definido por argumentos, intentamos leer desde la consola (stdin)
    if (Console.IsInputRedirected)
        return Console.In.ReadToEnd();

    throw new Exception("No se especificó archivo de entrada ni se detectó un flujo de texto en stdin.");
}

(string[]? Header, List<Dictionary<string, string>> Rows) ParseDelimited(string text, AppConfig config)
{
    var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    if (lines.Length == 0) return (null, new List<Dictionary<string, string>>());

    string[]? header = null;
    int startRow = 0;

    if (!config.NoHeader)
    {
        header = lines[0].Split(config.Delimiter);
        startRow = 1; 
    }

    var rows = new List<Dictionary<string, string>>();
    for (int i = startRow; i < lines.Length; i++)
    {
        var values = lines[i].Split(config.Delimiter);
        var rowDict = new Dictionary<string, string>();

        for (int j = 0; j < values.Length; j++)
        {
            string key = config.NoHeader ? j.ToString() : header![j];
            rowDict[key] = values[j];
        }
        rows.Add(rowDict);
    }

    return (header, rows);
}