
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;

record SortField(string Name, bool Numeric, bool Descending);
record AppConfig(string? InputFile, string? OutputFile, string Delimiter, bool NoHeader, List<SortField> SortFields);

var cfg = ParseArgs(args);
var text = ReadInput(cfg);
var (header, rows) = ParseDelimited(text, cfg.Delimiter, cfg.NoHeader);
SortRows(rows, header, cfg.SortFields);
var output = Serialize(header, rows, cfg);
WriteOutput(output, cfg);

AppConfig ParseArgs(string[] a){
    string? inputFile = null, outputFile = null;
    string delim = ",";
    bool noHeader = false;
    var fields = new List<SortField>();
    for(int k=0;k<a.Length;k++){
        var x = a[k];
        if(x=="-i"||x=="--input"){ if(++k>=a.Length) ExitWithError("falta argumento para -i"); inputFile = a[k]; }
        else if(x=="-o"||x=="--output"){ if(++k>=a.Length) ExitWithError("falta argumento para -o"); outputFile = a[k]; }
        else if(x=="-d"||x=="--delimiter"){ if(++k>=a.Length) ExitWithError("falta argumento para -d"); delim = Unescape(a[k]); }
        else if(x=="-nh"||x=="--no-header"){ noHeader = true; }
        else if(x=="-b"||x=="--by"){
            if(++k>=a.Length) ExitWithError("falta especificación para -b");
            var p = a[k].Split(':');
            bool num = p.Length>1 && p[1].ToLowerInvariant()=="num";
            bool desc = p.Length>2 && p[2].ToLowerInvariant()=="desc";
            fields.Add(new SortField(p[0], num, desc));
        }
        else if(x=="-h"||x=="--help"){ PrintHelp(); Environment.Exit(0); }
        else if(!x.StartsWith("-")){ if(inputFile==null) inputFile = x; else if(outputFile==null) outputFile = x; else ExitWithError("demasiados argumentos posicionales"); }
        else ExitWithError("opción desconocida: "+x);
    }
    return new AppConfig(inputFile, outputFile, delim, noHeader, fields);
}

