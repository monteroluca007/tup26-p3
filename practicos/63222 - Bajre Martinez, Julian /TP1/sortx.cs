using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


try
{
    var config = ParsearArgumentos(args);
    var texto = LeerEntrada(config);
    var filas = ParsearDelimitado(texto, config);
    var filasOrdenadas = OrdenarFilas(filas, config);
    var salida = Serializar(filasOrdenadas, config);
    EscribirSalida(config, salida);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}

ConfiguracionApp ParsearArgumentos(string[] args)
{
    string? archivoEntrada = null;
    string? archivoSalida = null;
    string delimitador = ",";
    bool sinEncabezado = false;
    var camposOrden = new List<CampoOrden>();

    int cantidadPosicionales = 0;

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

            case "-h":
            case "--help":
                MostrarAyuda();
                Environment.Exit(0);
                break;

            case "-b":
            case "--by":
                var partes = args[++i].Split(':');

                string nombre = partes[0];
                bool esNumerico = partes.Length > 1 && partes[1] == "num";
                bool descendente = partes.Length > 2 && partes[2] == "desc";

                camposOrden.Add(new CampoOrden(nombre, esNumerico, descendente));
                break;

            default:
                if (!args[i].StartsWith("-"))
                {
                    if (cantidadPosicionales == 0)
                        archivoEntrada = args[i];
                    else if (cantidadPosicionales == 1)
                        archivoSalida = args[i];

                    cantidadPosicionales++;
                }
                break;
        }
    }

    if (camposOrden.Count == 0)
        throw new Exception("Debe especificar al menos un campo de ordenamiento (-b)");

    return new ConfiguracionApp(
        archivoEntrada,
        archivoSalida,
        delimitador,
        sinEncabezado,
        camposOrden
    );
}

string LeerEntrada(ConfiguracionApp config)
{
    if (config.ArchivoEntrada != null)
        return File.ReadAllText(config.ArchivoEntrada);

    return Console.In.ReadToEnd();
}
List<Dictionary<string, string>>ParsearDelimitado(string texto, ConfiguracionApp config)
{
    var lineas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.TrimEnd('\r')).ToArray();
    if (lineas.Length == 0)
        throw new Exception("No se encontraron datos de entrada.");
        string[] encabezados;
        int inicio = 0;
    if (!config.SinEncabezado)
    {
        encabezados = lineas[0].Split(config.Delimitador);
        inicio = 1;
    }
    else
    {
        int cantidadCampos = lineas[0].Split(config.Delimitador).Length;
        encabezados = Enumerable.Range(0, cantidadCampos).Select(i => i.ToString()).ToArray();       
    }

    var filas = new List<Dictionary<string, string>>();
    for (int i = inicio; i < lineas.Length; i++)
    {
        var valores = lineas[i].Split(config.Delimitador);
        if (valores.Length != encabezados.Length)
            throw new Exception($"La fila {i + 1} tiene una cantidad de campos diferente al encabezado.");

        var fila = new Dictionary<string, string>();
        for (int j = 0; j < encabezados.Length; j++)
        {
            fila[encabezados[j]] = valores[j];
        }
        filas.Add(fila);
    }
    return filas;
}

List<Dictionary<string, string>> OrdenarFilas(List<Dictionary<string, string>> filas, configuracionApp config)
{
    IOrderedEnumerable<Dictionary<string, string>>? filasOrdenadas = null;

    foreach (var campo in config.CamposOrden)
    {
        if (!filas[0].ContainsKey(campo.Nombre))
            throw new Exception($"El campo de ordenamiento '{campo.Nombre}' no existe en los datos.");

        Func<Dictionary<string, string>, object> selector = fila =>
        {
            var valor = fila[campo.Nombre];
            if (campo.EsNumerico)
            return double.Parse(valor);
            return valor;
        };

        if (filasOrdenadas == null)
        {
            filasOrdenadas = campo.Descendente
                ? filas.OrderByDescending(selector)
                : filas.OrderBy(selector);
        }
        else
        {
            filasOrdenadas = campo.Descendente
                ? filasOrdenadas.ThenByDescending(selector)
                : filasOrdenadas.ThenBy(selector);
        }
    }

    return filasOrdenadas?.ToList() ?? filas;
}

string Serializar(List<Dictionary<string, string>> filas, ConfiguracionApp config)
{
    var lineas = new List<string>();
    var encabezados = filas[0].Keys.ToArray();
    if (!config.SinEncabezado)
    {
        lineas.Add(string.Join(config.Delimitador, encabezados));
    }
    foreach (var fila in filas)
    {
        lineas.Add(string.Join(config.Delimitador, encabezados.Select(e => fila[e])));
    }
    return string.Join("\n", lineas);
}

void EscribirSalida(ConfiguracionApp config, string salida)
{
    if (config.ArchivoSalida != null)
        File.WriteAllText(config.ArchivoSalida, salida);
    else
        Console.WriteLine(salida);
}

record CampoOrden(string Nombre, bool EsNumerico, bool Descendente);
record ConfiguracionApp(
    string? ArchivoEntrada,
    string? ArchivoSalida,
    string Delimitador,
    bool SinEncabezado,
    List<CampoOrden> CamposOrden
);