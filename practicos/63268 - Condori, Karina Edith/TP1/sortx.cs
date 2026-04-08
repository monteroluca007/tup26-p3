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
(List<string> header, List<Dictionary<string,string>> rows) = ParseDelimited(text, config);
    List<Dictionary<string, string>> sortedRows = SortRows(rows, config.SortFields);
    string outputText = WriteOutput(header, sortedRows, config);
    WriteOutputFile(outputText, config);
    }
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
 
 string Serialize(List<Dictionary<string,string>> rows, List<string> header, AppConfig config)
{
    var sb = new StringBuilder();

}

