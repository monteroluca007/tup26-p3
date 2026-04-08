
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
}