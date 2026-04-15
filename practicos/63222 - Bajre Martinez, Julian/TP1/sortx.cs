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
}