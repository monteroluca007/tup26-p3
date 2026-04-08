
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





record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);




