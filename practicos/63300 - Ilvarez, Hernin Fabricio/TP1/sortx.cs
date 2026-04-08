using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;


record SortField(string name, bool Numeric, bool Descending);
record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields);


try
{
    var config = ParseArgs(args);
    var input = ReadInput(config);
    var rows = ParseDelimited(input, config, out string[] header);
    var sortedRows = SortRows(rows, header, config);
    var output = Serialize(sortedRows, header, config);
    WriteOutput(output, config);

} catch (Exception ex)
{
    Console.Error.WriteLine($"Error : {ex.Message}");
    Environment.Exit(1);
}