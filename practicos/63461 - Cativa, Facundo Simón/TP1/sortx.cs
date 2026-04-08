using System;
using System.Collections.Generic;

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);

class Program
{
    static void Main(string[] args)
    {
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
            Console.Error.WriteLine(ex.Message);
        }
    }

    static AppConfig ParseArgs(string[] args)
    {
        return new AppConfig(null, null, ",", false, new List<SortField>());
    }

    static string ReadInput(AppConfig config)
    {
        return "";
    }

    static List<Dictionary<string, string>> ParseDelimited(string text, AppConfig config)
    {
        return new List<Dictionary<string, string>>();
    }

    static List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> rows, AppConfig config)
    {
        return rows;
    }

    static string Serialize(List<Dictionary<string, string>> rows, AppConfig config)
    {
        return "";
    }

    static void WriteOutput(string text, AppConfig config)
    {
        Console.WriteLine(text);
    }
}