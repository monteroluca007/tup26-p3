using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

var config = ParseArgs(args);

AppConfig ParseArgs(string[] a){
    string? inputFile = null, outputFile = null;
    string delimiter = ",";
    bool noHeader = false;
    var fields = new List<SortField>();

    for(int i=0;i<a.Length;i++){
        var x = a[i];
        if(x=="-i"||x=="--input"){ if(++i>=a.Length) ExitError("falta argumento para -i"); inputFile = a[i]; }
        else if(x=="-o"||x=="--output"){ if(++i>=a.Length) ExitError("falta argumento para -o"); outputFile = a[i]; }
        else if(x=="-d"||x=="--delimiter"){ if(++i>=a.Length) ExitError("falta argumento para -d"); delimiter = Unescape(a[i]); }
        else if(x=="-nh"||x=="--no-header"){ noHeader = true; }
        else if(x=="-b"||x=="--by"){
            if(++i>=a.Length) ExitError("falta especificación para -b");
            var p = a[i].Split(':');
            bool num = p.Length>1 && p[1].ToLowerInvariant()=="num";
            bool desc = p.Length>2 && p[2].ToLowerInvariant()=="desc";
            fields.Add(new SortField(p[0], num, desc));
        }
        else if(x=="-h"||x=="--help"){ PrintHelp(); Environment.Exit(0); }
        else if(!x.StartsWith("-")){
            if(inputFile==null) inputFile = x;
            else if(outputFile==null) outputFile = x;
            else ExitError("demasiados argumentos posicionales");
        }
        else ExitError("opción desconocida: " + x);
    }

    return new AppConfig(inputFile, outputFile, delimiter, noHeader, fields);
}

static string Unescape(string s) => s.Replace("\\t","\t").Replace("\\n","\n").Replace("\\r","\r");
static void ExitError(string msg){ Console.Error.WriteLine(msg); Environment.Exit(1); }
static void PrintHelp(){
    Console.WriteLine("Uso: sortx [entrada [salida]] -b campo[:tipo[:orden]] [-d delim] [-nh] [-h]");
    Console.WriteLine("campo[:tipo[:orden]]: tipo = num (numérico). orden = desc (descendente)");
}
var text = ReadInput(config);

string ReadInput(AppConfig cfg){
    try{
        return cfg.InputFile == null ? Console.In.ReadToEnd() : File.ReadAllText(cfg.InputFile);
    } catch(Exception ex){
        ExitError("error leyendo entrada: " + ex.Message);
        return "";
    }
}
var (header, rows) = ParseDelimited(text, config.Delimiter, config.NoHeader);

(string[] header, List<string[]> rows) ParseDelimited(string text, string delim, bool noHeader){
    text = text.Replace("\r\n","\n").Replace("\r","\n");
    var lines = text.Split('\n', StringSplitOptions.None).ToList();
    if(lines.Count>0 && lines[^1]=="") lines.RemoveAt(lines.Count-1);
    if(lines.Count==0) return (Array.Empty<string>(), new List<string[]>());

    var first = lines[0].Split(new[]{delim}, StringSplitOptions.None);
    int cols = first.Length;
    var rows = new List<string[]>();
    string[] header;

    if(noHeader){
        header = Enumerable.Range(0,cols).Select(n=>n.ToString()).ToArray();
        rows.Add(Pad(first, cols));
    } else {
        header = first;
    }

    for(int i=1;i<lines.Count;i++){
        rows.Add(Pad(lines[i].Split(new[]{delim}, StringSplitOptions.None), cols));
    }
    return (header, rows);
}

string[] Pad(string[] arr, int n){
    if(arr.Length==n) return arr;
    var r = new string[n];
    Array.Copy(arr, r, Math.Min(arr.Length, n));
    for(int i=arr.Length;i<n;i++) r[i]="";
    return r;
}