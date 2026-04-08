
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

record CampoOrden(string Nombre, bool Descendente);

record Configuracion(string Entrada, string Salida, bool Descendente, List<CampoOrden> Campos);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Iniciando aplicación...");
    }

    static Configuracion ObtenerConfiguracion(string[] args)
    {
        throw new NotImplementedException();
    }

    static List<string> LeerArchivo(string ruta)
    {
        throw new NotImplementedException();
    }

    static List<string> OrdenarLineas(List<string> lineas, Configuracion config)
    {
        throw new NotImplementedException();
    }

    static void GuardarArchivo(string ruta, List<string> lineas)
    {
        throw new NotImplementedException();
    }

    static void MostrarAyuda()
    {
        throw new NotImplementedException();
    }
}

static Configuracion ObtenerConfiguracion(string[] args)
{
    string? entrada = null;
    string? salida = null;
    bool descendente = false;

    foreach (var arg in args)
    {
        if (arg.StartsWith("--entrada="))
            entrada = arg.Replace("--entrada=", "");

        else if (arg.StartsWith("--salida="))
            salida = arg.Replace("--salida=", "");

        else if (arg == "--desc")
            descendente = true;

        else if (arg == "--help" || arg == "-h")
        {
            MostrarAyuda();
            Environment.Exit(0);
        }
    }

    if (entrada == null || salida == null)
        throw new ArgumentException("Debe indicar --entrada y --salida");

    return new Configuracion(entrada, salida, descendente, new List<CampoOrden>());
}

static List<string> LeerArchivo(string ruta)
{
    if (!File.Exists(ruta))
        throw new FileNotFoundException("El archivo indicado no existe.");

    return File.ReadAllLines(ruta).ToList();
}

static void GuardarArchivo(string ruta, List<string> lineas)
{
    File.WriteAllLines(ruta, lineas);
}