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


// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");