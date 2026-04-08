using System;
using System.Collections.Generic;


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
    foreach (var a in args)
    {
        if (a == "-h" || a == "--help")
        {
            ShowHelp();
            Environment.Exit(0);
        }
    }

    return new AppConfig(
        null,
        null,
        ",",
        false,
        new List<SortField>()
    );
}

string ReadInput(AppConfig config)
{
    return "";
}

List<Dictionary<string, string>> ParseDelimited(string text, AppConfig config)
{
    return new List<Dictionary<string, string>>();
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
    return "";
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
