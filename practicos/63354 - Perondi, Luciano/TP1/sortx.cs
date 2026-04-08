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
