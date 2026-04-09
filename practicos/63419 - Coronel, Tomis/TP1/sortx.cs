using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
 
record SortField(string Name, bool Numeric, bool Desc);

record AppConfig(
    string Input,
    string Output,
    string Delim,
    bool NoHeader,
    List<SortField> Fields
);
class program
{
    static void Main(string[] args)
    {
try
{
    var cfg = ParseArgs(args);
    var txt = ReadInput(cfg);
    var data = Parse(cfg, txt);
    var ordenado = Sort(cfg, data.rows);
    var salida = Serialize(cfg, data.header, ordenado);
    Write(cfg, salida);
}
catch (Exception e)
{
    Console.Error.WriteLine("Error: " + e.Message);
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    string input = null;
    string output = null;
    string delim = ",";
    bool noHeader = false;
    var fields = new List<SortField>();

    int i = 0;
    while (i < args.Length)
    {
        var a = args[i];

        if (a == "-h" || a == "--help")
        {
            Console.WriteLine("uso: sortx [input] [-b campo]");
            Environment.Exit(0);
        }
        else if (a == "-i") input = args[++i];
        else if (a == "-o") output = args[++i];
        else if (a == "-d")
        {
            delim = args[++i];
            if (delim == "\\t") delim = "\t";
        }
        else if (a == "-nh") noHeader = true;
        else if (a == "-b")
        {
            var p = args[++i].Split(':');
            string nombre = p[0];
            bool num = p.Length > 1 && p[1] == "num";
            bool desc = p.Length > 2 && p[2] == "desc";
            fields.Add(new SortField(nombre, num, desc));
        }
        else
        {
            if (input == null) input = a;
            else if (output == null) output = a;
        }
        i++;
    }

    if (fields.Count == 0) throw new Exception("falta -b");

    return new AppConfig(input, output, delim, noHeader, fields);
}

string ReadInput(AppConfig c)
{
    return c.Input != null ? File.ReadAllText(c.Input) : Console.In.ReadToEnd();
}

(List<string> header, List<Dictionary<string, string>> rows) Parse(AppConfig c, string txt)
{
    var lineas = txt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var header = new List<string>();
    var rows = new List<Dictionary<string, string>>();
    int start = 0;

    if (!c.NoHeader)
    {
        header = lineas[0].Split(c.Delim).ToList();
        start = 1;
    }

    for (int i = start; i < lineas.Length; i++)
    {
        var partes = lineas[i].Split(c.Delim);
        var fila = new Dictionary<string, string>();

        for (int j = 0; j < partes.Length; j++)
        {
            string key = c.NoHeader ? j.ToString() : header[j];
            fila[key] = partes[j];
        }

        rows.Add(fila);
    }

    return (header, rows);
}

List<Dictionary<string, string>> Sort(AppConfig c, List<Dictionary<string, string>> rows)
{
    rows.Sort((a, b) =>
    {
        foreach (var f in c.Fields)
        {
            var v1 = a[f.Name];
            var v2 = b[f.Name];
            int comp;

            if (f.Numeric)
                comp = double.Parse(v1).CompareTo(double.Parse(v2));
            else
                comp = string.Compare(v1, v2);

            if (comp != 0)
                return f.Desc ? -comp : comp;
        }
        return 0;
    });

    return rows;
}

string Serialize(AppConfig c, List<string> header, List<Dictionary<string, string>> rows)
{
    var lines = new List<string>();

    if (!c.NoHeader)
        lines.Add(string.Join(c.Delim, header));

    foreach (var r in rows)
    {
        var vals = new List<string>();

        if (c.NoHeader)
        {
            int i = 0;
            while (r.ContainsKey(i.ToString()))
            {
                vals.Add(r[i.ToString()]);
                i++;
            }
        }
        else
        {
            foreach (var h in header)
                vals.Add(r[h]);
        }

        lines.Add(string.Join(c.Delim, vals));
    }

    return string.Join("\n", lines);
}

void Write(AppConfig c, string outp)
{
    if (c.Output != null)
        File.WriteAllText(c.Output, outp);
    else
        Console.Write(outp);
}
    }
}