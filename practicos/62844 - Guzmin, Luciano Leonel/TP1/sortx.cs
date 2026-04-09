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

(List<Dictionary<string, string>> Filas, List<string> Encabezados) ParsearDelimitado(string texto, AppConfig config)
{
    var lineas = new List<string>();

    foreach (var l in texto.Split('\n'))
    {
        var limpia = l.Trim('\r');
        if (!string.IsNullOrWhiteSpace(limpia))
            lineas.Add(limpia);
    }

    if (lineas.Count == 0)
        throw new Exception("Archivo vacío");

    List<string> encabezados = new List<string>();

    if (!config.NoHeader)
    {
        var partes = Separar(lineas[0], config.Delimiter);
        foreach (var p in partes)
            encabezados.Add(p.Trim());

        lineas.RemoveAt(0);
    }
    else
    {
        var partes = Separar(lineas[0], config.Delimiter);
        for (int i = 0; i < partes.Length; i++)
            encabezados.Add(i.ToString());
    }

    var filas = new List<Dictionary<string, string>>();

    foreach (var linea in lineas)
    {
        var valores = Separar(linea, config.Delimiter);
        var fila = new Dictionary<string, string>();

        for (int i = 0; i < encabezados.Count; i++)
        {
            if (i < valores.Length)
                fila[encabezados[i]] = valores[i].Trim();
            else
                fila[encabezados[i]] = "";
        }

        filas.Add(fila);
    }

    return (filas, encabezados);
}


string[] Separar(string linea, string delimitador)
{
    return linea.Split(new string[] { delimitador }, StringSplitOptions.None);
}

(List<Dictionary<string, string>> Filas, List<string> Encabezados) OrdenarFilas(
    (List<Dictionary<string, string>> Filas, List<string> Encabezados) datos,
    AppConfig config)
{
    var filas = datos.Filas;

    foreach (var campo in config.SortFields)
    {
        if (!datos.Encabezados.Contains(campo.Name))
            throw new Exception($"El campo '{campo.Name}' no existe");
    }

    for (int i = 0; i < filas.Count - 1; i++)
    {
        for (int j = 0; j < filas.Count - i - 1; j++)
        {
            if (Comparar(filas[j], filas[j + 1], config.SortFields) > 0)
            {
                var temp = filas[j];
                filas[j] = filas[j + 1];
                filas[j + 1] = temp;
            }
        }
    }

    return datos;
}

int Comparar(Dictionary<string, string> a, Dictionary<string, string> b, List<SortField> campos)
{
    foreach (var campo in campos)
    {
        string valA = a[campo.Name];
        string valB = b[campo.Name];

        int resultado;

        if (campo.Numeric)
        {
            double numA = double.TryParse(valA, out var na) ? na : 0;
            double numB = double.TryParse(valB, out var nb) ? nb : 0;
            resultado = numA.CompareTo(numB);
        }
        else
        {
            resultado = string.Compare(valA, valB, StringComparison.Ordinal);
        }

        if (resultado != 0)
        {
            return campo.Descending ? -resultado : resultado;
        }
    }

    return 0;
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);