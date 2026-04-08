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
    string? input = null;
    string? output = null;
    var sortFields = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        var a = args[i];

        if (a == "-h" || a == "--help")
        {
            ShowHelp();
            Environment.Exit(0);
        }
        else if (a == "-i" || a == "--input")
        {
            if (i + 1 >= args.Length)
            {
                throw new Exception("Falta archivo después de -i");
            }

            input = args[i + 1];
            i++;
        }
        else if (a == "-o" || a == "--output")
        {
            if (i + 1 >= args.Length)
            {
                throw new Exception("Falta archivo después de -o");
            }

            output = args[i + 1];
            i++;
        }
        else if (a == "-b" || a == "--by")
        {
            if (i + 1 >= args.Length)
            {
                throw new Exception("Falta el campo después de -b");
            }

            var partes = args[i + 1].Split(':');

            var nombre = partes[0];
            var numeric = false;
            var descending = false;

            if (partes.Length >= 2)
            {
                if (partes[1] == "num") numeric = true;
                else if (partes[1] == "alpha") numeric = false;
                else throw new Exception("Tipo inválido");
            }

            if (partes.Length >= 3)
            {
                if (partes[2] == "desc") descending = true;
                else if (partes[2] == "asc") descending = false;
                else throw new Exception("Orden inválido");
            }

            sortFields.Add(new SortField(nombre, numeric, descending));
            i++;
        }
        else if (a == "--")
        {
            continue;
        }
        else if (input == null)
        {
            input = a;
        }
        else if (output == null)
        {
            output = a;
        }
    }

    return new AppConfig(
        input,
        output,
        ",",
        false,
        sortFields
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
    if (config.SortFields.Count == 0)
    {
        return data;
    }

    data.Sort(CompararFilas);

    return data;

    int CompararFilas(Dictionary<string, string> a, Dictionary<string, string> b)
    {
        foreach (var field in config.SortFields)
        {
            var valorA = "";
            var valorB = "";

            if (a.ContainsKey(field.Name))
            {
                valorA = a[field.Name];
            }

            if (b.ContainsKey(field.Name))
            {
                valorB = b[field.Name];
            }

            int resultado = 0;

            if (field.Numeric)
            {
                var numeroA = int.Parse(valorA);
                var numeroB = int.Parse(valorB);
                resultado = numeroA.CompareTo(numeroB);
            }
            else
            {
                resultado = string.Compare(valorA, valorB);
            }

            if (field.Descending)
            {
                resultado = resultado * -1;
            }

            if (resultado != 0)
            {
                return resultado;
            }
        }

        return 0;
    }
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
    if (config.OutputFile != null)
    {
        File.WriteAllText(config.OutputFile, text);
    }
    else
    {
        Console.Write(text);
    }
}

void ShowHelp()
{
    Console.WriteLine("Uso: sortx [opciones] [input] [output]");
    Console.WriteLine("  -h, --help   Muestra ayuda");
    Console.WriteLine("  -b, --by     Campo de ordenamiento");
    Console.WriteLine("               Formato: campo[:alpha|num[:asc|desc]]");
}


record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields
);
