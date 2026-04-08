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

    if (campos.Count == 0)
        throw new Exception("Debe usar -b para ordenar");

    return new Configuracion(entrada, salida, delimitador, sinEncabezado, campos, ayuda);
}
CampoOrden ParsearCampo(string texto)
{
    var partes = texto.Split(':');
    string nombre = partes[0];
    bool esNumero = false;
    bool desc = false;

    if (partes.Length > 1)
        esNumero = partes[1] == "num";
    if (partes.Length > 2)
        desc = partes[2] == "desc";
    return new CampoOrden(nombre, esNumero, desc);
}
void MostrarAyuda()
{
    Console.WriteLine("Uso: sortx [entrada [salida]] -b campo[:tipo[:orden]]");
}
string LeerEntrada(Configuracion config)
{
    if (config.Entrada != null)
        return File.ReadAllText(config.Entrada);

    return Console.In.ReadToEnd();
}
record CampoOrden(string Nombre, bool EsNumero, bool Desc);
record Configuracion( string? Entrada,string? Salida,string Delimitador,bool SinEncabezado,List<CampoOrden> Campos,bool Ayuda);