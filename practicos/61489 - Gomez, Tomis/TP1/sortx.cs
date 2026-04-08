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

// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");