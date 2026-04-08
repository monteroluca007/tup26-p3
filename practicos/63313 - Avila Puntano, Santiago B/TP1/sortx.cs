
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

using System.Net.Http.Headers;
using Microsoft.VisualBasic.FileIO;

Console.WriteLine($"sortx {string.Join(" ", args)}");


// este appconfig guarda los datos temporalmente, a diferencia del record que los toma al final
AppConfig parseargs(string[] args)
{
    string? inputFile = null; 
    string? outputFile = null; 
    string deLimiter = ","; 
    bool noHeader = false; 
    bool showHelp = false;
    List<SortField> sortFields = new List<SortField>(); 
    int positional = 0; 

    SortField ParseSortField(string spec)
    {
        var parts = spec.Split(':');
        string name = parts[0];
        bool numeric = parts.Length > 1 && parts[1].Equals("num", StringComparison.OrdinalIgnoreCase);
        bool descending = parts.Length > 2 && parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
        return new SortField(name, numeric, descending);}
    

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg == "--help" || arg == "-h")
        
            showHelp = true;
            continue;
        

        if (arg == "--no-header" || arg == "-nh")
        {
            noHeader = true;
            continue;
        

        if (arg == "--by" || arg == "-b")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"necesita '{arg}' un valor.");

            sortFields.Add(ParseSortField(args[++i]));
            continue;
        

        if (arg == "--input" || arg == "-i")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"necesita '{arg}' un valor.");

            inputFile = args[++i];
            continue;
        }

        if (arg == "--output" || arg == "-o")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"necesita '{arg}' un valor.");

            outputFile = args[++i];
            continue;
        }

        if (arg == "--deLimiter" || arg == "-d")
        
            if (i + 1 >= args.Length)
                throw new ArgumentException($"necesita '{arg}' un valor.");

            string raw = args[++i];
            deLimiter = raw == @"\t" ? "\t" : raw;
            continue;
        }

        if (!arg.StartsWith("-"))
        
            if (positional == 0) { inputFile = arg; positional++; }
            else if (positional == 1) { outputFile = arg; positional++; }
            else throw new ArgumentException($"arg posicional inexistente: '{arg}'.");
            continue;
        }

        throw new ArgumentException($"opción desconocida: '{arg}'.");
    }

    return new AppConfig(inputFile, outputFile, delimiter, noHeader, showHelp, sortFields);

}
string readinput(AppConfig cfg)
{
    //  se verifica que el archivo cfg no sea null (los datos parseadso)
    if(cfg.InputFile == null) 
      return File.ReadAllText(cfg.InputFile); 
      return Console.In.ReadToEnd(); 
} 
// lista de fila y encabezado en base al texto de archivo cfg
(List<Dictionary<string,string>> rows,string[]? Header) parsedelimited (string text, AppConfigconfig cfg)
{
    // el templines va a tener todsas las lineas incluyendo las vacias tambien, despues con el split separa en lineas y por ultimo se eliminan las lineas vacias.
  var tempLines = text
    .Replace("\r\n", "\n")
    .Replace("\r", "\n")
    .Split('\n');
string[] lines = Array.FindAll(tempLines, l => l.Length > 0);
}

    if (lines.length == 0)
     return (new List<Dictionary<string, string>>(), null);



    string[] header;
    int dataStart;

    if (!cfg.NoHeader)
    {
        header = lines[0].Split(cfg.Delimiter);
        dataStart = 1;
    }
    else
    {
       headers = null;
       dataStart = 0;
    }

    






 //commit 1
record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    bool ShowHelp,
    List<SortField> SortFields
);


