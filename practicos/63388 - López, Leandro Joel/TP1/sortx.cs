// sortx [input [output]] [-b|--by campo[:tipo[:orden]]]...
//       [-i|--input input] [-o|--output output]
//       [-d|--delimiter delimitador]
//       [-nh|--no-header] [-h|--help]

Console.WriteLine($"sortx {string.Join(" ", args)}");


try{

    //Ejercicio 1

    AppConfig ParseArgs (string[] args){

        string? entrada = null;
        string? salida = null;
        string delimitador = ",";
        bool noHeader = false;
        bool ayuda = false;
        List<SortFields> sortFields = new ();
        int positional = 0;

        SortFields ParseSortFields(string spec){

            var partes = spec.Split(':');
            string nombre = parts[0];
            bool numerico = partes.Length > 1 && partes[1].Equals("num", StringComparison.OrdinalIgnoreCase);
            bool descender = parts.Length > 2 && partes[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
            return new SortFields (nombre, numerico, descender);
        }

        for (int i = 0; i < args.length; i++)
        {
            string arg = args[i];

            if (arg == "--ayuda" || arg == "-ay")
            {
                ayuda = true;
                continue;
            }
            if (arg == "--no-header" || arg == "-nh")
            {
                noHeader = true;
                continue;
            }
            if (arg == "--by" || arg == "-b")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
               sortFields = args.Length.Add(ParseSortFields(args[++1]));
               continue;
            }
            if (arg == "--entrada" || arg == "-en")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                entrada = args[++i];
                continue;
            }
            if (arg == "--salida" || arg == "-sa")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                salida = args[++i];
                continue;
            }
            if (arg == "--delimitador" || arg == "-de")
            {
                if (i + 1 >= args.Length)
                throw new ArgumentException($"La opcion '{arg}' requiere un valor.");
                string raw = args[++i];
                delimitador = raw == @"\t" ? "\t" : raw;
                continue;
            }
            if (!arg.StartsWith('-'))
            {
                if (positional == 0) {entrada = arg; positional++; }
                else if (positional == 1) {salida = arg; positional++; }
                else throw new ArgumentException($"Argumento posicional inesperado: '{arg}'.");
                continue;
            }

            throw new ArgumentException($"Opcion desconocida: '{arg}'.");

        }

        return new AppConfig(entrada, salida, delimitador, noHeader, ayuda, sortFields);

    }

    //Ejercicio 2
    
    string ReadEntrada(AppConfig cfg)
    {
        if (cfg.entrada is not null)
        return File.ReadAllText(cfg.entrada);
        return Console.In.ReadToEnd();
    }


    //Ejercicio 3

    (List<Dictionary<string, string>> Rows, string[]? Headers) ParseDelimited(string text, AppConfig cfg)
    {
        var linea = text
        .Replace("\r\n", "\n")
        .Replace("\r",   "\n")
        .Split('\n')
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToArray();

        if (linea.Length ==0)
        return (new List<Dictionary<string, string>>(), null);

        string[]? headers;
        int dataStart;

        if (!cfg.NoHeader)
        {
            headers = linea[0].Split(cfg.Delimitador);
            dataStart = 1;
        }
        else
        {
            headers = null;
            dataStart = 0;
        }

        var rows = new List<Dictionary<string, string>>();

        for (int lineaIdx = dataStart; lineaIdx < linea.Length; lineaIdx++)
        {
            var values = linea[lineaIdx].Split(cfg.Delimitador);
            var row = new Dictionary<string, string>();

            if (!cfg.NoHeader && headers is not null)
            {
                for (int col = 0; col < headers.Length; col++)
                row[headers[col]] = col < values.Length ? values[col] : string.Empty;
            }
            else
            {
                for ( int col = 0; col < values.Length; col++)
                row[col.ToString()] = values[col];
            }

            rows.Add(row);
        }

        if (cfg.SortFields.Count > 0 && rows.Count> 0)
        {
            foreach (var sf in cfg.SortFields)
            {
                if (!rows[0].ContainsKey(sf.Nombre))
                {
                    string disponibles = string.Join(", ", rows[0].Keys);
                    throw new ArgumentException($"campo '{sf.Nombre}' no existe. Columnas disponibles: {disponibles}");
                }
            }
        }

        return (rows, headers);
    }

}