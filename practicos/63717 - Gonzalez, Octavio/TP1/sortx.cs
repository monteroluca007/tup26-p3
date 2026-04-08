using System;

using static System.Console;
using System.IO;
/*
sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
      [-i|--input input] [-o|--output output]
      [-d|--delimiter delimitador]
      [-nh|--no-header] [-h|--help]
| Opción larga    | Corta  | Descripción |
|-----------------|--------|-------------|
| `--by`          | `-b`   | Campo por el que ordenar. Se puede repetir para ordenamiento múltiple. |
| `--input`       | `-i`   | Archivo de entrada. |
| `--output`      | `-o`   | Archivo de salida. |
| `--delimiter`   | `-d`   | Carácter delimitador. Default: `,`. Usar `\t` para tabulación. |
| `--no-header`   | `-nh`  | Indica que el archivo no tiene fila de encabezado. En ese caso los campos se identifican por su índice numérico (0, 1, 2...). |
| `--help`        | `-h`   | Muestra la ayuda y termina. |

Especificación de campo: `campo[:tipo[:orden]]`

Cada valor de `--by` tiene el formato `campo[:tipo[:orden]]`, donde:

- **`campo`** — nombre de la columna (si hay encabezado) o índice numérico desde 0 (si no hay encabezado).
- **`tipo`** — criterio de comparación:
                             `alpha` — comparación alfabética (default).
  -                          `num` — comparación numérica.
- **`orden`** — dirección:
  -                           `asc` — ascendente (default).
  -                           `desc` — descendente.


1. ParseArgs      → leer la configuración desde los argumentos
2. ReadInput      → leer el texto desde el archivo o stdin
3. ParseDelimited → convertir el texto en una lista de filas (lista de diccionarios)
4. SortRows       → ordenar las filas según los criterios configurados
5. Serialize      → convertir las filas ordenadas de vuelta a texto
6. WriteOutput    → escribir en el archivo de salida o stdout

punto de entrada (`try/catch` principal) debe limitarse a invocar estas funciones en orden, sin lógica adicional.

Si el archivo de entrada no se especifica, la herramienta debe leer desde stdin.
Si el archivo de salida no se especifica, la herramienta debe escribir en stdout.

*/

try
{
    /*parseargs  => reandinput => parsedelimited => sortrows => serialize => writeoutput
      Obtener de consola => leer config => separar => ordenar => escribir => entregar 
    */

    AppConfig configuracion = ParseArgs(args); //args ==> lo q pasa por consola 
    string texto = ReadInput(configuracion);
    List<Dictionary<string, string>> filas = ParseDelimited(texto);
}
catch (Exception e)
{
    Error.WriteLine("Error encontrado:" + e.Message);
    Environment.Exit(1); // aviso que el programa fallo 
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
                if (config[j] == "desc") {desc = true;asc = false; }

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
; //Retornara el record AppConfig la funcion es ParseArgs 

static string ReadInput(AppConfig configuracion) // Me lee las entradas detexto, valida y escupe un string que se guarda en texto
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

record SortField(string campo, bool alpha, bool num, bool desc, bool asc); // campo por el que ordena. 
record AppConfig(string? Entrada, string? Salida, string Delimitador, bool noheader, List<SortField> sortfields);






