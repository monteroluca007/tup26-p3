using System;
using System.Collections.Generic;
using System.IO;


try
{
    var config = ParseArgs(args);

    var inputText = ReadInput(config);

    var table = ParseDelimited(inputText, config);

    var sortedRows = SortRows(table.Rows, config);

    var outputText = Serialize(table.Headers, sortedRows, config);

    WriteOutput(config, outputText);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}



AppConfig ParseArgs(string[] args)
{
    string? inputFile = null;
    string? outputFile = null;

    foreach (var a in args)
    {
        if (a == "-h" || a == "--help")
        {
            ShowHelp();
            Environment.Exit(0);
        }
       // ignorar "--"
        if (a == "--")
        {
            continue;
        }

        // argumentos posicionales
        if (inputFile == null)
        {
            inputFile = a;
        }
        else if (outputFile == null)
        {
            outputFile = a;
        }
    }

    return new AppConfig(
        inputFile,
        outputFile,
        ",",
        false,
        new List<SortField>()
    );
}

string ReadInput(AppConfig config)
{
    if (config.InputFile != null)
    {
        return File.ReadAllText(config.InputFile);
    }
    else
    {
        return Console.In.ReadToEnd();
    }
}

(List<string> Headers, List<Dictionary<string, string>> Rows) ParseDelimited(string text, AppConfig config)
{
    var headers = new List<string>();
    var rows = new List<Dictionary<string, string>>();

    if (string.IsNullOrWhiteSpace(text))
    {
        return (headers, rows);
    }

    var lineas = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    if (lineas.Length == 0)
    {
        return (headers, rows);
    }

    var primeraLinea = lineas[0].Trim();
    var encabezados = primeraLinea.Split(config.Delimiter);

    foreach (var encabezado in encabezados)
    {
        headers.Add(encabezado.Trim());
    }

    for (int i = 1; i < lineas.Length; i++)
    {
        var linea = lineas[i].Trim();

        if (linea == "")
        {
            continue;
        }

        var campos = linea.Split(config.Delimiter);
        var fila = new Dictionary<string, string>();

        for (int j = 0; j < headers.Count; j++)
        {
            var valor = "";

            if (j < campos.Length)
            {
                valor = campos[j].Trim();
            }

            fila[headers[j]] = valor;
        }

        rows.Add(fila);
    }

    return (headers, rows);
}

List<Dictionary<string, string>> SortRows(
    List<Dictionary<string, string>> data,
    AppConfig config
)
{
    return data;
}

string Serialize(
    List<string> headers,
    List<Dictionary<string, string>> data,
    AppConfig config
)
{
    var lineas = new List<string>();

    lineas.Add(string.Join(config.Delimiter, headers));

    foreach (var fila in data)
    {
        var valores = new List<string>();

        foreach (var header in headers)
        {
            if (fila.ContainsKey(header))
            {
                valores.Add(fila[header]);
            }
            else
            {
                valores.Add("");
            }
        }

        lineas.Add(string.Join(config.Delimiter, valores));
    }

    return string.Join("\n", lineas);
}

void WriteOutput(AppConfig config, string text)
{
    Console.Write(text);
}

void ShowHelp()
{
    Console.WriteLine("Uso: sortx [opciones] [input] [output]");
    Console.WriteLine("  -h, --help   Muestra ayuda");
}


record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);
