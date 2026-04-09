using System;
using static System.Console;
using System.IO;
try
{
    AppConfig configuracion = ParseArgs(args);
    string texto = ReadInput(configuracion);
    List<Dictionary<string, string>> filas = ParseDelimited(texto, configuracion.Delimitador, configuracion.noheader);
    List<Dictionary<string, string>> orden = SortRow(filas, configuracion);
}
catch (Exception e)
{
    Error.WriteLine("Error encontrado:" + e.Message);
    Environment.Exit(1); // aviso
}

static AppConfig ParseArgs(string[] argumentos)
{
    //configuracion
    string? entrada = null;
    string? Salida = null;
    string Delimitador = "," ?? "\t" ?? "|";
    bool noheader = false;
    List<SortField> sortfields = [];

    for (int i = 0; i < argumentos.Length; i++) /*recorre los argumentos, valida y guarda lo que viene despues */
    {
        if (argumentos[i] == "-i" || argumentos[i] == "--input")
        {
            entrada = argumentos[++i];
        }
        else if (argumentos[i] == "-o" || argumentos[i] == "--output") Salida = argumentos[++i];
        else if (argumentos[i] == "-d" || argumentos[i] == "--delimiter") Delimitador = argumentos[++i];
        else if (argumentos[i] == "-nh" || argumentos[i] == "--no-header") noheader = true;
        else if (argumentos[i] == "-b" || argumentos[i] == "--by")
        {
            string[] config = argumentos[++i].Split(':');
            //Ordenamiento
            bool num = false;
            bool desc = false;
            bool asc = true;//default 
            bool alpha = true;//default
            string campo = config[0];

            for (int j = 1; j < config.Length; j++)
            {
                //cambia tipo
                if (config[j] == "num") { num = true; alpha = false; }

                //orden
                if (config[j] == "desc") { desc = true; asc = false; }

            }
            sortfields.Add(new SortField(campo, alpha, num, desc, asc));

        }
        else if (argumentos[i] == "-h")
        {
            WriteLine("help (ayuda)");
            WriteLine("sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...\n      [-i|--input input] [-o|--output output]\n      [-d|--delimiter delimitador]\n      [-nh|--no-header] [-h|--help]");

            Environment.Exit(0);
        }
        else if (!argumentos[i].StartsWith("-")) // no comienza con - 
        {
            if (entrada == null) entrada = argumentos[i];
            else if (Salida == null) Salida = argumentos[i];
        }
    }
    return new AppConfig(entrada, Salida, Delimitador, noheader, sortfields);

}

static string ReadInput(AppConfig configuracion)

{
    if (configuracion.Entrada != null)
    {
        return File.ReadAllText(configuracion.Entrada);
    }
    else
    {
        return In.ReadToEnd();
    }
}


static List<Dictionary<string, string>> ParseDelimited(string texto, string? delimitador, bool noheader)
{
    string[] linea = texto.Split('\n'); // Divido por fila
    Dictionary<string, string> fila = [];
    List<Dictionary<string, string>> filas = []; // Lista final 

    if (noheader == false)
    {
        string[] encabezado = linea[0].Split(delimitador); // nombres columna 

        for (int i = 1; i < linea.Length; i++) //recorre las filas
        {
            string[] valores = linea[i].Trim().Split(delimitador);
            for (int j = 0; j < valores.Length; j++)
            {
                fila.Add(encabezado[j], valores[j]); //agrego al diccionario 
            }
            filas.Add(fila);
            fila = []; // limpia la variable
        }
    }
    else
    {
        for (int i = 0; i < linea.Length; i++) //recorre las filas
        {
            string[] valores = linea[i].Trim().Split(delimitador);
            string[] encabezado = new string[valores.Length];
            for (int j = 0; j < valores.Length; j++)
            {
                encabezado[j] = j.ToString(); //El indice pasa a ser nombre de la columna. 
            }
            for (int j = 0; j < valores.Length; j++)
            {
                fila.Add(encabezado[j], valores[j]); //agrego al diccionario 
            }
            filas.Add(fila);
            fila = [];
        }
    }

    return filas;

}
//4. SortRows       → ordenar las filas según los criterios configurados

static List<Dictionary<string, string>> SortRow(List<Dictionary<string, string>> filas, AppConfig configuracion)
{
    filas.Sort((a, b) => 
    {
        foreach (var criterio in configuracion.sortfields)
        {
            int resultado = 0;
            if (criterio.num)
            {
                double valA = double.Parse(a[criterio.campo]);
                double valB = double.Parse(b[criterio.campo]);
                resultado = valA.CompareTo(valB);
            }
            else
            {
                resultado = string.Compare(a[criterio.campo], b[criterio.campo]);
            }

            if (criterio.desc) resultado *= -1;

            if (resultado != 0) return resultado;
        }
        return 0;
    });
    return filas;
}
    

static string Serialize(List<Dictionary<string, string>> filas, AppConfig config)
{
   

}
static void WriteOutput(string contenido, AppConfig configuracion) //salida
{
    if (configuracion.Salida != null)
    {
        File.WriteAllText(configuracion.Salida, contenido);
    }
    else
    {
        Write(contenido); // Escribe en la consola (stdout)
    }
}
record SortField(string campo, bool alpha, bool num, bool desc, bool asc); // campo por el que ordena. 
record AppConfig(string? Entrada, string? Salida, string Delimitador, bool noheader, List<SortField> sortfields);

/*
sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
      [-i|--input input] [-o|--output output]
      [-d|--delimiter delimitador]
      [-nh|--no-header] [-h|--help]

| `--by`          | `-b`   | Campo por el que ordenar. Se puede repetir para ordenamiento múltiple. |
| `--no-header`   | `-nh`  | Indica que el archivo no tiene fila de encabezado. En ese caso los campos se identifican por su índice numérico (0, 1, 2...). |

Cada valor de `--by` tiene el formato `campo[:tipo[:orden]]`, donde:

- **`campo`** — nombre de la columna (si hay encabezado) o índice numérico desde 0 (si no   hay encabezado).
- **`tipo`** — criterio de comparación:
                             `alpha` — comparación alfabética (default).
  -                          `num` — comparación numérica.
- **`orden`** — dirección:
  -                           `asc` — ascendente (default).
  -                           `desc` — descendente.

4. SortRows       → ordenar las filas según los criterios configurados
5. Serialize      → convertir las filas ordenadas de vuelta a texto


*/