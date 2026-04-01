//
// TUI: Interfaz de Usuario de Texto para una Agenda de Contactos
//
// Este programa permite al usuario agregar y ver contactos a través de una interfaz de texto en la consola. 
// Utiliza un menú simple para navegar entre las opciones y muestra la información de los contactos de
// la agenda.

int ElegirOpcion(){
    while(true){
        Console.WriteLine("\nElegir opción:\n");
        Console.WriteLine(" 1. Agregar contacto");
        Console.WriteLine(" 2. Ver contactos");
        Console.WriteLine(" 0. Salir");
        Console.Write("\nSeleccione una opción: ");
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);

        if(keyInfo.Key == ConsoleKey.Escape) return 0; 
        return int.TryParse(keyInfo.KeyChar.ToString(), out int opcion) ? opcion : -1; 
    }
}

string LeerEntrada(string mensaje, int longitud = 30){
    Console.Write($"{mensaje, 10} :");
    var lugar = Console.GetCursorPosition();
    while(true){
        LimpiarLinea(lugar, longitud);
        var entrada = Console.ReadLine();
        if(!string.IsNullOrEmpty(entrada)){
            return entrada;
        }
    }
}

int LeerNumero(string mensaje, int longitud = 10 ){
    Console.Write($"{mensaje, 10} :");
    var lugar = Console.GetCursorPosition();
    while(true){
        LimpiarLinea(lugar, longitud);
        var entrada = Console.ReadLine();
        if(!string.IsNullOrEmpty(entrada) && int.TryParse(entrada, out int resultado)){
            return resultado;
        }
    }
}

Contacto LeerContacto(){
    Console.Clear();
    Console.WriteLine("\n= Agregando Contacto =\n");
    string nombre   = LeerEntrada("Nombre", 30);
    string apellido = LeerEntrada("Apellido", 30);
    string telefono = LeerEntrada("Teléfono", 15);
    int edad        = LeerNumero( "Edad", 5);
    return new Contacto(NombrePropio(nombre), NombrePropio(apellido), telefono, edad);
}

void MostrarContacto(Contacto contacto){
    Console.WriteLine($" - {contacto.NombreCompleto, -30}  Tel: {contacto.Telefono, -15} Edad: {contacto.Edad, 3}");
}

void ListarContactos(List<Contacto> contactos){
    Console.Clear();
    Console.WriteLine("\n= Listando Contactos =\n");
    foreach(var contacto in contactos){
        MostrarContacto(contacto);
    }
}

// - Datos iniciales -

List<Contacto> contactos = new List<Contacto> {
    new Contacto("Juan",   "Pérez", "555-1234", 30),
    new Contacto("María",  "Gómez", "555-5678", 25),
    new Contacto("Carlos", "López", "555-9012", 40)
};

// - Bucle principal del programa -

while(true){
    Console.Clear();
    Console.WriteLine("=== Agenda de Contactos ===");
    int opcion = ElegirOpcion();
    switch(opcion){
        case 1:
            Contacto contacto = LeerContacto();
            contactos.Add(contacto);
            Console.WriteLine("\nContacto agregado exitosamente.");
            Pausar();
            break;
        case 2:            
            ListarContactos(contactos);
            Pausar();
            break;
        case 0:
            Console.Clear();
            Console.WriteLine("¡Hasta luego!");
            return;
        default:
            Console.WriteLine("Opción no válida. Intente nuevamente.");
            break;
    }
}   


// - Funciones auxiliares -

string NombrePropio(string nombre){
    if(string.IsNullOrEmpty(nombre)) return nombre;
    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower());
}

// Un poco de magia para limpiar la línea de entrada sin afectar el resto de la pantalla.
void LimpiarLinea((int Left, int Top) lugar, int longitud){
    Console.SetCursorPosition(lugar.Left, lugar.Top);
    Console.Write($"[ {new string(' ', longitud)} ]");
    Console.SetCursorPosition(lugar.Left + 1, lugar.Top);
}

void Pausar(){
    Console.Write("\nPresione una tecla para continuar...");
    Console.ReadKey();
}

record Contacto(string Nombre, string Apellido, string Telefono, int Edad){
    public string NombreCompleto => $"{Apellido}, {Nombre}";
};