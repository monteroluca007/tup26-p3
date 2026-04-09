using System;
using System.IO;
using System.Collections.Generic;

AppConfig ParsearArgumentos(string[] args)
{
    string? archivoEntrada = null;
    string? archivoSalida = null;
    string delimitador = ",";
    bool sinEncabezado = false;

    var camposOrden = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-i":
            case "--input":
                archivoEntrada = args[++i];
                break;

            case "-o":
            case "--output":
                archivoSalida = args[++i];
                break;

            case "-d":
            case "--delimiter":
                var d = args[++i];
                delimitador = d == "\\t" ? "\t" : d;
                break;

            case "-nh":
            case "--no-header":
                sinEncabezado = true;
                break;

            case "-b":
            case "--by":
                camposOrden.Add(ParsearCampoOrden(args[++i]));
                break;

            case "-h":
            case "--help":
                MostrarAyuda();
                Environment.Exit(0);
                break;

            default:
                if (archivoEntrada == null) archivoEntrada = args[i];
                else if (archivoSalida == null) archivoSalida = args[i];
                break;
        }
    }

    if (camposOrden.Count == 0)
        throw new Exception("Debe especificar al menos un campo de ordenamiento");

    return new AppConfig(archivoEntrada, archivoSalida, delimitador, sinEncabezado, camposOrden);
}
SortField ParsearCampoOrden(string expresion)
{
    var partes = expresion.Split(':');

    string nombre = partes[0];
    bool esNumerico = partes.Length > 1 && partes[1] == "num";
    bool descendente = partes.Length > 2 && partes[2] == "desc";

    return new SortField(nombre, esNumerico, descendente);
}

string LeerEntrada(AppConfig config)
{
    if (config.InputFile != null)
        return File.ReadAllText(config.InputFile);

    return Console.In.ReadToEnd();
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);