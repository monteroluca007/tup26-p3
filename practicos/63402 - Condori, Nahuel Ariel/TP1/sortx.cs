using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

try
{
    var config = ParseArgs(args);
    if (config == null) return 0;

    var rawText = ReadInput(config);
    var rows = ParseDelimited(rawText, config, out var header);
    var sortedRows = SortRows(rows, config);
    var outputText = Serialize(sortedRows, config, header);
    WriteOutput(outputText, config);

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}