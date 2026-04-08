
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");


try
{
    var config = ParsearArgumentos(args);

    var textoEntrada = LeerEntrada(config);

    var (filas, encabezados) = ParsearDelimitado(textoEntrada, config);

    var filasOrdenadas = OrdenarFilas(filas, encabezados, config);

    var textoSalida = Serializar(filasOrdenadas, encabezados, config);

    EscribirSalida(textoSalida, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    Environment.Exit(1);
}

AppConfig ParsearArgumentos(string[] args)
{
    string? archivoEntrada = null;
    string? archivoSalida = null;
    string delimitador = ",";
    bool sinEncabezado = false;
    bool mostrarAyuda = false;
    var camposOrden = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (arg == "-i" || arg == "--input")
        {
            archivoEntrada = args[++i];
        }
        else if (arg == "-o" || arg == "--output")
        {
            archivoSalida = args[++i];
        }
        else if (arg == "-d" || arg == "--delimiter")
        {
            var valor = args[++i];
            delimitador = valor == "\\t" ? "\t" : valor;
        }
        else if (arg == "-nh" || arg == "--no-header")
        {
            sinEncabezado = true;
        }
        else if (arg == "-ay" || arg == "--ayuda")
        {
            mostrarAyuda = true;
        }
        else if (arg == "-b" || arg == "--by")
        {
            var especificacion = args[++i];
            camposOrden.Add(ParsearCampoOrden(especificacion));
        }
        else
        {
            if (archivoEntrada == null)
                archivoEntrada = arg;
            else if (archivoSalida == null)
                archivoSalida = arg;
        }
    }

    return new AppConfig(
        archivoEntrada,
        archivoSalida,
        delimitador,
        sinEncabezado,
        mostrarAyuda,
        camposOrden
    );
}


record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);




