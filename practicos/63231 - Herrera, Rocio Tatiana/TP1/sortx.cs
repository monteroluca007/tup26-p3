
// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

using System;
using System.Collections.Generic;
using System.IO;    
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;

record SortField(string Name,bool Numeric,bool descending);
record Appconfig(string ?InputFile, string ?OutputFile, char Delimiter, bool NoHeader, List<SortField> Fields);

var config=ParseArgs(args);
var text=ReadInput(config);
var (header,rows)=ParseDelimited(text,config.Delimiter,config.NoHeader);
SortRows(rows,config.Fields);
var output=Serialize(header,rows,config);
WriteOutput(output,config);

Appconfig ParseArgs(string [] a)
{
  string ?inputFile=null,outputFile=null;
  string delimiter=",";
   bool noHeader=false;
   var fields=new List<SortField>();

   for(int i = 0; i < a.Length; i++)
   {
      
      var x=a[i];
      if (x=="-i" || x == "--input") {if (++i>a.Length)
      Exiterror("Falta argumento para -i");
      inputFile=a[i];}
      else if (x=="-o" || x == "--output") {if (++i>a.Length)      
      Exiterror("Falta argumento para -o");
      outputFile=a[i];}
      else if (x=="-d" || x == "--delimiter") {if (++i>a.Length)
      Exiterror("Falta argumento para -d");
      delimiter=Unescape(a[i]); }
      
      else if (x=="-nh" || x == "--no-header"){noHeader=true;}
      else if (x=="-b" || x == "--by"){
      if (++i>a.Length) Exiterror("Falta argumento para -b");
      var p = a[i].Split(':');
      bool num=p.Length>1 && p[1].ToLowerInvariant()=="num";
      bool desc=p.Length>2 && p[2].ToLowerInvariant()=="desc";
      fields.Add(new SortField(p[0],num,desc));}
      else if(x=="-h" || x == "--help") {PrintHelp(); Environment.Exit(0);}
      else if (x.StartsWith("-")){
         Exiterror("opcion desconocida: "+x);
      }
      else {
         if(inputFile==null) inputFile=x;
         else if(outputFile==null) outputFile=x;
         else Exiterror("demasiados argumentos posicionales");
      }
    
   }
return new Appconfig(inputFile,outputFile,delimiter,noHeader,fields);
}

string ReadInput(Appconfig cfg){try
}
return cfg.InputFile==null?Console.In.ReadToEnd():File.ReadAllText(cfg.InputFile);
catch(Exception ex){Exiterror("Error leyendo entrada; return "";}}

(string [] header List<string[]> rows) ParseDelimited(string text, string delimi, bool noHeader)
{
   text=text.Replace("\r\n","\n").Replace("\r","\n");
   var lines=text.Split('\n';
   StringSplitOptions.None).Tolist();
   if(lines.Couunt>0 && lines.Last()=="") lines.RemoveAt(lines.Count-1);
   if(lines.Count==0) return (Array.Empty<string>(),new List<string[]>());

   var first=lines[0].Split(delimi);
   StringSplitOptions.None);
   int cols=first.Length;
   var rows=new List<string[]>();
   string [] header;

   if(noHeader) {header=Enumerable.Range(0,cols).Select(n=>n.ToString()).ToArray();
   rows.Add(Pad(first,cols));
   }
   else {header=first;
   }
   for(int i=1;i<lines.Count;i++)
   {
     rows.Add(Pad(lines[i].Split(delimi,StringSplitOptions.None),cols));

    }
   return (header,rows);
}
string [] Pad(string [] arr, int n)
{
   if(arr.Length==n) return arr;
   var r=new string[n];
   Array.Copy(arr,r,Math.Min(arr.Length,n));
   for(int i=arr.Length;i<n;i++) r[i]="";
   return r;
}