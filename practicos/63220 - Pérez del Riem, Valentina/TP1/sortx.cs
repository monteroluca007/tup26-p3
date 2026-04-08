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


 if (input == null && posicionales.Count > 0)
        input = posicionales[0];

    if (output == null && posicionales.Count > 1)
        output = posicionales[1];

    if (posicionales.Count > 2)
        throw new Exception("Hay demasiados argumentos posicionales.");

    if (reglas.Count == 0)
        throw new Exception("Debes indicar al menos un campo de orden con -b.");

    return new AppConfig(input, output, separador, sinHeader, reglas);

    void MostrarAyuda()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...");
        Console.WriteLine("      [-i|--input input] [-o|--output output]");
        Console.WriteLine("      [-d|--delimiter delimitador]");
        Console.WriteLine("      [-nh|--no-header] [-h|--help]");
        Console.WriteLine();
        Console.WriteLine("Ejemplos:");
        Console.WriteLine("sortx empleados.csv -b apellido");
        Console.WriteLine("sortx empleados.csv -b salario:num:desc");
        Console.WriteLine("sortx datos.tsv -d \"\\t\" -nh -b 1:alpha:asc");
    }
}


SortField ParseSortField(string texto)
{
    string[] partes = texto.Split(':');

    if (partes.Length == 0 || string.IsNullOrWhiteSpace(partes[0]))
        throw new Exception("Campo inválido en --by.");

    string nombre = partes[0];
    bool esNumerico = false;
    bool esDescendente = false;

    if (partes.Length > 1)
    {
        string tipo = partes[1].ToLower();

        if (tipo == "num")
            esNumerico = true;
        else if (tipo == "alpha")
            esNumerico = false;
        else
            throw new Exception("Tipo inválido en --by: " + partes[1]);
    }

    if (partes.Length > 2)
    {
        string orden = partes[2].ToLower();

        if (orden == "desc")
            esDescendente = true;
        else if (orden == "asc")
            esDescendente = false;
        else
            throw new Exception("Orden inválido en --by: " + partes[2]);
    }

    if (partes.Length > 3)
        throw new Exception("Formato inválido en --by: " + texto);

    return new SortField(nombre, esNumerico, esDescendente);
}