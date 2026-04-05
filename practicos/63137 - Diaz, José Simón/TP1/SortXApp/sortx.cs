try
{
    AppConfig appConfig = ParseArgs(args);

    string rawInputText = ReadInput(appConfig.InputFile);

    var parsedData = ParseDelimited(rawInputText, appConfig.Delimiter, appConfig.NoHeader);

    var sortedRows = SortRows(parsedData, appConfig.SortFields, appConfig.NoHeader);
    string serializedOutput = Serialize((sortedRows, parsedData.Headers), appConfig.Delimiter, appConfig.NoHeader);

    WriteOutput(serializedOutput, appConfig.OutputFile);
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Error: {exception.Message}");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    bool helpRequested = args.Any(argument => IsFlag(argument, Flags.Help));

    if (args.Length == 0 || helpRequested)
    {
        PrintHelp();
        Environment.Exit(0);
    }

    string? inputFilePath = null;
    string? outputFilePath = null;
    string delimiter = Defaults.DefaultDelimiter;
    bool noHeader = false;
    List<SortField> sortFields = new();

    List<string> positionalArguments = args
        .TakeWhile(argument => !argument.StartsWith("-"))
        .ToList();

    if (positionalArguments.Count >= 1) inputFilePath = positionalArguments[0];
    if (positionalArguments.Count >= 2) outputFilePath = positionalArguments[1];

    for (int currentIndex = positionalArguments.Count; currentIndex < args.Length; currentIndex++)
    {
        string currentArgument = args[currentIndex];

        switch (currentArgument)
        {
            case var _ when IsFlag(currentArgument, Flags.By):
                sortFields.Add(ParseSortField(RequireNextArg(args, currentIndex, currentArgument)));
                currentIndex++;
                break;

            case var _ when IsFlag(currentArgument, Flags.Input):
                inputFilePath = RequireNextArg(args, currentIndex, currentArgument);
                currentIndex++;
                break;

            case var _ when IsFlag(currentArgument, Flags.Output):
                outputFilePath = RequireNextArg(args, currentIndex, currentArgument);
                currentIndex++;
                break;

            case var _ when IsFlag(currentArgument, Flags.Delimiter):
                string rawDelimiter = RequireNextArg(args, currentIndex, currentArgument);
                delimiter = rawDelimiter == Escapes.Tab ? "\t" : rawDelimiter;
                currentIndex++;
                break;

            case var _ when IsFlag(currentArgument, Flags.NoHeader):
                noHeader = true;
                break;

            default:
                throw new ArgumentException($"Opción desconocida: '{currentArgument}'");
        }
    }

    return new AppConfig(inputFilePath, outputFilePath, delimiter, noHeader, sortFields);
}

string ReadInput(string? inputFilePath)
{
    if (inputFilePath is null)
        return Console.In.ReadToEnd();

    if (!File.Exists(inputFilePath))
        throw new FileNotFoundException($"El archivo de entrada no existe: '{inputFilePath}'");

    return File.ReadAllText(inputFilePath);
}

(List<Dictionary<string, string>> Rows, List<string> Headers) ParseDelimited(
    string rawText,
    string delimiter,
    bool noHeader)
{
    List<string[]> allLines = rawText
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.TrimEnd('\r').Split(delimiter))
        .ToList();

    if (allLines.Count == 0)
        throw new InvalidDataException("El archivo de entrada está vacío.");

    List<string> headers = noHeader
        ? Enumerable.Range(0, allLines[0].Length).Select(index => index.ToString()).ToList()
        : allLines[0].ToList();

    IEnumerable<string[]> dataLines = noHeader ? allLines : allLines.Skip(1);

    List<Dictionary<string, string>> rows = dataLines
        .Select(fields => MapFieldsToHeaders(fields, headers))
        .ToList();

    return (rows, headers);
}

List<Dictionary<string, string>> SortRows(
    (List<Dictionary<string, string>> Rows, List<string> Headers) parsedData,
    List<SortField> sortFields,
    bool noHeader)
{
    if (sortFields.Count == 0)
        return parsedData.Rows;

    ValidateSortFields(sortFields, parsedData.Headers, noHeader);

    IOrderedEnumerable<Dictionary<string, string>> orderedRows =
        ApplyPrimarySort(parsedData.Rows, sortFields[0]);

    foreach (SortField sortField in sortFields.Skip(1))
        orderedRows = ApplySecondarySort(orderedRows, sortField);

    return orderedRows.ToList();
}

string Serialize(
    (List<Dictionary<string, string>> Rows, List<string> Headers) sortedData,
    string delimiter,
    bool noHeader)
{
    List<string> outputLines = new();

    if (!noHeader)
        outputLines.Add(string.Join(delimiter, sortedData.Headers));

    foreach (var row in sortedData.Rows)
    {
        var orderedValues = sortedData.Headers.Select(header => row[header]);
        outputLines.Add(string.Join(delimiter, orderedValues));
    }

    return string.Join(Environment.NewLine, outputLines);
}

void WriteOutput(string content, string? outputFilePath)
{
    if (outputFilePath is null)
        Console.WriteLine(content);
    else
        File.WriteAllText(outputFilePath, content);
}

bool IsFlag(string argument, params string[] possibleForms)
{
    return possibleForms.Any(form =>
        form.Equals(argument, StringComparison.OrdinalIgnoreCase));
}

string RequireNextArg(string[] args, int currentIndex, string flag)
{
    int nextIndex = currentIndex + 1;

    if (nextIndex >= args.Length)
        throw new ArgumentException($"La opción '{flag}' requiere un valor.");

    return args[nextIndex];
}

SortField ParseSortField(string fieldSpecification)
{
    string[] parts = fieldSpecification.Split(':');

    string fieldName = parts[0];

    string comparisonType = parts.Length >= 2
        ? parts[1].ToLower()
        : SortTokens.Alpha;

    string sortOrder = parts.Length >= 3
        ? parts[2].ToLower()
        : SortTokens.Asc;

    bool isNumeric = comparisonType == SortTokens.Numeric;
    bool isDescending = sortOrder == SortTokens.Desc;

    return new SortField(fieldName, isNumeric, isDescending);
}

Dictionary<string, string> MapFieldsToHeaders(string[] fields, List<string> headers)
{
    var row = new Dictionary<string, string>();

    for (int columnIndex = 0; columnIndex < headers.Count; columnIndex++)
    {
        string value = columnIndex < fields.Length ? fields[columnIndex].Trim() : string.Empty;
        row[headers[columnIndex]] = value;
    }

    return row;
}

void ValidateSortFields(List<SortField> sortFields, List<string> headers, bool noHeader)
{
    foreach (SortField sortField in sortFields)
    {
        if (!headers.Contains(sortField.Name))
        {
            string context = noHeader ? "índice de columna" : "nombre de columna";
            throw new ArgumentException($"El campo '{sortField.Name}' no existe como {context}.");
        }
    }
}

IOrderedEnumerable<Dictionary<string, string>> ApplyPrimarySort(
    List<Dictionary<string, string>> rows,
    SortField sortField)
{
    return sortField.Descending
        ? rows.OrderByDescending(row => ExtractSortKey(row, sortField))
        : rows.OrderBy(row => ExtractSortKey(row, sortField));
}

IOrderedEnumerable<Dictionary<string, string>> ApplySecondarySort(
    IOrderedEnumerable<Dictionary<string, string>> orderedRows,
    SortField sortField)
{
    return sortField.Descending
        ? orderedRows.ThenByDescending(row => ExtractSortKey(row, sortField))
        : orderedRows.ThenBy(row => ExtractSortKey(row, sortField));
}

IComparable ExtractSortKey(Dictionary<string, string> row, SortField sortField)
{
    string rawValue = row.TryGetValue(sortField.Name, out string? value)
        ? value
        : string.Empty;

    if (!sortField.Numeric)
        return rawValue;

    return double.TryParse(rawValue, out double parsedNumber)
        ? parsedNumber
        : double.MinValue;
}

void PrintHelp()
{
    Console.WriteLine("""
        sortx — Ordenador de archivos de texto delimitados

        USO:
          sortx [input [output]] [-b campo[:tipo[:orden]]]... [opciones]
        """);
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);

static class SortTokens
{
    public const string Alpha = "alpha";
    public const string Numeric = "num";
    public const string Asc = "asc";
    public const string Desc = "desc";
}

static class Defaults
{
    public const string DefaultDelimiter = ",";
}

static class Escapes
{
    public const string Tab = @"\t";
}

static class Flags
{
    public static readonly string[] By = ["-b", "--by"];
    public static readonly string[] Input = ["-i", "--input"];
    public static readonly string[] Output = ["-o", "--output"];
    public static readonly string[] Delimiter = ["-d", "--delimiter"];
    public static readonly string[] NoHeader = ["-nh", "--no-header"];
    public static readonly string[] Help = ["-h", "--help"];
}