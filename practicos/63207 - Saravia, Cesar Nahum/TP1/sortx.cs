using System.Text;

try
{
    var config = ParseArgs(args);
    var text = ReadInput(config.InputFile);
    var (headers, rows) = ParseDelimited(text, config);
    var sorted = SortRows(rows, headers, config);
    var output = Serialize(headers, sorted, config);
    WriteOutput(config.OutputFile, output);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    string delimiter = ",";
    bool noHeader = false;
    var fields = new List<SortField>();
    var positional = new List<string>();

    for (int i = 0; i < args.Length;)
    {
        var arg = args[i];

        switch (arg)
        {
            case "-h":
            case "--help":
                PrintHelp();
                Environment.Exit(0);
                break;

            case "-i":
            case "--input":
                input = Next(args, ref i, arg);
                break;

            case "-o":
            case "--output":
                output = Next(args, ref i, arg);
                break;

            case "-d":
            case "--delimiter":
                delimiter = Next(args, ref i, arg);
                if (delimiter == "\\t") delimiter = "\t";
                break;

            case "-nh":
            case "--no-header":
                noHeader = true;
                i++;
                break;

            case "-b":
            case "--by":
                fields.Add(ParseSortField(Next(args, ref i, arg)));
                break;

            default:
                if (arg.StartsWith("-"))
                    throw new Exception($"Opción desconocida: {arg}");

                positional.Add(arg);
                i++;
                break;
        }
    }

    if (positional.Count >= 1 && input == null) input = positional[0];
    if (positional.Count >= 2 && output == null) output = positional[1];

    if (fields.Count == 0)
        throw new Exception("Debe indicar al menos un campo con -b");

    return new AppConfig(input, output, delimiter, noHeader, fields);
}

void PrintHelp()
{
    Console.WriteLine("Uso: sortx [input [output]] -b campo[:tipo[:orden]]");
}

string Next(string[] args, ref int i, string opt)
{
    i++;
    if (i >= args.Length)
        throw new Exception($"{opt} requiere valor");
    return args[i++];
}

SortField ParseSortField(string text)
{
    var parts = text.Split(':');

    string name = parts[0];
    bool numeric = parts.Length > 1 && parts[1] == "num";
    bool desc = parts.Length > 2 && parts[2] == "desc";

    return new SortField(name, numeric, desc);
}


string ReadInput(string? path)
{
    if (path == null)
        return Console.In.ReadToEnd();

    if (!File.Exists(path))
        throw new Exception($"Archivo no encontrado: {path}");

    return File.ReadAllText(path);
}

void WriteOutput(string? path, string content)
{
    if (path == null)
        Console.Write(content);
    else
        File.WriteAllText(path, content);
}


record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);
