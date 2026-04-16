using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var config = ParsearArgumentos(args);
            var texto = LeerEntrada(config);
            var filas = ParsearDelimitado(texto, config);
            var filasOrdenadas = OrdenarFilas(filas, config);
            var salida = Serializar(filasOrdenadas, config);
            EscribirSalida(config, salida);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.Exit(1);
        }
    }

    static ConfiguracionApp ParsearArgumentos(string[] args)
    {
        string archivoEntrada = null;
        string archivoSalida = null;
        string delimitador = ",";
        bool sinEncabezado = false;
        var camposOrden = new List<CampoOrden>();

        int cantidadPosicionales = 0;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                case "--input":
                    ValidarSiguiente(args, i);
                    archivoEntrada = args[++i];
                    break;

                case "-o":
                case "--output":
                    ValidarSiguiente(args, i);
                    archivoSalida = args[++i];
                    break;

                case "-d":
                case "--delimiter":
                    ValidarSiguiente(args, i);
                    var d = args[++i];
                    delimitador = d == "\\t" ? "\t" : d;
                    break;

                case "-nh":
                case "--no-header":
                    sinEncabezado = true;
                    break;

                case "-h":
                case "--help":
                    MostrarAyuda();
                    Environment.Exit(0);
                    break;

                case "-b":
                case "--by":
                    ValidarSiguiente(args, i);
                    var partes = args[++i].Split(':');

                    string nombre = partes[0];
                    bool esNumerico = partes.Length > 1 && partes[1] == "num";
                    bool descendente = partes.Length > 2 && partes[2] == "desc";

                    camposOrden.Add(new CampoOrden(nombre, esNumerico, descendente));
                    break;

                default:
                    if (!args[i].StartsWith("-"))
                    {
                        if (cantidadPosicionales == 0)
                            archivoEntrada = args[i];
                        else if (cantidadPosicionales == 1)
                            archivoSalida = args[i];

                        cantidadPosicionales++;
                    }
                    break;
            }
        }

        if (camposOrden.Count == 0)
            throw new Exception("Debe especificar al menos un campo de ordenamiento (-b)");

        return new ConfiguracionApp(
            archivoEntrada,
            archivoSalida,
            delimitador,
            sinEncabezado,
            camposOrden
        );
    }

    static void ValidarSiguiente(string[] args, int i)
    {
        if (i + 1 >= args.Length)
            throw new Exception("Falta valor para argumento " + args[i]);
    }

    static void MostrarAyuda()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("-i | --input <archivo>");
        Console.WriteLine("-o | --output <archivo>");
        Console.WriteLine("-d | --delimiter <delimitador>");
        Console.WriteLine("-nh | --no-header");
        Console.WriteLine("-b | --by campo[:num][:desc]");
    }

    static string LeerEntrada(ConfiguracionApp config)
    {
        if (config.ArchivoEntrada != null)
            return File.ReadAllText(config.ArchivoEntrada);

        return Console.In.ReadToEnd();
    }

    

    