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
}

AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
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

            case "-b":
            case "--by":
                var parts = args[++i].Split(':');
                string name = parts[0];
                bool numeric = parts.Length > 1 && parts[1] == "num";
                bool desc = parts.Length > 2 && parts[2] == "desc";

                sortFields.Add(new SortField(name, numeric, desc));
                break;
        }
    }

    return new AppConfig(input, output, delimiter, noHeader, sortFields);
}
string ReadInput(AppConfig config) => throw new NotImplementedException();
List<Dictionary<string,string>> ParseDelimited(string text, AppConfig config) => throw new NotImplementedException();
List<Dictionary<string,string>> SortRows(List<Dictionary<string,string>> rows, AppConfig config) => throw new NotImplementedException();
string Serialize(List<Dictionary<string,string>> rows, AppConfig config) => throw new NotImplementedException();
void WriteOutput(string output, AppConfig config) => throw new NotImplementedException();

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);
