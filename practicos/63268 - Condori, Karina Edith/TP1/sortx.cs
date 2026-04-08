using System;
using System.Linq;
Console.WriteLine($"sortx {string.Join(" ", args)}");
record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields


try
{
    AppConfig config = ParseArgs(Environment.GetCommandLineArgs().Skip(1).ToArray());
    string text = ReadInput(config);
