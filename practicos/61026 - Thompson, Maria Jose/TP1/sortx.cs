using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

try
{
    var config = LeerArgumentos(args);
    var contenido = LeerEntrada(config);
    var filas = ProcesarTexto(contenido, config);
    var filasOrdenadas = OrdenarFilas(filas, config);
    var resultado = Serializar(filasOrdenadas, config);
    EscribirSalida(resultado, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
Configuracion LeerArgumentos(string[] argumentos)
{
    string? entrada = null;
    string? salida = null;
    string delim = ",";
    bool noHeader = false;
    var criterios = new List<CriterioOrden>();

    for (int i = 0; i < argumentos.Length; i++)
    {
        string arg = argumentos[i];

        if (arg == "-h" || arg == "--help")
        {
            Console.WriteLine("Uso: sortx [input [output]] [opciones]");
            Environment.Exit(0);
        }
        else if (arg == "-i" || arg == "--input")
        {
            entrada = argumentos[++i];
        }
        else if (arg == "-o" || arg == "--output")
        {
            salida = argumentos[++i];
        }
        else if (arg == "-d" || arg == "--delimiter")
        {
            delim = argumentos[++i].Replace("\\t", "\t");
        }
        else if (arg == "-nh" || arg == "--no-header")
        {
            noHeader = true;
        }
        else if (arg == "-b" || arg == "--by")
        {
            string[] partes = argumentos[++i].Split(':');
            string campo = partes[0];
            bool esNum = partes.Length > 1 && partes[1] == "num";
            bool esDesc = partes.Length > 2 && partes[2] == "desc";
            criterios.Add(new CriterioOrden(campo, esNum, esDesc));
        }
        else
        {
            if (entrada == null) entrada = arg;
            else if (salida == null) salida = arg;
        }
    }
    return new Configuracion(entrada, salida, delim, noHeader, criterios);
}
string LeerEntrada(Configuracion config)
{
    if (config.ArchivoEntrada != null)
    {
        return File.ReadAllText(config.ArchivoEntrada);
    }

    return Console.In.ReadToEnd();
}
List<Dictionary<string, string>> ProcesarTexto(string texto, Configuracion config)
{
    var lineas = texto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    var listaResultante = new List<Dictionary<string, string>>();

    if (lineas.Length == 0) return listaResultante;

    string[] encabezados;
    int indiceInicio = 0;

    if (config.SinEncabezado)
    {
        string[] columnasFilaUno = lineas[0].Split(config.Delimitador);
        encabezados = new string[columnasFilaUno.Length];
        for (int i = 0; i < columnasFilaUno.Length; i++) encabezados[i] = i.ToString();
    }
    else
    {
        encabezados = lineas[0].Split(config.Delimitador);
        indiceInicio = 1;
    }
    for (int i = indiceInicio; i < lineas.Length; i++)
    {
        string[] celdas = lineas[i].Split(config.Delimitador);
        var filaDiccionario = new Dictionary<string, string>();

        for (int j = 0; j < encabezados.Length; j++)
        {
            filaDiccionario[encabezados[j]] = j < celdas.Length ? celdas[j] : "";
        }
        listaResultante.Add(filaDiccionario);
    }
    if (!config.SinEncabezado)
    {
        var filaHeader = new Dictionary<string, string>();
        foreach (var nombreColumna in encabezados)
        {
            filaHeader[nombreColumna] = nombreColumna;
        }
        listaResultante.Insert(0, filaHeader);
    }

    return listaResultante;
}




record CriterioOrden(string Campo, bool EsNumerico, bool EsDescendente);
record Configuracion(
    string? ArchivoEntrada,
    string? ArchivoSalida,
    string Delimitador,
    bool SinEncabezado,
    List<CriterioOrden> Criterios
);
