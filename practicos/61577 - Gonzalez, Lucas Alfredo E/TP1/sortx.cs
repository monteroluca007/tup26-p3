
// Programa sortx para ordenar archivos CSV
// Lucas Gonzalez - TP1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Configuracion del programa
record Configuracion(
    string ArchivoEntrada,
    string ArchivoSalida,
    char Delimitador,
    bool TieneEncabezado,
    List<(string Campo, string Tipo, string Orden)> CriteriosOrdenamiento
);

class Program
{
    static void Main(string[] args)
    {
        // Mostrar ayuda si se pide
        if (args.Contains("-h") || args.Contains("--help"))
        {
            MostrarAyuda();
            return;
        }

        // Aqui va el resto del codigo
    }

    static void MostrarAyuda()
    {
        Console.WriteLine("Uso: sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...");
        Console.WriteLine("Opciones:");
        Console.WriteLine("  -b, --by campo[:tipo[:orden]]  Campo para ordenar");
        Console.WriteLine("  -i, --input archivo           Archivo de entrada");
        Console.WriteLine("  -o, --output archivo          Archivo de salida");
        Console.WriteLine("  -d, --delimiter char          Delimitador (default: ,)");
        Console.WriteLine("  -nh, --no-header              Sin encabezado");
        Console.WriteLine("  -h, --help                    Mostrar esta ayuda");
    }

    static Configuracion ParsearArgumentos(string[] args)
    {
        string entrada = null;
        string salida = null;
        char delimitador = ',';
        bool tieneEncabezado = true;
        var criterios = new List<(string, string, string)>();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                case "--input":
                    entrada = args[++i];
                    break;
                case "-o":
                case "--output":
                    salida = args[++i];
                    break;
                case "-d":
                case "--delimiter":
                    delimitador = args[++i][0];
                    break;
                case "-nh":
                case "--no-header":
                    tieneEncabezado = false;
                    break;
                case "-b":
                case "--by":
                    var partes = args[++i].Split(':');
                    string campo = partes[0];
                    string tipo = partes.Length > 1 ? partes[1] : "alpha";
                    string orden = partes.Length > 2 ? partes[2] : "asc";
                    criterios.Add((campo, tipo, orden));
                    break;
                default:
                    if (entrada == null) entrada = args[i];
                    else if (salida == null) salida = args[i];
                    break;
            }
        }

        return new Configuracion(entrada, salida, delimitador, tieneEncabezado, criterios);
    }
}