using System.Runtime.CompilerServices;

namespace Tup26.AlumnosApp;

class Program {
    static void Main(string[] args) {
        Alumnos alumnos = AlumnosManager.Cargar(AppPaths.ArchivoAlumnos);
        
        var practicos = File.ReadAllLines("./tp1.csv")
            .Select(line => line.Trim().Split(',')[..2])
            .Skip(1)
            .Select(a => (legajo: int.Parse(a[0]), resuelto: int.Parse(a[1])>20))
            .ToList();

        foreach(var alumno in alumnos) {
            alumno.Practico(1, Estado.Vacio);
        }
        foreach(var (legajo, resuelto) in practicos) {
            Console.WriteLine($"Legajo: {legajo} - Resuelto: {resuelto}");
            var alumno = alumnos.BuscarPorLegajo(legajo);
            if (alumno != null) {
                alumno.Practico(1, resuelto ? Estado.Aprobado : Estado.Desaprobado);
                // alumno.Practico1Resuelto = resuelto;
            }
        }

        alumnos = alumnos.ConPractico(1, Estado.Desaprobado);
        AlumnosManager.Guardar(alumnos, "alumnos-p1-sin-presentar.md");
        AlumnosManager.CrearCarpetas(alumnos);
        
        // AlumnosManager.Listar(alumnos.ConFotos(false), "Alumnos sin foto");
        // AlumnosManager.Listar(alumnos.ConTelefono(false), "Alumnos sin telefono");
        // AlumnosManager.Listar(alumnos.ConGithub(false).ConTelefono(true), "Alumnos sin GitHub");
        // AlumnosManager.ActualizarDesdePerfiles(alumnos, "../material-docente/perfil/perfiles");
        // AlumnosManager.Guardar(alumnos, AppPaths.ArchivoAlumnos);
        // AlumnosManager.GuardarJSON(alumnos, "alumnos.json");
        // AlumnosManager.GuardarVCard(alumnos, AppPaths.ArchivoVcf);
        // AlumnosManager.CrearCarpetas(alumnos);
        // AlumnosManager.CopiarFotoPerfil(alumnos, rutaPerfiles);

        // var sinFoto = alumnos.SinFotos();
        // AlumnosManager.Guardar(sinFoto, "alumnos-sin-foto.md");

        // var sinTelefono = alumnos.SinTelefono();
        // AlumnosManager.Guardar(sinTelefono, "alumnos-sin-telefono.md");

        // var sinGitHub = alumnos.FiltrarSinGithub();
        // AlumnosManager.Guardar(sinGitHub, "alumnos-sin-github.md");

        // Console.WriteLine("Enviando mensaje al grupo de WhatsApp...");

        // Alumnos alumnosConTelefonoComision7 = alumnos.EnComision("C7").SinTelefono();
        // AlumnosManager.Listar(alumnosConTelefonoComision7);

        // Alumnos alumnosConTelefonoComision9 = alumnos.EnComision("C9").SinTelefono();

        // AlumnosManager.Listar(alumnos.ParaAgregar());

        // AlumnosManager.GuardarVCard(alumnos.ParaAgregar().EnComision("C7"), "alumnos-agregar-c7.vcf");
        // AlumnosManager.GuardarVCard(alumnos.ParaAgregar().EnComision("C9"), "alumnos-agregar-c9.vcf");
        // AlumnosManager.CopiarEnunciadoPracticos(alumnos, "tp1");

        // GitHub gh = new GitHub();
        // if (gh.PRSinLegajo() == 0) { 
        //     gh.NormalizarTitulos(alumnos, simular: false); 
        // } 
        // foreach(var pr in gh.PullRequests()) {
        //     var commits   = gh.Commits(pr.Numero);
        //     var detallePr = gh.ObtenerEstado(pr.Numero);
        //     var estado    = detallePr.Estado == "open" ? "abierto" : detallePr.Estado == "closed" ? "cerrado" : "sin dato";
        //     var mergeable = detallePr.EsMergeable ? "mergeable" : "con conflictos";

        //     Console.WriteLine($"- #{pr.Numero:D3} | {estado,-7} | {mergeable,-13} | {(commits.Count > 3 ? "🟢" : "🔴")} {commits.Count,2} | {pr.Titulo}");
        //     foreach(var commit in commits) {
        //         Console.WriteLine($" > {commit.FechaHora:dd-MM HH:mm} - {commit.Titulo} ()");
        //     }
        // }
        // List<string> colaboradores = gh.ListarColaboradores();
        // List<string> invitaciones  = gh.ListarInvitacionesPendientes();

        // Console.WriteLine($"Colaboradores: {string.Join(" ", colaboradores)}");
        // Console.WriteLine($"Pendientes: {string.Join(" ", invitaciones)}");
        // Console.WriteLine();

        // AlumnosManager.Guardar(alumnos, AppPaths.ArchivoAlumnos);

        // MensajesService.MensajeSinGithub();
        // MensajesService.MensajeGithubErroneo();

        // var wapp = new WAppService();
        // // wapp.Enviar("3815343456", "Hola desde la aplicación de alumnos!");
        // Console.WriteLine("\n= Participantes del grupo C7 =");
        // foreach(var c in wapp.Participantes("C7")) {
        //     Console.WriteLine($"- {c.Name,-30} | {c.PhoneNumber} | {c.Jid}");
        // }
        // Console.WriteLine("\n= Participantes del grupo C9 =");
        // foreach(var c in wapp.Participantes("C9")) {
        //     Console.WriteLine($"- {c.Name,-30} | {c.PhoneNumber} | {c.Jid}");
        // }
        // Console.WriteLine("\n= Grupos ==");
        // foreach(var g in wapp.Grupos()) {
        //     Console.WriteLine($"- {g.Group,-30} | {g.Jid}");
        // }

    }
}
