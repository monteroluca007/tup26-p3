
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

using System.Drawing;
using System.Net.Http.Headers;
using Microsoft.VisualBasic.FileIO;

Console.WriteLine($"sortx {string.Join(" ", args)}");


// este appconfig guarda los datos temporalmente, a diferencia del record que los toma al final
AppConfig parseargs(string[] args)
{
    string? inputFile = null; 
    string? outputFile = null; 
    string delimiter = ","; 
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
                throw new ArgumentException($"necesita '{arg}' un valor.");

            sortFields.Add(ParseSortField(args[++i]));
            continue;
        }

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

        if (arg == "--delimiter" || arg == "-d")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"necesita '{arg}' un valor.");

            string raw = args[++i];
            delimiter = raw == @"\t" ? "\t" : raw;
            continue;
        }

        if (!arg.StartsWith("-"))
        {
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
    //  se verifica que el archivo cfg no sea null (los datos parseados)
    if(cfg.InputFile != null) 
      return File.ReadAllText(cfg.InputFile); 
    return Console.In.ReadToEnd(); 
}
// lista de fila y encabezado en base al texto de archivo cfg
(List<Dictionary<string,string>> rows, string[]? Header) parsedelimited(string text, AppConfig cfg);

    // el templines va a tener todas las lineas incluyendo las vacias tambien, despues con el split separa en lineas y por ultimo se eliminan las lineas vacias.
    var tempLines = text
        .Replace("\r\n", "\n")
        .Replace("\r", "\n")
        .Split('\n');
    string[] lines = Array.FindAll(tempLines, l => l.Length > 0);

    if (lines.Length == 0)
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
    header = null;
    dataStart = 0;
}
var rows = new List<Dictionary<string, string>>(); //guardo los datos de listas en esta variable rows
for (int lineIdx = dataStart; lineIdx < lines.Length; lineIdx++) // recorre las lineas del archivo en base al data start y si es q hay encabezado o no
{
    var values = lines[lineIdx].Split(cfg.Delimiter); // se guardan los datos spliteados en values

    var row = new Dictionary<string, string>(); // se crea una carpeta vacia para guardar los datos de cada fila.

    if (!cfg.NoHeader && header is not null)
    {
        for (int col = 0; col < header.Length; col++) // bucle q recorre las columnas de las columnas 0 al 3
        {
            row[header[col]] = col < values.Length ? values[col] : string.Empty; // se guarda en  la var row el valor de cada columna
        }
    }
    else
    {
        for (int col = 0; col < values.Length; col++)
        {
            row[col.ToString()] = values[col];
        }
    }
    rows.Add(row);
}
    if (cfg.sortFields.Count > 0 && row.Count > 0) //toma en cuenta la cantidad de elementos de sort fields y la cantidad de elementos de row 

    var firstrow = rows[0]; // guarda el valor de la primera fila de rows en la variable firstrow
    foreach (var sf in cfg.sortFields) //bucle en donde la variable sf es temporal y toma los datos de cfg sort fields
    {
        if (!firstrow.ContainsKey(sf.Name)) // si no existe la condicion de que la primera fila contenga la clave del campo de ordenamiento
        {
            string available = "";
            foreach (var key in firstrow.Keys) //bucle mostrando las columnas de row y se guardan en key temporalmente 
            {
                if (available.Length > 0) 
                available += ", ";
                available += key; 
                // se verifica si la var available tiene algun valor, si es que tiene se le agrega una , y despues el nombre de la columna que esta en key
            }
         throw new ArgumentException($"campo de ordenamiento desconocido: '{sf.Name}'. disponible: {available}"); // error si es que la primera fila no tiene clave sf
        }
    }
    return (rows, header); // devuelve las filas y el encabezado

    List<Dictionary<string, string>> sortrows( //lista con clave string y valor string que representa los nombres de la fila y datos de estas,sortrows funcion q ordena las filas en base a rows
     List<Dictionary<string, string>> rows, //lista de filas a ordenar
     List<SortField> sortFields) //lista de criterios de ordenamiento
    {
        if (sortFields.Count == 0 || rows.Count == 0) 
        return rows; // si no hay criterio de ordenamiento o filas devuelve las filas sin ordenar 
    }
     rows.Sort((a, b) => // ordenamo lista comparando fila a y b
 {
     foreach (var sf in sortFields) // recorremos los criterios de ordenamiento para comparar las filas a y b y se guardan en sf temporalmente
     {
         int cmp = 0;  // aqui se guardara e l resultado de la comparacion.

         if (sf.Numeric) 
         {
            // creacion de 2 variables a y b para guardar los valores numericos de las filas a y b , se inician en 0 por si no se pueden parsear a numero
            double va = 0; 
            double vb = 0; 

             //en caso de ser string se parsean a numero, si no se pueden parsear quedan en 0, se guardan en va y vb.
            double.TryParse(a[sf.Name], out va); 
            double.TryParse(b[sf.Name], out vb); 

            cmp = va.CompareTo(vb); // se guarda en cmp el resultado de la comparacion entre las dos variables
         }

         else
         {
            // verificacion de que los valores de a y b no sean null, si son null se ponen como ""  de lo contrario quedan con el valor orig
             string va = a[sf.Name] ?? ""; 
             string vb = b[sf.Name] ?? "";
             cmp = string.Compare(va, vb, StringComparison.Ordinal); // comparacion guardada en cmp
         }

        if(cmp != 0) // si la comparacion es igual a 0 se debe decidir cual variable va primero, si cmp es distinto de 0 se devuelve el resultado de la comparacion teniendo en cuenta el ordenamiento ascendente o descendente
        {
            if (sf.Descending)  // mayor a menor
                return -cmp;   // devuelve la fila que tenga el valor mayor
            else
                return cmp; // lo contrario la menor.
        }
     }
     return 0;

 });
    return rows;








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


