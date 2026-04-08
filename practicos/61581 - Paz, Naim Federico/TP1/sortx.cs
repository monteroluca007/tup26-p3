using System;
using System.Collections.Generic;
using System.IO;


try
{
    var config = ParseArgs(args);

    var inputText = ReadInput(config);

    var data = ParseDelimited(inputText, config);

    var sorted = SortRows(data, config);

    var outputText = Serialize(sorted, config);

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

List<Dictionary<string, string>> ParseDelimited(string text, AppConfig config)
{
    var filas = new List<Dictionary<string, string>>();

    if (string.IsNullOrWhiteSpace(text))
    {
        return filas;
    }

    var lineas = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    if (lineas.Length == 0)
    {
        return filas;
    }

    var encabezados = lineas[0].Trim().Split(config.Delimiter);

    for (int i = 1; i < lineas.Length; i++)
    {
        var linea = lineas[i].Trim();

        if (linea == "")
        {
            continue;
        }

        var campos = linea.Split(config.Delimiter);
        var fila = new Dictionary<string, string>();

        for (int j = 0; j < encabezados.Length; j++)
        {
            var valor = "";

            if (j < campos.Length)
            {
                valor = campos[j].Trim();
            }

            fila[encabezados[j].Trim()] = valor;
        }

        filas.Add(fila);
    }

    return filas;
}

List<Dictionary<string, string>> SortRows(
    List<Dictionary<string, string>> data,
    AppConfig config
)
{
    return data;
}

string Serialize(
    List<Dictionary<string, string>> data,
    AppConfig config
)
{
    var salida = "";

    foreach (var fila in data)
    {
        foreach (var par in fila)
        {
            salida += par.Key + "=" + par.Value + " ";
        }

        salida += "\n";
    }

    return salida;
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
