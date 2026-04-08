using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
try
{
    var config = LeerArgumentos(args);

    if (config == null) return;

    var texto = LeerEntrada(config);
    var (filas, encabezado) = ParsearTexto(texto, config);
    var ordenado = OrdenarFilas(filas, config);
    var salida = ConvertirTexto(ordenado, encabezado, config);
    EscribirSalida(salida, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}
Configuracion LeerArgumentos(string[] args)
{
    string? entrada = null;
    string? salida = null;
    string delimitador = ",";
    bool sinEncabezado = false;
    bool ayuda = false;
    var campos = new List<CampoOrden>();
    int i = 0;
    while (i < args.Length)
    {
        var arg = args[i];

        if (arg == "-h" || arg == "--help")
        {
            ayuda = true;
            MostrarAyuda();
            return null;
        }
        else if (arg == "-nh" || arg == "--no-header")
        {
            sinEncabezado = true;
        }
        else if (arg == "-d" || arg == "--delimiter")
        {
            i++;
            delimitador = args[i] == "\\t" ? "\t" : args[i];
        }
        else if (arg == "-i" || arg == "--input")
        {
            i++;
            entrada = args[i];
        }
        else if (arg == "-o" || arg == "--output")
        {
            i++;
            salida = args[i];
        }
        else if (arg == "-b" || arg == "--by")
        {
            i++;
            campos.Add(ParsearCampo(args[i]));
        }
        else if (!arg.StartsWith("-"))
        {
            if (entrada == null) entrada = arg;
            else if (salida == null) salida = arg;
        }

        i++;
    }


record CampoOrden(string Nombre, bool EsNumero, bool Desc);
record Configuracion( string? Entrada,string? Salida,string Delimitador,bool SinEncabezado,List<CampoOrden> Campos,bool Ayuda);