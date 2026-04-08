using System;
using static System.Console;


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

    AppConfig configuracion = ParseArgs(args); //args son los argumentos que me pasan por consola 
    string texto = ReadInput(configuracion);
    List<Dictionary<string, string>> filas = ParseDelimited(texto); //creo la lista diccionario y la igualo a la otra lista.
}
catch (Exception e)
{
    Error.WriteLine("Error encontrado:" + e.Message);
    Environment.Exit(1); // aviso que el programa fallo 
}

static AppConfig ParseArgs(string[] argumentos)
{
    string? entrada = null ; 
    string? Salida = null; 
    string Delimitador = ","; 
    bool noheader = false; 
    List<SortField> sortfields = new List<SortField>();


     for (int i=0; i<argumentos.Length; i++) //va recorriendo todos los argumentos y hace las validaciones, si cumple, guarda lo que viene despues 
    {
        if (argumentos[i] == "-i") entrada=argumentos[i+1]; 
        if (argumentos[i] == "-o") Salida=argumentos[i+1];
        if (argumentos[i] == "-d") Delimitador=argumentos[i+1];
        if (argumentos[i] == "-b")
        {
            string[] partes= argumentos[++i].Split(':');

            string nombre = partes[0];
            bool num=false;
            bool desc=false;
            bool asc=false;
            for (int j=1; j<partes.Length; j++)
            {
                if (partes[j]=="num") num=true;
                if (partes[j]=="desc") desc=true;
                if (partes[j]=="asc") asc=true;
            }

            sortfields.Add(new SortField(nombre, num, desc, asc));

        }
        ;
        if (argumentos[i] == "-nh") ;
        if (argumentos[i] == "-h") ;

    }
     


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

record SortField(string nombre, bool num, bool desc, bool asc); // campo por el que ordena. 
record AppConfig(string? Entrada, string? Salida, string Delimitador, bool noheader, List<SortField> sortfields);






