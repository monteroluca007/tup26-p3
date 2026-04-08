using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]
try
{
    Console.WriteLine($"sortx {string.Join(" ", args)}");
    
    AppConfig config = ParseArgs(args);
    string texto = ReadInput(config);
    var datos = ParseDelimited(texto,config);
    List<Dictionary<string, string>> filasOrdenadas = SortRows(datos.Rows, datos.Headers, config);
    string salida = Serialize(filasOrdenadas,datos.Headers,config);
    WriteOutput(salida,config);
}
catch (Exception ex)
{
    Console.Error.WriteLine("error: "+ ex.Message);
    Environment.Exit(1);
}