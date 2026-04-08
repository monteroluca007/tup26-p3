using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]
try
{
    Console.WriteLine($"sortx {string.Join(" ", args)}");
    
    AppConfig config = ParseArgs(args);
    string texto = ReadInput(config);
    var datos = ParseDelimited(texto,config);
    List<Dictionary<string, string>> filasOrdenadas = SortRows(datos.Rows, datos.Headers, config);
    string salida = Serialize(filasOrdenadas,datos.Headers,config);
    WriteOutput(salida,config);
}
catch (Exception ex)
{
    Console.Error.WriteLine("error: "+ ex.Message);
    Environment.Exit(1);
}

// 1. ParseArgs

AppConfig ParseArgs(string[] argumentos)
{
    string? input = null;
    string? output = null;
    string separador = ",";
    bool sinHeader = false;
    List<SortField> reglas = new List<SortField>();
    List<string> posicionales = new List<string>();

      for (int i = 0; i < argumentos.Length; i++)
    {
        string actual = argumentos[i].ToLower();

        switch (actual)
        {
            case "-h":
            case "--help":
                MostrarAyuda();
                Environment.Exit(0);
                break;

            case "-i":
            case "--input":
                if (i + 1 >= argumentos.Length)
                    throw new Exception("Falta el archivo de entrada.");

                input = argumentos[++i];
                break;

            case "-o":
            case "--output":
                if (i + 1 >= argumentos.Length)
                    throw new Exception("Falta el archivo de salida.");

                output = argumentos[++i];
                break;

            case "-d":
            case "--delimiter":
                if (i + 1 >= argumentos.Length)
                    throw new Exception("Falta el delimitador.");

                separador = argumentos[++i];

                if (separador == "\\t")
                    separador = "\t";

                break;

            case "-nh":
            case "--no-header":
                sinHeader = true;
                break;

            case "-b":
            case "--by":
                if (i + 1 >= argumentos.Length)
                    throw new Exception("Falta el valor para -b / --by.");

                reglas.Add(ParseSortField(argumentos[++i]));
                break;

            default:
                if (!argumentos[i].StartsWith("-"))
                {
                    posicionales.Add(argumentos[i]);
                }
                else
                {
                    throw new Exception("Opción no válida: " + argumentos[i]);
                }
                break;
        }
    } 
}