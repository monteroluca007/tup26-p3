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
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    return new AppConfig(
        InputFile: args.Length > 0 ? args[0] : null,
        OutputFile: null,
        Delimiter: ",",
        NoHeader: false,
        SortFields: new List<SortField>()
    );
}

string ReadInput(AppConfig config)
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

List<Dictionary<string, string>> ParseDelimited(string input, AppConfig config)
{
    var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    var rows = new List<Dictionary<string, string>>();

    if (lines.Length == 0)
        return rows;

    // Obtener encabezados
    string[] headers;

    int startIndex = 0;

    if (!config.NoHeader)
    {
        headers = lines[0].Trim().Split(config.Delimiter);
        startIndex = 1;
    }
    else
    {
        var columnCount = lines[0].Split(config.Delimiter).Length;
        headers = Enumerable.Range(0, columnCount)
                            .Select(i => i.ToString())
                            .ToArray();
    }

    // Procesar filas
    for (int i = startIndex; i < lines.Length; i++)
    {
        var values = lines[i].Trim().Split(config.Delimiter);

        var dict = new Dictionary<string, string>();

        for (int j = 0; j < headers.Length; j++)
        {
            var key = headers[j];
            var value = j < values.Length ? values[j] : "";

            dict[key] = value;
        }

        rows.Add(dict);
    }

    return rows;
}

List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> rows, AppConfig config)
{
    return rows;
}

string Serialize(List<Dictionary<string, string>> rows, AppConfig config)
{
    foreach (var row in rows)
    {
        Console.WriteLine(string.Join(" | ", row.Values));
    }

    return "";
}

void WriteOutput(string output, AppConfig config)
{
    Console.WriteLine(output);
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);