namespace Tup26.AlumnosApp;

class WAppService
{
    readonly string? store;
    readonly TimeSpan timeout;
    bool gruposHabilitados = true;
    List<GrupoWhatsApp> grupos = new();

    public WAppService(string? store = null, TimeSpan? timeout = null, bool refrescarGrupos = true)
    {
        this.store = store;
        this.timeout = timeout ?? TimeSpan.FromMinutes(5);

        try
        {
            CargarGrupos(refrescarGrupos);
        }
        catch (InvalidOperationException ex) when (EsErrorAutenticacionWacli(ex))
        {
            gruposHabilitados = false;
            grupos = new();
            Console.WriteLine("Aviso: wacli no está autenticado; se omite la carga de grupos.");
        }
    }

    public void EnviarMensajeAGrupo(string grupo, string mensaje, bool json = false)
    {
        string grupoJid = ResolverJidGrupo(grupo);
        EnviarTexto(grupoJid, mensaje, json);
    }

    public void EnviarTexto(string destinatario, string mensaje, bool json = false)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            throw new ArgumentException("El destinatario no puede estar vacío.", nameof(destinatario));
        }

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            throw new ArgumentException("El mensaje no puede estar vacío.", nameof(mensaje));
        }

        List<string> argumentos = new() { "send", "text", "--to", destinatario, "--message", mensaje };
        Ejecutar(json, argumentos);
    }

    public void InvitarParticipanteAGrupo(string grupo, params string[] usuarios)
    {
        InvitarParticipantesAGrupo(grupo, false, usuarios);
    }

    public void InvitarParticipantesAGrupo(string grupo, bool json, params string[] usuarios)
    {
        if (string.IsNullOrWhiteSpace(grupo))
        {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        if (usuarios is null || usuarios.Length == 0)
        {
            throw new ArgumentException("Debes indicar al menos un participante.", nameof(usuarios));
        }

        string grupoJid = ResolverJidGrupo(grupo);
        List<string> argumentos = new() { "groups", "participants", "add", "--jid", grupoJid };

        foreach (string usuario in usuarios)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                continue;
            }

            argumentos.Add("--user");
            argumentos.Add(usuario);
        }

        if (!argumentos.Any(argumento => argumento == "--user"))
        {
            throw new ArgumentException("Debes indicar al menos un participante válido.", nameof(usuarios));
        }

        Ejecutar(json, argumentos);
    }

    public void InvitarGrupoComision(IEnumerable<Alumno> alumnos, bool json = false)
    {
        if (alumnos is null)
        {
            throw new ArgumentNullException(nameof(alumnos));
        }

        List<Alumno> alumnosValidos = alumnos
            .Where(alumno =>
                !string.IsNullOrWhiteSpace(alumno.Comision) &&
                !string.IsNullOrWhiteSpace(alumno.TelefonoId))
            .ToList();

        foreach (IGrouping<string, Alumno> grupoComision in alumnosValidos
            .GroupBy(alumno => ObtenerReferenciaGrupoPorComision(alumno.Comision))
            .OrderBy(grupo => grupo.Key, StringComparer.OrdinalIgnoreCase))
        {
            string[] usuarios = grupoComision
                .Select(alumno => alumno.TelefonoId)
                .Where(telefono => !string.IsNullOrWhiteSpace(telefono))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (usuarios.Length == 0)
            {
                continue;
            }

            Console.WriteLine($"Invitando {usuarios.Length} alumno(s) al grupo {grupoComision.Key}...");
            InvitarParticipantesAGrupo(grupoComision.Key, json, usuarios);
        }
    }

    public List<ContactoWhatsApp> ListarParticipantesGrupo(string grupo, bool refrescar = false)
    {
        if (string.IsNullOrWhiteSpace(grupo))
        {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        string grupoJid = ResolverJidGrupoParaLectura(grupo, refrescar);

        if (refrescar && gruposHabilitados)
        {
            try
            {
                EjecutarYObtenerSalida(false, new() { "groups", "info", "--jid", grupoJid });
            }
            catch (InvalidOperationException ex) when (EsErrorAutenticacionWacli(ex))
            {
                gruposHabilitados = false;
                grupos = new();
                Console.WriteLine("Aviso: wacli no está autenticado; se usan los participantes de la base local.");
            }
        }

        return ListarParticipantesDesdeBaseLocal(grupoJid);
    }

    public string? BuscarJidGrupoPorDescripcion(string descripcion, bool refrescar = false)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException("La descripción no puede estar vacía.", nameof(descripcion));
        }

        GrupoWhatsApp? grupo = BuscarGrupoPorReferencia(descripcion, refrescar);
        return grupo?.Jid;
    }

    public void RecargarGrupos()
    {
        if (!gruposHabilitados)
        {
            Console.WriteLine("Aviso: no se pueden recargar grupos porque wacli no está autenticado.");
            return;
        }

        CargarGrupos(true);
    }

    public List<GrupoWhatsApp> ListarGrupos(bool refrescar = false)
    {
        if (!gruposHabilitados)
        {
            return new();
        }

        if (refrescar || grupos.Count == 0)
        {
            CargarGrupos(refrescar);
        }

        return new(grupos);
    }

    string ResolverJidGrupo(string grupo)
    {
        if (string.IsNullOrWhiteSpace(grupo))
        {
            throw new ArgumentException("El grupo no puede estar vacío.", nameof(grupo));
        }

        string referencia = grupo.Trim();

        if (EsJidGrupo(referencia))
        {
            return referencia;
        }

        GrupoWhatsApp? coincidencia = BuscarGrupoPorReferencia(referencia, false);
        if (coincidencia != null)
        {
            return coincidencia.Jid;
        }

        coincidencia = BuscarGrupoPorReferencia(referencia, true);
        if (coincidencia != null)
        {
            return coincidencia.Jid;
        }

        throw new InvalidOperationException($"No se encontró ningún grupo para '{referencia}'.");
    }

    string ResolverJidGrupoParaLectura(string grupo, bool refrescar)
    {
        string referencia = grupo.Trim();

        if (EsJidGrupo(referencia))
        {
            return referencia;
        }

        if (gruposHabilitados)
        {
            GrupoWhatsApp? coincidencia = BuscarGrupoPorReferencia(referencia, refrescar);
            if (coincidencia != null)
            {
                return coincidencia.Jid;
            }
        }

        string? grupoJid = BuscarJidGrupoEnBaseLocal(referencia);
        if (!string.IsNullOrWhiteSpace(grupoJid))
        {
            return grupoJid;
        }

        throw new InvalidOperationException($"No se encontró ningún grupo para '{referencia}'.");
    }

    string? BuscarJidGrupoEnBaseLocal(string referencia)
    {
        string valor = EscaparSqlite(referencia.Trim());

        List<string> coincidenciasExactas = EjecutarSqlite(
            $"SELECT jid FROM groups WHERE lower(name) = lower('{valor}') OR lower(jid) = lower('{valor}') ORDER BY name, jid;");

        if (coincidenciasExactas.Count == 1)
        {
            return coincidenciasExactas[0];
        }

        if (coincidenciasExactas.Count > 1)
        {
            throw new InvalidOperationException($"La referencia exacta '{referencia}' coincide con varios grupos locales.");
        }

        List<string> coincidenciasParciales = EjecutarSqlite(
            $"SELECT jid FROM groups WHERE lower(name) LIKE lower('%{valor}%') OR lower(jid) LIKE lower('%{valor}%') ORDER BY name, jid;");

        if (coincidenciasParciales.Count == 1)
        {
            return coincidenciasParciales[0];
        }

        if (coincidenciasParciales.Count > 1)
        {
            throw new InvalidOperationException($"La búsqueda '{referencia}' coincide con varios grupos locales.");
        }

        return null;
    }

    List<ContactoWhatsApp> ListarParticipantesDesdeBaseLocal(string grupoJid)
    {
        string jid = EscaparSqlite(grupoJid);
        string rutaSessionDb = EscaparSqlite(Path.Combine(ObtenerDirectorioStore(), "session.db"));

        List<string> filas = EjecutarSqlite(
            $@"ATTACH DATABASE '{rutaSessionDb}' AS session;
                SELECT
                    gp.user_jid || char(9) ||
                    COALESCE(
                        NULLIF(ca.alias, ''),
                        NULLIF(c.full_name, ''),
                        NULLIF(c.push_name, ''),
                        NULLIF(c.business_name, ''),
                        NULLIF(c.first_name, ''),
                        NULLIF(sc.full_name, ''),
                        NULLIF(sc.push_name, ''),
                        NULLIF(sc.business_name, ''),
                        NULLIF(sc.first_name, ''),
                        gp.user_jid
                    ) || char(9) ||
                    COALESCE(
                        NULLIF(c.phone, ''),
                        NULLIF(sc.redacted_phone, ''),
                        NULLIF(lm.pn, ''),
                        ''
                    )
                FROM group_participants gp
                LEFT JOIN session.whatsmeow_lid_map lm
                    ON replace(gp.user_jid, '@lid', '') = lm.lid
                LEFT JOIN contacts c
                    ON c.phone = lm.pn
                    OR c.jid = lm.pn || '@s.whatsapp.net'
                LEFT JOIN contact_aliases ca
                    ON ca.jid = gp.user_jid
                    OR ca.jid = lm.pn || '@s.whatsapp.net'
                LEFT JOIN session.whatsmeow_contacts sc
                    ON sc.their_jid = gp.user_jid
                    OR sc.their_jid = lm.pn || '@s.whatsapp.net'
                WHERE gp.group_jid = '{jid}'
                ORDER BY CASE gp.role WHEN 'superadmin' THEN 0 WHEN 'admin' THEN 1 ELSE 2 END, gp.user_jid;");

        return filas.Select(ParsearContactoWhatsApp).ToList();
    }

    static ContactoWhatsApp ParsearContactoWhatsApp(string fila)
    {
        string[] partes = fila.Split('\t');

        string jid = partes.Length > 0 ? partes[0].Trim() : string.Empty;
        string nombre = partes.Length > 1 ? partes[1].Trim() : string.Empty;
        string telefono = partes.Length > 2 ? partes[2].Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            nombre = jid;
        }

        if (string.IsNullOrWhiteSpace(telefono))
        {
            telefono = ExtraerTelefonoDesdeJid(jid);
        }

        return new(jid, nombre, telefono);
    }

    static string ExtraerTelefonoDesdeJid(string jid)
    {
        if (string.IsNullOrWhiteSpace(jid))
        {
            return string.Empty;
        }

        int separador = jid.IndexOf('@');
        string candidato = separador >= 0 ? jid.Substring(0, separador) : jid;

        return candidato.All(char.IsDigit) ? candidato : string.Empty;
    }

    List<string> EjecutarSqlite(string query)
    {
        string rutaDb = Path.Combine(ObtenerDirectorioStore(), "wacli.db");

        if (!File.Exists(rutaDb))
        {
            Console.WriteLine($"Aviso: no existe la base local de wacli: {rutaDb}");
            return new();
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "sqlite3",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(rutaDb);
        startInfo.ArgumentList.Add(query);

        using Process proceso = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar sqlite3.");

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error = proceso.StandardError.ReadToEnd().Trim();

        proceso.WaitForExit();

        if (proceso.ExitCode != 0)
        {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            throw new InvalidOperationException($"sqlite3 falló con código {proceso.ExitCode}: {detalle}");
        }

        return salida.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(linea => linea.Trim())
                     .Where(linea => !string.IsNullOrWhiteSpace(linea))
                     .ToList();
    }

    string ObtenerDirectorioStore()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (string.IsNullOrWhiteSpace(store))
        {
            return Path.Combine(home, ".wacli");
        }

        if (store == "~")
        {
            return home;
        }

        if (store.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(home, store.Substring(2));
        }

        return Environment.ExpandEnvironmentVariables(store);
    }

    static string EscaparSqlite(string valor)
    {
        return valor.Replace("'", "''");
    }

    GrupoWhatsApp? BuscarGrupoPorReferencia(string referencia, bool refrescar)
    {
        string textoBuscado = referencia.Trim();
        List<GrupoWhatsApp> gruposDisponibles = ListarGrupos(refrescar);

        List<GrupoWhatsApp> coincidenciasExactas = gruposDisponibles
            .Where(grupo =>
                string.Equals(grupo.Nombre, textoBuscado, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(grupo.Jid, textoBuscado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (coincidenciasExactas.Count == 1)
        {
            return coincidenciasExactas[0];
        }

        if (coincidenciasExactas.Count > 1)
        {
            string nombres = string.Join(", ", coincidenciasExactas.Select(grupo => $"{grupo.Nombre} ({grupo.Jid})"));
            throw new InvalidOperationException($"La referencia exacta '{textoBuscado}' coincide con varios grupos: {nombres}");
        }

        List<GrupoWhatsApp> coincidenciasParciales = gruposDisponibles
            .Where(grupo =>
                grupo.Nombre.Contains(textoBuscado, StringComparison.OrdinalIgnoreCase) ||
                grupo.Jid.Contains(textoBuscado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (coincidenciasParciales.Count == 1)
        {
            return coincidenciasParciales[0];
        }

        if (coincidenciasParciales.Count > 1)
        {
            string nombres = string.Join(", ", coincidenciasParciales.Select(grupo => $"{grupo.Nombre} ({grupo.Jid})"));
            throw new InvalidOperationException($"La búsqueda '{textoBuscado}' coincide con varios grupos: {nombres}");
        }

        return null;
    }

    void CargarGrupos(bool refrescar)
    {
        if (!gruposHabilitados)
        {
            grupos = new();
            return;
        }

        if (refrescar)
        {
            Ejecutar(false, new() { "groups", "refresh" });
        }

        string salida = EjecutarYObtenerSalida(false, new() { "groups", "list" });
        grupos = ParsearGrupos(salida);
    }

    static bool EsJidGrupo(string texto)
    {
        return texto.EndsWith("@g.us", StringComparison.OrdinalIgnoreCase);
    }

    static string ObtenerReferenciaGrupoPorComision(string comision)
    {
        string valor = comision.Trim();

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("La comisión no puede estar vacía.", nameof(comision));
        }

        if (EsJidGrupo(valor))
        {
            return valor;
        }

        if (valor.StartsWith("TUP26-P3-", StringComparison.OrdinalIgnoreCase))
        {
            return valor;
        }

        return $"TUP26-P3-{valor}";
    }

    static List<GrupoWhatsApp> ParsearGrupos(string salida)
    {
        List<GrupoWhatsApp> grupos = new();

        foreach (string linea in salida.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            string actual = linea.Trim();

            if (string.IsNullOrWhiteSpace(actual) || actual.StartsWith("NAME", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string[] columnas = Regex.Split(actual, @"\s{2,}");
            if (columnas.Length < 3)
            {
                continue;
            }

            string nombre = columnas[0].Trim();
            string jid = columnas[1].Trim();
            string creado = columnas[2].Trim();

            if (!jid.EndsWith("@g.us", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            DateTime? fechaCreacion = null;
            if (DateTime.TryParse(creado, out DateTime fecha))
            {
                fechaCreacion = fecha;
            }

            grupos.Add(new(nombre, jid, fechaCreacion));
        }

        return grupos;
    }

    void Ejecutar(bool json, List<string> argumentos)
    {
        string salida = EjecutarYObtenerSalida(json, argumentos);

        if (!string.IsNullOrWhiteSpace(salida))
        {
            Console.WriteLine(salida);
        }
    }

    string EjecutarYObtenerSalida(bool json, List<string> argumentos)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "wacli",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (json)
        {
            startInfo.ArgumentList.Add("--json");
        }

        if (!string.IsNullOrWhiteSpace(store))
        {
            startInfo.ArgumentList.Add("--store");
            startInfo.ArgumentList.Add(store);
        }

        startInfo.ArgumentList.Add("--timeout");
        startInfo.ArgumentList.Add(FormatearDuracionWacli(timeout));

        foreach (string argumento in argumentos)
        {
            startInfo.ArgumentList.Add(argumento);
        }

        using Process proceso = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar wacli.");

        proceso.StandardInput.Close();

        if (!proceso.WaitForExit((int)timeout.TotalMilliseconds))
        {
            try
            {
                proceso.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignoramos errores al intentar detener el proceso.
            }

            throw new TimeoutException($"wacli no terminó dentro de {timeout}.");
        }

        string salida = proceso.StandardOutput.ReadToEnd().Trim();
        string error = proceso.StandardError.ReadToEnd().Trim();

        if (proceso.ExitCode != 0)
        {
            string detalle = string.IsNullOrWhiteSpace(error) ? salida : error;
            throw new InvalidOperationException($"wacli falló con código {proceso.ExitCode}: {detalle}");
        }

        return salida;
    }

    static string FormatearDuracionWacli(TimeSpan duracion)
    {
        if (duracion <= TimeSpan.Zero)
        {
            return "1s";
        }

        if (duracion.TotalSeconds < 60 && duracion.TotalSeconds == Math.Floor(duracion.TotalSeconds))
        {
            return $"{(int)duracion.TotalSeconds}s";
        }

        if (duracion.TotalMinutes < 60 && duracion.TotalSeconds % 60 == 0)
        {
            return $"{(int)duracion.TotalMinutes}m";
        }

        int horas = (int)duracion.TotalHours;
        int minutos = duracion.Minutes;
        int segundos = duracion.Seconds;

        StringBuilder sb = new();

        if (horas > 0)
        {
            sb.Append($"{horas}h");
        }

        if (minutos > 0)
        {
            sb.Append($"{minutos}m");
        }

        if (segundos > 0 || sb.Length == 0)
        {
            sb.Append($"{segundos}s");
        }

        return sb.ToString();
    }

    static bool EsErrorAutenticacionWacli(Exception ex)
    {
        return ex.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("auth", StringComparison.OrdinalIgnoreCase);
    }
}
