using static system.Console;
using System;
using System.IO;
using System.Collections.Generic;


WriteLine(@"
Indicaciones para usar sortx:

sortx [archivoEntrada [archivoSalida]] -b campo[:tipo[:orden]]

Ejemplo:
sortx empleados.csv -b apellido
sortx empleados.csv -b edad:num:desc

Opciones:
  -b, --by            Campo de ordenamiento
  -i, --input         Archivo de entrada
  -o, --output        Archivo de salida
  -d, --delimiter     Delimitador (ej: , | \t)
  -nh, --no-header    Indica que no hay encabezado
  -h, --help          Mostrar ayuda
");

//Parseo de argumentos
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
                sinEncabezado = true;
                break;

            case "-b":
                camposOrden.Add(ParsearCampoOrden(args[++i]));
                break;
        }
    }

    if (camposOrden.Count == 0)
        throw new Exception("Debe especificar un campo de ordenamiento");

    return new AppConfig(archivoEntrada, archivoSalida, delimitador, sinEncabezado, camposOrden);
}
//Parseo de campo de ordenamiento
SortField ParsearCampoOrden(string expresion)
{
    var partes = expresion.Split(':');

    string nombre = partes[0];
    bool esNumerico = partes.Length > 1 && partes[1] == "num";
    bool descendente = partes.Length > 2 && partes[2] == "desc";

    return new SortField(nombre, esNumerico, descendente);
}

//LECTURA de archivos de entrada
string LeerEntrada(AppConfig config)
{
    if (config.InputFile != null)
        return File.ReadAllText(config.InputFile);

    return Console.In.ReadToEnd();
}

//Parseo de datos sin ordenar
(List<Dictionary<string, string>>, List<string>) ParsearDelimitado(string texto, AppConfig config)
{
 var lineas = new List<string>();

    foreach (var l in texto.Split('\n'))
    {
        var limpia = l.Trim('\r');
        if (!string.IsNullOrWhiteSpace(limpia))
            lineas.Add(limpia);
    }
}


//MODELOS para configurar y ordenar

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);
