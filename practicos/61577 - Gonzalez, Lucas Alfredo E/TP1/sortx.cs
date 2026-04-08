
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
        // Aqui va el codigo principal
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
}