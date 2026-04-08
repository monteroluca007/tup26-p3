
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

using System.Net;
using System.Security.AccessControl;
using System.Xml;
using Microsoft.VisualBasic.FileIO;

Console.WriteLine($"sortx {string.Join(" ", args)}");
AppConfig parseargs(string[] args)
{
    string? inputFile = null;
    string? outputFile = null;
    string delimiter = ",";
    bool help = false;
    bool noHeader = false;
    List<SortField> sortFields = new List<SortField>();
    int positional = 0;
    sortFields parseSoftfield(string spec)
    {
        var parts = spec.split(',');
        string name = parts[0];
        bool numeric = parts.length > 1 && parts[1].equals("num", StringComparison.OrdinalIgnoreCase);
        bool descending = parts.length > 2 && parts[2].equals("desc", StringComparison.OrdinalIgnoreCase);
        return new SortField(name, numeric, descending);
    }

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg == "-ay" || arg == "--ayuda") ;
        showhelp() = true;
        continue;
        if (args == --noheader || args == -nh) ;
        {
            noheader = true;
            continue;
        }
        if (arg == -by || arg == -b) ;
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '(arg)' requiere un valor.");
            SortField.Add(ParseSortField(args[++i]));
            continue;
        }
        if (arg == "--input" || arg == "-i")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '(arg)' requiere un valor.");
            inputFile = args[++i];
            continue;
        }
        if (arg == "--output" || arg == "-o")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '(arg)' requiere un valor.");
            outputFile = args[++i];
            continue;
        }
        if (arg == "--delimiter" || arg == "-d")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '(arg)' requiere un valor.");
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
    if (cfg.InputFile is not null)
        return File.ReadAllText(cfg.InputFile);
    return Console.In.ReadToEnd();
}

(List<Dictionary<string, string>> Rows, string[]? Headers) ParseDelimited(string text, AppConfig cfg)
    {
        var lines = text
            .Replace("\r\n", "\n")
            .Replace("\r",   "\n")
            .Split('\n')
            .Where(l => l.Length > 0)
            .ToArray();

        if (lines.Length == 0)
            return (new List<Dictionary<string, string>>(), null);

        string[]? headers;
        int       dataStart;

        if (!cfg.NoHeader)
        {
            headers   = lines[0].Split(cfg.Delimiter);
            dataStart = 1;
        }
        else
        {
            headers   = null;
            dataStart = 0;
        }

        var rows = new List<Dictionary<string, string>>();


        for (int lineIdx = dataStart; lineIdx < lines.Length; lineIdx++)
        {
            var values = lines[lineIdx].Split(cfg.Delimiter);
            var row    = new Dictionary<string, string>();

            if (!cfg.NoHeader && headers is not null)
            {
                for (int col = 0; col < headers.Length; col++)
                    row[headers[col]] = col < values.Length ? values[col] : string.Empty;
            }
            else
            {
                for (int col = 0; col < values.Length; col++)
                    row[col.ToString()] = values[col];
            }

            rows.Add(row);
        }

        // Validar que los campos de --by existen
        if (cfg.SortFields.Count > 0 && rows.Count > 0)
        {
            foreach (var sf in cfg.SortFields)
            {
                if (!rows[0].ContainsKey(sf.Name))
                {
                    string disponibles = string.Join(", ", rows[0].Keys);
                    throw new ArgumentException(
                        $"Campo '{sf.Name}' no existe. Columnas disponibles: {disponibles}");
                }
            }
        }
        return (rows, headers);
    }

List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> rows,List<SortField> sortFields)
    {
        if (sortFields.Count == 0 || rows.Count == 0)
            return rows;

        double NumKey(Dictionary<string, string> row, string name) =>
            double.TryParse(row[name],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double d) ? d : 0.0;

        SortField first = sortFields[0];

        IOrderedEnumerable<Dictionary<string, string>> ordered =
            (first.Numeric, first.Descending) switch
            {
                (true,  true)  => rows.OrderByDescending(r => NumKey(r, first.Name)),
                (true,  false) => rows.OrderBy(r => NumKey(r, first.Name)),
                (false, true)  => rows.OrderByDescending(r => r[first.Name],StringComparer.OrdinalIgnoreCase),
                (false, false) => rows.OrderBy(r => r[first.Name],StringComparer.OrdinalIgnoreCase),
            };

        for (int i = 1; i < sortFields.Count; i++)
        {
            SortField sf = sortFields[i];
            ordered = (sf.Numeric, sf.Descending) switch
            {
                (true,  true)  => ordered.ThenByDescending(r => NumKey(r, sf.Name)),
                (true,  false) => ordered.ThenBy(r => NumKey(r, sf.Name)),
                (false, true)  => ordered.ThenByDescending(r => r[sf.Name],StringComparer.OrdinalIgnoreCase),
                (false, false) => ordered.ThenBy(r => r[sf.Name],StringComparer.OrdinalIgnoreCase),
            };
        }
        return ordered.ToList();
    }









record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);