try
{
    AppConfig config                        = ParseArgs(args);                                              // Paso 1
    string rawText                          = ReadInput(config.InputFile);                                  // Paso 2
    var (headers, rows)                     = ParseDelimited(rawText, config.Delimiter, config.NoHeader);   // Paso 3
    List<Dictionary<string, string>> sorted = SortRows(rows, headers, config.SortFields);                  // Paso 4
    string output                           = Serialize(sorted, headers, config.Delimiter, config.NoHeader);// Paso 5
    WriteOutput(config.OutputFile, output);                                                                 // Paso 6
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine("Use --help para ver las opciones.");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    string?         inputFile  = null;
    string?         outputFile = null;
    string          delimiter  = ",";
    bool            noHeader   = false;
    var             sortFields = new List<SortField>();
    var             positional = new List<string>();
    int i = 0;

    while (i < args.Length)
    {
        string arg = args[i];
        switch (arg)
        {
            case "--help" or "-h":
                PrintHelp();
                Environment.Exit(0);
                break;

            case "--input" or "-i":
                inputFile = Next(args, ref i, arg);
                break;

            case "--output" or "-o":
                outputFile = Next(args, ref i, arg);
                break;

            case "--delimiter" or "-d":
                delimiter = ProcessDelimiter(Next(args, ref i, arg));
                break;

            case "--no-header" or "-nh":
                noHeader = true;
                i++;
                break;

            case "--by" or "-b":
                sortFields.Add(ParseSortField(Next(args, ref i, arg)));
                break;

            default:
                if (arg.StartsWith('-'))
                    throw new Exception($"Opción desconocida: '{arg}'. Use --help.");
                positional.Add(arg);
                i++;
                break;
        }
    }

    if (positional.Count >= 1 && inputFile  == null) inputFile  = positional[0];
    if (positional.Count >= 2 && outputFile == null) outputFile = positional[1];
    if (positional.Count > 2)
        throw new Exception("Demasiados argumentos posicionales. Máximo: [input] [output].");

    return new AppConfig(inputFile, outputFile, delimiter, noHeader, sortFields);
}

string Next(string[] args, ref int i, string option)
{
    i++;
    if (i >= args.Length)
        throw new Exception($"'{option}' requiere un valor.");
    return args[i++];
}

string ProcessDelimiter(string d) => d switch
{
    "\\t" => "\t",
    "\\n" => "\n",
    "\\|" => "|",
    _     => d
};

SortField ParseSortField(string spec)
{
    string[] parts = spec.Split(':');
    string name    = parts[0].Trim();

    if (string.IsNullOrWhiteSpace(name))
        throw new Exception($"Especificación inválida: '{spec}'. El nombre/índice no puede estar vacío.");

    if (parts.Length >= 2)
    {
        string tipo = parts[1].Trim().ToLower();
        if (tipo != "alpha" && tipo != "num")
            throw new Exception($"Tipo inválido en '{spec}'. Use 'alpha' o 'num'.");
    }

    if (parts.Length >= 3)
    {
        string orden = parts[2].Trim().ToLower();
        if (orden != "asc" && orden != "desc")
            throw new Exception($"Orden inválido en '{spec}'. Use 'asc' o 'desc'.");
    }

    bool numeric    = parts.Length >= 2 && parts[1].Trim().Equals("num",  StringComparison.OrdinalIgnoreCase);
    bool descending = parts.Length >= 3 && parts[2].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);

    return new SortField(name, numeric, descending);
}

string ReadInput(string? filePath)
{
    if (filePath == null)
        return Console.In.ReadToEnd();

    if (!File.Exists(filePath))
        throw new FileNotFoundException($"Archivo no encontrado: '{filePath}'");

    return File.ReadAllText(filePath);
}
