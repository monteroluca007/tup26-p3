using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

try
{
    var config = ParseArgs(args);
    var texto = ReadInput(config);
    var filas = ParseDelimited(texto, config);
    var ordenadas = SortRows(filas, config);
    var salida = Serialize(ordenadas, config);
    WriteOutput(salida, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    return new AppConfig(null, null, ",", false, new List<SortField>());
}

string ReadInput(AppConfig config)
{
    return "";
}

List<Dictionary<string, string>> ParseDelimited(string texto, AppConfig config)
{
    return new List<Dictionary<string, string>>();
}

List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> filas, AppConfig config)
{
    return filas;
}

string Serialize(List<Dictionary<string, string>> filas, AppConfig config)
{
    return "";
}

void WriteOutput(string salida, AppConfig config)
{
    Console.WriteLine(salida);
}

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);