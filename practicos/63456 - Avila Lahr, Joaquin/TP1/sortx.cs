using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

try
{
    Console.WriteLine("Programa sortx iniciado");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
}

record CampoOrden(string Nombre, bool EsNumero, bool Desc);
record Config( string Entrada, string Salida, string Delimitador, bool SinEncabezado, List<CampoOrden> Campos );
