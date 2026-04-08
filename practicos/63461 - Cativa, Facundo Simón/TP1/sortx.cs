using System;
using System.Collections.Generic;
using System.IO;

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
    string? input = null;
    string? output = null;

    if (args.Length > 0)
        input = args[0];

    if (args.Length > 1)
        output = args[1];

    return new AppConfig(
        input,
        output,
        ",",
        false,
        new List<SortField>()
    );
}

   static string ReadInput(AppConfig config)
{
    if (!string.IsNullOrEmpty(config.InputFile))
    {
        return File.ReadAllText(config.InputFile);
    }
    else
    {
        return Console.In.ReadToEnd();
    }
}

   static List<Dictionary<string, string>> ParseDelimited(string text, AppConfig config)
{
    var rows = new List<Dictionary<string, string>>();
    var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    if (lines.Length == 0)
        return rows;

    var delimiter = config.Delimiter;
    string[] headers;

    int startIndex = 0;

    if (!config.NoHeader)
    {
        headers = lines[0].Trim().Split(delimiter);
        startIndex = 1;
    }
    else
    {
        var firstLine = lines[0].Trim().Split(delimiter);
        headers = new string[firstLine.Length];

        for (int i = 0; i < headers.Length; i++)
            headers[i] = i.ToString();
    }

    for (int i = startIndex; i < lines.Length; i++)
    {
        var values = lines[i].Trim().Split(delimiter);
        var dict = new Dictionary<string, string>();

        for (int j = 0; j < headers.Length && j < values.Length; j++)
        {
            dict[headers[j]] = values[j];
        }

        rows.Add(dict);
    }

    return rows;
}

    static List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> rows, AppConfig config)
{
    if (config.SortFields.Count == 0)
        return rows;

    var field = config.SortFields[0]; // primer campo de la lista

    rows.Sort((a, b) =>
    {
        var valA = a.ContainsKey(field.Name) ? a[field.Name] : "";
        var valB = b.ContainsKey(field.Name) ? b[field.Name] : "";

        int result;

        if (field.Numeric)
        {
            double numA = double.TryParse(valA, out var na) ? na : 0;
            double numB = double.TryParse(valB, out var nb) ? nb : 0;
            result = numA.CompareTo(numB);
        }
        else
        {
            result = string.Compare(valA, valB, StringComparison.OrdinalIgnoreCase);
        }

        return field.Descending ? -result : result;
    });

    return rows;
}

    static string Serialize(List<Dictionary<string, string>> rows, AppConfig config)
{
    if (rows.Count == 0)
        return "";

    var delimiter = config.Delimiter;
    var headers = new List<string>(rows[0].Keys);

    var lines = new List<string>();

    if (!config.NoHeader)
    {
        lines.Add(string.Join(delimiter, headers));
    }

    foreach (var row in rows)
    {
        var values = new List<string>();

        foreach (var h in headers)
        {
            values.Add(row.ContainsKey(h) ? row[h] : "");
        }

        lines.Add(string.Join(delimiter, values));
    }

    return string.Join("\n", lines);
}

   static void WriteOutput(string text, AppConfig config)
{
    if (!string.IsNullOrEmpty(config.OutputFile))
    {
        File.WriteAllText(config.OutputFile, text);
    }
    else
    {
        Console.WriteLine(text);
    }
}
}