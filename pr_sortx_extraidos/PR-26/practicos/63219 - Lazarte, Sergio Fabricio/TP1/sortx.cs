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
    string? input = null;
    var sortFields = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "-b" || args[i] == "--by")
        {
            var parts = args[i + 1].Split(':');

            string name = parts[0];
            bool numeric = parts.Length > 1 && parts[1] == "num";
            bool desc = parts.Length > 2 && parts[2] == "desc";

            sortFields.Add(new SortField(name, numeric, desc));

            i++;
        }
        else
        {
            input = args[i];
        }
    }

    return new AppConfig(
        InputFile: input,
        OutputFile: null,
        Delimiter: ",",
        NoHeader: false,
        SortFields: sortFields
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
    if (config.SortFields.Count == 0)
        return rows;

    var field = config.SortFields[0];

    if (field.Numeric)
    {
        if (field.Descending)
        {
            return rows.OrderByDescending(r => double.Parse(r[field.Name])).ToList();
        }
        else
        {
            return rows.OrderBy(r => double.Parse(r[field.Name])).ToList();
        }
    }
    else
    {
        if (field.Descending)
        {
            return rows.OrderByDescending(r => r[field.Name]).ToList();
        }
        else
        {
            return rows.OrderBy(r => r[field.Name]).ToList();
        }
    }
}

string Serialize(List<Dictionary<string, string>> rows, AppConfig config)
{
    var lines = new List<string>();

    if (rows.Count == 0)
        return "";

    // Encabezado
    if (!config.NoHeader)
    {
        var headers = rows[0].Keys;
        lines.Add(string.Join(config.Delimiter, headers));
    }

    // Filas
    foreach (var row in rows)
    {
        lines.Add(string.Join(config.Delimiter, row.Values));
    }

    return string.Join("\n", lines);
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