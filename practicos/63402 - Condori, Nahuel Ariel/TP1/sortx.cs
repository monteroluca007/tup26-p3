using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

record SortField(string Name, bool Numeric, bool Descending);

record AppConfig(
    string? InputFile,
    string? OutputFile,
    string Delimiter,
    bool NoHeader,
    List<SortField> SortFields);