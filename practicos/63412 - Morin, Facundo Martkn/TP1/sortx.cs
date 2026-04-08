
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]
using System;
using System.Collections.Generic;
using System.IO;
Console.WriteLine($"sortx {string.Join(" ", args)}");

try
{
    AppConfig ParseArgs(string[] args)
    {
        string? inputFile = null;
        string? outputFile = null;
        string delimiter = ",";
        bool noHeader = false;
        bool showHelp = false;
        List<SortField> sortFields = new();
        int positional = 0;

        SortField ParseSortField(string spec)
        {
            var parts = spec.Split(':');
            string name = parts[0];
            bool numeric = parts.Length > 1 && parts[1].Equals("num", StringComparison.OrdinalIgnoreCase);
            bool descending = parts.Length > 2 && parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
            return new SortField(name, numeric, descending);
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == "--ayuda" || arg == "-ay")
            {
                showHelp = true;
                continue;
            }
            if (arg == "--no-header" || arg == "-nh")
            {
                noHeader = true;
                continue;
            }
            if (arg == "--by" || arg == "-b")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"La opción '{arg}' requiere un valor.");
                sortFields.Add(ParseSortField(args[++i]));
                continue;
            }
            if (arg == "--input" || arg == "-i")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"La opción '{arg}' requiere un valor.");
                inputFile = args[++i];
                continue;
            }
            if (arg == "--output" || arg == "-o")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"La opción '{arg}' requiere un valor.");
                outputFile = args[++i];
                continue;
            }
            if (arg == "--delimiter" || arg == "-d")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"La opción '{arg}' requiere un valor.");
                string raw = args[++i];
                delimiter = raw == @"\t" ? "\t" : raw;
                continue;
            }
            if (!arg.StartsWith('-'))
            {
                if (positional == 0) { inputFile = arg; positional++; }
                else if (positional == 1) { outputFile = arg; positional++; }
                else throw new ArgumentException($"Argumento posicional inesperado: '{arg}'.");
                continue;
            }

            throw new ArgumentException($"Opción desconocida: '{arg}'.");
        }

        return new AppConfig(inputFile, outputFile, delimiter, noHeader, showHelp, sortFields);
    }


string ReadInput(AppConfig cfg)
{
    if (cfg.InputFile != null)
        return File.ReadAllText(cfg.InputFile);

    return Console.In.ReadToEnd();
}

}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error al procesar argumentos: {ex.Message}");
    return;
}





record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);