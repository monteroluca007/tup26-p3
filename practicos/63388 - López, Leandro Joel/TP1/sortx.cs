// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");


try{

    //Ejercicio 1

    AppConfig ParseArgs (string[] args){

        string? entrada = null;
        string? salida = null;
        string delimitador = ",";
        bool noHeader = false;
        bool ayuda = false;
        List<SortField> sortFields = new ();
        int positional = 0;

        SortField ParseSortFields(string spec){

            var partes = spec.Split(':');
            string nombre = partes[0];
            bool numerico = partes.Length > 1 && partes[1].Equals("num", StringComparison.OrdinalIgnoreCase);
            bool descender = partes.Length > 2 && partes[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
            return new SortField (nombre, numerico, descender);
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == "--ayuda" || arg == "-ay")
            {
                ayuda = true;
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
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                sortFields.Add(ParseSortFields(args[++i]));
                continue;
            }
            if (arg == "--entrada" || arg == "-en")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                entrada = args[++i];
                continue;
            }
            if (arg == "--salida" || arg == "-sa")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                salida = args[++i];
                continue;
            }
            if (arg == "--delimitador" || arg == "-de")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                string raw = args[++i];
                delimitador = raw == @"\t" ? "\t" : raw;
                continue;
            }
            if (!arg.StartsWith('-'))
            {
                if (positional == 0) {entrada = arg; positional++; }
                else if (positional == 1) {salida = arg; positional++; }
                else throw new ArgumentException($"Argumento posicional inesperado: '{arg}'.");
                continue;
            }

            throw new ArgumentException($"Opcion desconocida: '{arg}'.");

        }

        return new AppConfig(entrada, salida, delimitador, noHeader, ayuda, sortFields);

    }

    //Ejercicio 2
    
    string ReadEntrada(AppConfig cfg)
    {
        if (cfg.entrada is not null)
        return File.ReadAllText(cfg.entrada);
        return Console.In.ReadToEnd();
    }


    //Ejercicio 3

    (List<Dictionary<string, string>> Rows, string[]? Headers) ParseDelimited(string text, AppConfig cfg)
    {
        var linea = text
        .Replace("\r\n", "\n")
        .Replace("\r",   "\n")
        .Split('\n')
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToArray();

        if (linea.Length ==0)
        return (new List<Dictionary<string, string>>(), null);

        string[]? headers;
        int dataStart;

        if (!cfg.NoHeader)
        {
            headers = linea[0].Split(cfg.Delimitador);
            dataStart = 1;
        }
        else
        {
            headers = null;
            dataStart = 0;
        }

        var rows = new List<Dictionary<string, string>>();

        for (int lineaIdx = dataStart; lineaIdx < linea.Length; lineaIdx++)
        {
            var values = linea[lineaIdx].Split(cfg.Delimitador);
            var row = new Dictionary<string, string>();

            if (!cfg.NoHeader && headers is not null)
            {
                for (int col = 0; col < headers.Length; col++)
                row[headers[col]] = col < values.Length ? values[col] : string.Empty;
            }
            else
            {
                for ( int col = 0; col < values.Length; col++)
                row[col.ToString()] = values[col];
            }

            rows.Add(row);
        }

        if (cfg.SortFields.Count > 0 && rows.Count> 0)
        {
            foreach (var sf in cfg.SortFields)
            {
                if (!rows[0].ContainsKey(sf.Nombre))
                {
                    string disponibles = string.Join(", ", rows[0].Keys);
                    throw new ArgumentException($"campo '{sf.Nombre}' no existe. Columnas disponibles: {disponibles}");
                }
            }
        }

        return (rows, headers);
    }

    //Ejercicio 4

List<Dictionary<string, string>> SortRows(
List<Dictionary<string, string>> rows,
List<SortField> sortFields)
{
if (sortFields.Count == 0 || rows.Count == 0)
return rows;

double NumKey(Dictionary<string, string> row, string nombre) =>
double.TryParse(row.ContainsKey(nombre) ? row[nombre] : "",
System.Globalization.NumberStyles.Any,
System.Globalization.CultureInfo.InvariantCulture,
 out double d) ? d : 0.0;

SortField primero = sortFields[0];

IOrderedEnumerable<Dictionary<string, string>> ordered = (primero.Numeric, primero.Descender) switch
{
(true, true) => rows.OrderByDescending(r => NumKey(r, primero.Nombre)),
(true, false) => rows.OrderBy(r => NumKey(r, primero.Nombre)),
(false, true) => rows.OrderByDescending(r => r[primero.Nombre], StringComparer.OrdinalIgnoreCase),
(false, false) => rows.OrderBy(r => r[primero.Nombre], StringComparer.OrdinalIgnoreCase),
};
for (int i = 1; i < sortFields.Count; i++)
{
SortField sf = sortFields[i];
ordered = (sf.Numeric, sf.Descender) 
switch
{
(true, true) => ordered.ThenByDescending(r => NumKey(r, sf.Nombre)),
(true, false) => ordered.ThenBy(r => NumKey(r, sf.Nombre)),
(false, true) => ordered.ThenByDescending(r => r[sf.Nombre], StringComparer.OrdinalIgnoreCase),
(false, false) => ordered.ThenBy(r => r[sf.Nombre], StringComparer.OrdinalIgnoreCase),
};
}

return ordered.ToList();
}

//Ejercicio 5

string Serialize(List<Dictionary<string, string>> rows,
string[]? headers,
AppConfig cfg)
{

var sb = new System.Text.StringBuilder();

if (!cfg.NoHeader && headers is not null)
{

sb.AppendLine(string.Join(cfg.Delimitador, headers));

foreach ( var row in rows)
{

 var values = headers.Select(h => row.TryGetValue(h, out string? v) ? v : string.Empty);
 sb.AppendLine(string.Join(cfg.Delimitador, values));
}
}
else
{
    foreach (var row in rows)
    {
        var values = row.Values;
        sb.AppendLine(string.Join(cfg.Delimitador, values));
    }
}

return sb.ToString();
}

//Ejercicio 6

void WriteSalida(string text, AppConfig cfg)
{
    if (cfg.salida is not null)
    File.WriteAllText(cfg.salida, text);
    else
    Console.Write(text);
}

void ayuda()
{
    Console.WriteLine(@"sortx - Ordena datos de textos delimitados (csv, tsv, etc.)");
    Console.WriteLine(@"Su uso: sortx [entrada [salida]] [-b|--by campo[:tipo[:orden]]]... [-i|--input entrada] [-o|--output salida] [-d|--delimiter delimitador] [-nh|--no-header] [-h|--help]");

    Opciones:

    -b|--by campo[:tipo[:orden]]   Especifica un campo por el cual ordenar. Se puede repetir para ordenar por varios campos. 'tipo' puede ser 'num' para orden numérico (por defecto es orden lexicográfico). 'orden' puede ser 'desc' para orden descendente (por defecto es ascendente).

    -i|--input entrada             Especifica el archivo de entrada. Si no se especifica, se lee de la entrada estándar.

    -o|--output salida             Especifica el archivo de salida. Si no se   especifica, se escribe a la salida estándar.

    -d|--delimiter delimitador      Especifica el delimitador de campos (por defecto es la coma ','). Para tabulador, usar '\t'.

    -nh|--no-header                Indica que la primera línea no es un encabezado, sino que es parte de los datos. En este caso, las columnas se referencian por su índice (0, 1, 2, etc.) en lugar de por nombre.

    -h|--help  ayuda                    Muestra esta ayuda y sale.
}

var config = ParseArgs(args);
if (config.Ayuda)
{
    ayuda();
    return;

}
var rawText = ReadEntrada(config);
var (rows, headers) = ParseDelimited(rawText, config);
var sortedRows = SortRows(rows, config.SortFields);
var salidaText = Serialize(sortedRows, headers, config);
WriteSalida(salidaText, config);

}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error de argumento: {ex.Message}");
    Console.Error.WriteLine("Use --help para ver la ayuda.");
    Environment.Exit(1);
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"Error: Archivo no encontrado - {ex.FileName}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error inesperado: {ex.Message}");
    Environment.Exit(1);
}
record SortField(
    string Nombre,
    bool Numeric,
    bool Descender);

record AppConfig(
    string? entrada,
    string? salida,
    string Delimitador,
    bool NoHeader,
    bool Ayuda,
    List<SortField> SortFields);