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