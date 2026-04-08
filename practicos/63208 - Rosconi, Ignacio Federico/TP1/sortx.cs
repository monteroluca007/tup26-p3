
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");

using System.Text;

record SortField(string Name, bool IsNumeric, bool IsDescending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields,
    bool HelpRequested
);

try
{
    var config = ParseArgs(args);

    if (config.HelpRequested)
    {
        PrintHelp();
        return;
    }

    var raw = ReadInput(config);
    var parsed = ParseDelimited(raw, config);
    var sorted = SortRows(parsed.Headers, parsed.Rows, config);
    var text = Serialize(parsed.Headers, sorted, config);
    WriteOutput(text, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

// ===== FUNCIONES (VACÍAS POR AHORA) =====

AppConfig ParseArgs(string[] args)
{
    throw new NotImplementedException();
}

string ReadInput(AppConfig config)
{
    throw new NotImplementedException();
}

(List<string> Headers, List<List<string>> Rows)
ParseDelimited(string text, AppConfig config)
{
    throw new NotImplementedException();
}

List<List<string>> SortRows(
    List<string> headers,
    List<List<string>> rows,
    AppConfig config)
{
    throw new NotImplementedException();
}

string Serialize(
    List<string> headers,
    List<List<string>> rows,
    AppConfig config)
{
    throw new NotImplementedException();
}

void WriteOutput(string text, AppConfig config)
{
    throw new NotImplementedException();
}

void PrintHelp()
{
    throw new NotImplementedException();
}

static AppConfig ParseArgs(string[] args)
{
    string? input = null;
    string? output = null;
    bool descending = false;
    List<SortField> sortFields = new();

    foreach (var arg in args)
    {
        if (arg.StartsWith("--input="))
            input = arg.Replace("--input=", "");

        else if (arg.StartsWith("--output="))
            output = arg.Replace("--output=", "");

        else if (arg == "--desc")
            descending = true;
    }

    if (input == null || output == null)
        throw new ArgumentException("Debe especificar --input y --output");

    return new AppConfig(input, output, descending, sortFields);
}

static List<string> ReadInput(string inputPath)
{
    if (!File.Exists(inputPath))
        throw new FileNotFoundException("Archivo de entrada no encontrado.");

    return File.ReadAllLines(inputPath).ToList();
}

static void WriteOutput(string outputPath, List<string> lines)
{
    File.WriteAllLines(outputPath, lines);
}

static List<string> SortData(List<string> lines, AppConfig config)
{
    if (config.SortFields.Count == 0)
        return lines;

    var sortField = config.SortFields[0];

    if (sortField.Descending)
    {
        return lines
            .OrderByDescending(line => line)
            .ToList();
    }
    else
    {
        return lines
            .OrderBy(line => line)
            .ToList();
    }
}

static void PrintHelp()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  sortx --input=archivo.txt --output=salida.txt [--desc]");
    Console.WriteLine();
    Console.WriteLine("Opciones:");
    Console.WriteLine("  --input=RUTA      Archivo de entrada");
    Console.WriteLine("  --output=RUTA     Archivo de salida");
    Console.WriteLine("  --desc            Orden descendente");
    Console.WriteLine("  -h, --help        Mostrar ayuda");
}