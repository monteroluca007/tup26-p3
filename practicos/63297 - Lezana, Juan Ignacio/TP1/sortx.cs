using System;
using System.Collections.Generic;
using System.IO;

try
{
    var config = ParseArgs(args);
    var text = ReadInput(config);
    var rows = ParseDelimited(text, config);
    var sorted = SortRows(rows, config);
    var output = Serialize(sorted, config);
    WriteOutput(output, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
}

AppConfig ParseArgs(string[] args) => throw new NotImplementedException();
string ReadInput(AppConfig config) => throw new NotImplementedException();
List<Dictionary<string,string>> ParseDelimited(string text, AppConfig config) => throw new NotImplementedException();
List<Dictionary<string,string>> SortRows(List<Dictionary<string,string>> rows, AppConfig config) => throw new NotImplementedException();
string Serialize(List<Dictionary<string,string>> rows, AppConfig config) => throw new NotImplementedException();
void WriteOutput(string output, AppConfig config) => throw new NotImplementedException();

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string?         InputFile,
    string?         OutputFile,
    string          Delimiter,
    bool            NoHeader,
    List<SortField> SortFields
);
