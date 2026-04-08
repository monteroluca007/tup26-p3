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

}