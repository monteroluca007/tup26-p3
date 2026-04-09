namespace Tup26.AlumnosApp;

class GitHub {
    readonly string owner;
    readonly string repo;

    public GitHub(string owner = "AlejandroDiBattista", string repo = "tup26-p3") {
        this.owner = owner;
        this.repo = repo;
    }

    public bool AgregarColaborador(string usuario) {
        (string salida, int codigoSalida) = EjecutarGH(
            $"Error al agregar colaborador '{usuario}'",
            new[] { "api", "--method", "PUT", $"repos/{owner}/{repo}/collaborators/{usuario}", "-f", "permission=push" });

        if (codigoSalida != 0) {
            return false;
        }

        return true;
    }

    public List<string> ListarColaboradores() {
        (string salida, int codigoSalida) = EjecutarGH(
            "Error al listar colaboradores",
            new[] { "api", $"repos/{owner}/{repo}/collaborators", "--jq", ".[] | select(.permissions.push == true) | .login" });

        if (codigoSalida != 0) {
            return new();
        }

        return LeerLineas(salida);
    }

    public List<string> ListarInvitacionesPendientes() {
        (string salida, int codigoSalida) = EjecutarGH(
            "Error al listar invitaciones pendientes",
            new[] { "api", $"repos/{owner}/{repo}/invitations", "--paginate", "--jq", ".[].invitee.login" });

        if (codigoSalida != 0) {
            return new();
        }

        return LeerLineas(salida);
    }

    public List<(int Numero, string Titulo)> ListarPR(bool soloAbiertos = true) {
        string estado = soloAbiertos ? "open" : "all";

        (string salida, int codigoSalida) = EjecutarGH(
            "Error al listar PRs",
            new[] { "api", $"repos/{owner}/{repo}/pulls?state={estado}", "--paginate", "--jq", @".[] | ""\(.number)\t\(.title)""" });

        if (codigoSalida != 0) {
            return new();
        }

        List<(int Numero, string Titulo)> prs = new();
        foreach (string linea in LeerLineas(salida, pasarAMinusculas: false)) {
            string[] partes = linea.Split('\t', 2);

            if (partes.Length != 2) { continue; }
            if (!int.TryParse(partes[0], out int numero)) { continue; }

            prs.Add((numero, partes[1]));
        }
        prs.Sort((a, b) => a.Numero.CompareTo(b.Numero));
        return prs;
    }

    public static int ExtraerTP(string titulo) {
        Match match = Regex.Match(titulo, @"\bTP\d+\b", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Value[2..]) : 0;
    }

    public static int ListarPRSinLegajo(GitHub gh) {
        List<(int Numero, string Titulo)> prs = gh.ListarPR();
        int count = 0;

        foreach ((int Numero, string Titulo) pr in prs) {
            if (ExtraerLegajo(pr.Titulo) == 0) {
                if (count++ == 0) {
                    Console.WriteLine("= PR Sin legajo válido =");
                }

                Console.WriteLine($"- #{pr.Numero}: {pr.Titulo}");
            }
        }

        if (count == 0) {
            Console.WriteLine("Todos los PRs tienen un legajo válido en el título.");
        } else {
            Console.WriteLine($"Total de PRs sin legajo válido: {count}");
        }

        return count;
    }

    public static int ListarPRConConflictos(GitHub gh) {
        List<(int Numero, string Titulo)> prs = gh.ListarPR();
        int count = 0;

        foreach ((int Numero, string Titulo) pr in prs) {
            (string Estado, bool EsMergeable) detallePr = gh.ObtenerEstado(pr.Numero);

            if (detallePr.EsMergeable == false) {
                if (count++ == 0) {
                    Console.WriteLine("= PR con conflictos =");
                }

                Console.WriteLine($"- #{pr.Numero}: {pr.Titulo}");
            }
        }

        if (count == 0) {
            Console.WriteLine("No se encontraron PRs con conflictos.");
        } else {
            Console.WriteLine($"Total de PRs con conflictos: {count}");
        }

        return count;
    }

    public static int NormalizarTitulosPR(GitHub gh, Alumnos alumnos, bool simular = false) {
        List<(int Numero, string Titulo)> prs = gh.ListarPR();
        int count = 0;

        foreach ((int Numero, string Titulo) pr in prs) {
            int legajo = ExtraerLegajo(pr.Titulo);
            Alumno? alumno = alumnos.BuscarPorLegajo(legajo);

            if (alumno != null) {
                string nuevoTitulo = $"{legajo} - TP{ExtraerTP(pr.Titulo)} - {alumno.NombreCompleto}";

                if (nuevoTitulo != pr.Titulo) {
                    if (count++ == 0) {
                        Console.WriteLine("= PRs a actualizar =");
                    }

                    Console.WriteLine($"Actualizando PR #{pr.Numero}:\n > {pr.Titulo}\n < {nuevoTitulo}");

                    if (!simular) {
                        gh.CambiarTituloPR(pr.Numero, nuevoTitulo);
                    }
                }
            }
        }

        if (count == 0) {
            Console.WriteLine("No se encontraron PRs para actualizar.");
        } else {
            Console.WriteLine($"Total de PRs a actualizar: {count}");
        }

        return count;
    }

    public (string Estado, bool EsMergeable) ObtenerEstado(int numeroPR) {
        (string salida, int codigoSalida) = EjecutarGH(
            $"Error al consultar el estado del PR #{numeroPR}",
            new[] { "api", $"repos/{owner}/{repo}/pulls/{numeroPR}", "--jq", @"""\(.state)\t\(.mergeable)""" });

        if (codigoSalida != 0) {
            return (string.Empty, false);
        }

        string[] partes = salida.Trim().Split('\t', 2);

        if (partes.Length != 2) {
            return (string.Empty, false);
        }

        return (partes[0].ToLower(), partes[1].ToLower() == "true");
    }

    public bool MergeAutomatico(int numeroPR) {
        var detalle = ObtenerEstado(numeroPR);

        if (!string.Equals(detalle.Estado, "open")) {
            Console.WriteLine($"Error al mergear el PR #{numeroPR}: el PR no está abierto.");
            return false;
        }

        (string salida, int codigoSalida) = EjecutarGH(
            $"Error al mergear el PR #{numeroPR}",
            new[] { "pr", "merge", numeroPR.ToString(), "--repo", $"{owner}/{repo}", "--auto", "--merge" });

        return (codigoSalida == 0);
    }

    public int MergearTP(int numeroTP) {
        if (numeroTP <= 0) {
            Console.WriteLine("Error al mergear PRs: el número de TP debe ser mayor a cero.");
            return 0;
        }

        List<(int Numero, string Titulo)> prs = ListarPR(soloAbiertos: true)
            .Where(pr => EsPRDeTP(pr.Titulo, numeroTP))
            .ToList();

        if (prs.Count == 0) {
            Console.WriteLine($"No se encontraron PRs abiertos del TP {numeroTP}.");
            return 0;
        }

        int mergesRealizados = 0;

        foreach ((int Numero, string Titulo) pr in prs) {
            Console.WriteLine($"Mergeando PR #{pr.Numero}: {pr.Titulo}");

            if (MergeAutomatico(pr.Numero)) {
                mergesRealizados++;
            }
        }

        Console.WriteLine($"PRs mergeados del TP {numeroTP}: {mergesRealizados}/{prs.Count}");
        return mergesRealizados;
    }

    public bool CambiarTituloPR(int numeroPR, string nuevoTitulo) {
        if (string.IsNullOrWhiteSpace(nuevoTitulo)) {
            Console.WriteLine("Error al cambiar el título del PR: el nuevo título no puede estar vacío.");
            return false;
        }

        string titulo = nuevoTitulo.Trim();

        (string salida, int codigoSalida) = EjecutarGH(
            $"Error al cambiar el título del PR #{numeroPR}",
            new[] { "api", "--method", "PATCH", $"repos/{owner}/{repo}/pulls/{numeroPR}", "-f", $"title={titulo}" });

        return (codigoSalida == 0);
    }

    public List<(string Titulo, DateTimeOffset FechaHora)> ListarCommits(int numeroPR) {
        (string salida, int codigoSalida) = EjecutarGH(
            $"Error al listar commits del PR #{numeroPR}",
            new[] { "api", $"repos/{owner}/{repo}/pulls/{numeroPR}/commits", "--paginate", "--jq", @".[] | ""\(.commit.message | split(""\n"")[0])\t\(.commit.author.date)""" });

        if (codigoSalida != 0) {
            return new();
        }

        List<(string Titulo, DateTimeOffset FechaHora)> commits = new();

        foreach (string linea in LeerLineas(salida, pasarAMinusculas: false)) {
            string[] partes = linea.Split('\t', 2);

            if (partes.Length != 2) {
                continue;
            }

            if (!DateTimeOffset.TryParse(partes[1], out DateTimeOffset fechaHora)) {
                continue;
            }

            commits.Add((partes[0], fechaHora));
        }

        commits.Sort((a, b) => a.FechaHora.CompareTo(b.FechaHora));
        return commits;
    }

    
    (string Salida, int CodigoSalida) EjecutarGH(string mensajeError, IEnumerable<string> argumentos) {
        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "gh",
            RedirectStandardOutput = true,
            RedirectStandardError  = true
        };

        foreach (string argumento in argumentos) {
            startInfo.ArgumentList.Add(argumento);
        }

        using Process proceso = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar gh.");

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error  = proceso.StandardError.ReadToEnd().Trim();

        proceso.WaitForExit();

        if (proceso.ExitCode != 0) {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            Console.WriteLine($"{mensajeError}: {detalle}");
        }

        return (salida, proceso.ExitCode);
    }

    

    static List<string> LeerLineas(string texto, bool pasarAMinusculas = true) {
        return texto.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(linea => linea.Trim())
                    .Select(linea => pasarAMinusculas ? linea.ToLower() : linea)
                    .Where(linea => !string.IsNullOrWhiteSpace(linea))
                    .ToList();
    }

    static int ExtraerLegajo(string titulo) {
        Match match = Regex.Match(titulo, @"\b\d{5}\b");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    static bool EsPRDeTP(string titulo, int numeroTP) {
        return ExtraerTP(titulo) == numeroTP;
    }

    
}
