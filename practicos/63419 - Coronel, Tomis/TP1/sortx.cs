
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;



record CampoOrden(string nombre , bool EsNumerico , bool Descendente);
record configuracion ( String? ArchivoEntrada, string? ArchivoSalida, string delimitador, bool SinEncabezado, List<CampoOrden>Campos);


class Program
{
    static int Main(string[] args)
    {
        try
        {
            var config = ParseArgs(args);
            var texto = LeerEntrada(config);
            var (encabezado, filas) = Parsear(config, texto);
            var FilasOrdenadas = Ordenar(config, encabezado, filas);
            var salida = Serializar(config, encabezado, FilasOrdenadas);
            WriteOutput(config, salida);  
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error: " + e.Message);
            return 1;
        }
    }
     
     
     
     configuracion parseArgs(string[] args)
     {
        string? archivoEntrada = null;
        string? archivoSalida = null;
        string delimitador = ",";
        bool sinEncabezado = false;
        List<CampoOrden> campos = new List<CampoOrden>();

        int i = 0;
        while (i < args.Length)
        {
            string arg = args[i];

            if (arg == "-h"  || arg ==  "--help" )
            {
                Console.WriteLine("uso: sortx [entrada [salida]] [-b|--by campo[:tipo[:orden]]]... [-i|--input input] [-o|--output output] [-d|--delimiter delimitador] [-nh|--no-header] [-h|--help]");
                Environment.Exit(0);
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
        else if (arg == "-d" || arg == "--delimiter")
        {
            i++;
            delimitador = args[i];
            if (delimitador == "\\t") delimitador = "\t";
        }
        else if (arg == "-nh" || arg == "--no-header")
        {
            sinEncabezado = true;
        }
        else if (arg == "-b" || arg == "--by")
        {
            i++;
            string[] partes = args[i].Split(':');

            string nombre = partes[0];
            bool numerico = false;
            bool descendente = false;

            if (partes.Length > 1 && partes[1] == "num")
                numerico = true;

            if (partes.Length > 2 && partes[2] == "desc")
                descendente = true;

            campos.Add(new CampoOrden(nombre, numerico, descendente));
        }
        else
        {
            if (entrada == null) entrada = arg;
            else if (salida == null) salida = arg;
            else throw new Exception("Demasiados argumentos");
        }

        i++;
    }






        if (campos.Count == 0)
            throw new Exception("Debe especificar al menos un campo de ordenamiento con -b o --by");
        
        
           return new configuracion(archivoEntrada, archivoSalida, delimitador, sinEncabezado, campos);
        }
     
     
     string leerEntrada(configuracion config)
     {
        if (config.ArchivoEntrada != null)
        {
            return File.ReadAllText(config.ArchivoEntrada);
        }
        else
        {
            return Console.In.ReadToEnd();
        }

        (List<string> encabezado, List<List<string>> filas) parsear(configuracion config, string texto)
        {
        
          var encabezado = new List<string>();
          var filas = new List<List<string>>();

            if (!config.SinEncabezado)
            {
                encabezado = lineas[0].Split(config.delimitador).ToList();
                  for (int i = 0; i < partesEncabezado.lenght; i++)
                {
                    encabezado.Add(partesEncabezado[i]);
                     inicio=1;
                }
            }
            for (int i = inicio; i < lineas.Length; i++)
            {
               string linea = lineas[i].Trim();
                if (linea == "") continue;
            }
     
     string? partes = linea.Split(config.delimitador);
            var fila = new List<string>();
            for (int j = 0; j < partes.Length; j++)
            {
                fila.Add(partes[j]);
            }
            filas.Add(fila);
        }

        return (encabezado, filas); 

     }
     
     }




