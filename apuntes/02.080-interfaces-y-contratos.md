# Interfaces y contratos en C#

## Introducción

Una **interfaz** define un **contrato**: dice qué puede hacer un tipo, sin decir cómo lo hace. Cualquier clase que implemente ese contrato garantiza que puede hacer esas cosas.

Si una clase abstracta expresa "es un tipo de…", una interfaz expresa "puede hacer…" o "se comporta como…".

```
IVehiculo        → puede arrancar, frenar, girar
ISerializable    → puede serializar su estado
IComparable<T>   → puede compararse con otro T
IEnumerable<T>   → puede ser recorrido con foreach
```

Ese modelo —múltiples capacidades ortogonales— es lo que hace a las interfaces tan potentes.

---

## 1. Sintaxis básica

```csharp
// Declaración — todo es público implícitamente
public interface ILogger
{
    void Log(string mensaje);
    void LogError(string mensaje, Exception? ex = null);
    bool EstaActivo { get; }
}

// Implementación
public class ConsoleLogger : ILogger
{
    public bool EstaActivo => true;

    public void Log(string mensaje)
        => Console.WriteLine($"[INFO]  {mensaje}");

    public void LogError(string mensaje, Exception? ex = null)
        => Console.WriteLine($"[ERROR] {mensaje}{(ex is null ? "" : $": {ex.Message}")}");
}

public class NullLogger : ILogger
{
    public bool EstaActivo => false;
    public void Log(string mensaje)      { /* descarta */ }
    public void LogError(string mensaje, Exception? ex = null) { }
}

// Uso polimórfico — el código trabaja con la interfaz, no con la clase concreta
ILogger log = new ConsoleLogger();
log.Log("Aplicación iniciada");
```

### 1.1 Qué puede tener una interfaz

| Miembro | Permitido |
|---------|:---------:|
| Métodos (sin implementación) | Sí |
| Propiedades (sin implementación) | Sí |
| Eventos | Sí |
| Métodos con implementación por defecto | Sí (C# 8+) |
| Propiedades con implementación por defecto | Sí (C# 8+) |
| Campos / variables de instancia | **No** |
| Constructores | **No** |

---

## 2. Implementación múltiple

La ventaja clave sobre las clases abstractas: un tipo puede implementar **múltiples interfaces**:

```csharp
public interface ILegible
{
    string Leer();
}

public interface IEscribible
{
    void Escribir(string contenido);
}

public interface IFlusheable
{
    void Flush();
}

// Una clase puede implementar todas
public class Buffer : ILegible, IEscribible, IFlusheable
{
    private readonly StringBuilder _sb = new();

    public string Leer()                    => _sb.ToString();
    public void   Escribir(string contenido) => _sb.Append(contenido);
    public void   Flush()                   => _sb.Clear();
}
```

---

## 3. Interfaces con métodos por defecto (C# 8+)

Una interfaz puede proveer una implementación predeterminada para sus miembros. Las clases que implementan la interfaz pueden usarla o sobreescribirla:

```csharp
public interface IFormateador
{
    // Miembro abstracto — obligatorio implementar
    string Formatear(string texto);

    // Miembro con implementación por defecto — opcional sobreescribir
    string FormatearLista(IEnumerable<string> items)
        => string.Join(", ", items.Select(Formatear));

    string FormatearConEncabezado(string titulo, string texto)
        => $"=== {titulo} ===\n{Formatear(texto)}";
}

// Usa ambos métodos por defecto
public class FormateadorMayusculas : IFormateador
{
    public string Formatear(string texto) => texto.ToUpper();
    // FormatearLista y FormatearConEncabezado usan la implementación por defecto
}

// Sobreescribe uno de los métodos por defecto
public class FormateadorHTML : IFormateador
{
    public string Formatear(string texto) => $"<span>{texto}</span>";

    // Sobreescribe la implementación por defecto
    public string FormatearLista(IEnumerable<string> items)
        => "<ul>" + string.Join("", items.Select(i => $"<li>{Formatear(i)}</li>")) + "</ul>";
}

// Uso
IFormateador f1 = new FormateadorMayusculas();
Console.WriteLine(f1.FormatearLista(["hola", "mundo"]));   // HOLA, MUNDO

IFormateador f2 = new FormateadorHTML();
Console.WriteLine(f2.FormatearLista(["hola", "mundo"]));   // <ul><li><span>hola</span></li>...
```

> Los métodos por defecto se acceden a través de la **interfaz**, no de la clase concreta. Si `f2` fuera `FormateadorHTML` (no `IFormateador`), no podría llamar `FormatearConEncabezado` a menos que la clase lo declare.

---

## 4. Interfaces vs. Clases abstractas

Son herramientas distintas para problemas distintos:

| Característica | Interfaz | Clase abstracta |
|---------------|----------|----------------|
| Herencia múltiple | Sí — un tipo implementa N interfaces | No — un tipo hereda de una sola clase |
| Estado (campos de instancia) | No | Sí |
| Constructores | No | Sí |
| Implementación por defecto | Sí (C# 8+, limitada) | Sí (completa) |
| Modificadores de acceso en miembros | Solo `public` | Todos |
| Expresa "puede hacer…" | Sí | Indirecto |
| Expresa "es un tipo de…" | No | Sí |

### Regla práctica

```
¿Los tipos comparten estado + lógica base?
    └─► Clase abstracta

¿Los tipos comparten sólo un contrato de comportamiento?
    └─► Interfaz

¿Necesitás que un tipo cumpla múltiples contratos distintos?
    └─► Múltiples interfaces

¿Querés una implementación base con algunos métodos obligatorios?
    └─► Clase abstracta (o abstracta + interfaz combinados)
```

### Ejemplo comparativo

```csharp
// Clase abstracta — modela jerarquía de tipos con estado compartido
public abstract class Animal(string nombre)
{
    public string Nombre => nombre;
    protected int Energia { get; set; } = 100;

    public abstract void HacerSonido();
    public void Comer() => Energia += 10;   // lógica compartida con estado
}

// Interfaces — modelan capacidades ortogonales
public interface IEntrenable    { void Entrenar(); }
public interface IVacunable     { void Vacunar(); }
public interface IDomesticable  { string Duenio { get; } }

// Un perro ES un Animal Y PUEDE SER entrenado, vacunado y domesticado
public class Perro(string nombre, string duenio)
    : Animal(nombre), IEntrenable, IVacunable, IDomesticable
{
    public string Duenio => duenio;

    public override void HacerSonido() => Console.WriteLine("Guau!");
    public void Entrenar() => Console.WriteLine($"{Nombre} aprende un truco");
    public void Vacunar()  => Console.WriteLine($"{Nombre} fue vacunado");
}
```

---

## 5. Tipos genéricos e interfaces

Los genéricos y las interfaces se combinan para crear contratos flexibles y seguros en tipos:

### 5.1 Interfaz genérica

```csharp
// Interfaz genérica — el contrato es sobre un tipo que se especificará después
public interface IRepositorio<T>
{
    T?           ObtenerPorId(int id);
    IList<T>     ObtenerTodos();
    void         Agregar(T entidad);
    void         Actualizar(T entidad);
    void         Eliminar(int id);
}

// Implementación para una entidad concreta
public record Producto(int Id, string Nombre, decimal Precio);

public class ProductoRepositorio : IRepositorio<Producto>
{
    private readonly List<Producto> _datos = [];

    public Producto? ObtenerPorId(int id)
        => _datos.FirstOrDefault(p => p.Id == id);

    public IList<Producto> ObtenerTodos() => _datos.AsReadOnly();

    public void Agregar(Producto p)     => _datos.Add(p);
    public void Actualizar(Producto p)  => _datos[_datos.FindIndex(x => x.Id == p.Id)] = p;
    public void Eliminar(int id)        => _datos.RemoveAll(p => p.Id == id);
}
```

### 5.2 Restricciones sobre parámetros de tipo — `where`

Se puede exigir que el parámetro genérico cumpla ciertos contratos:

```csharp
// T debe implementar IComparable<T>
public static T Maximo<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) >= 0 ? a : b;

// T debe ser una clase (tipo referencia) y tener constructor sin parámetros
public static T Crear<T>() where T : class, new()
    => new T();

// T debe ser struct (tipo valor)
public static T Default<T>() where T : struct
    => default;

// T debe implementar múltiples interfaces
public interface IPersistible   { int    Id    { get; } }
public interface IValidable     { bool   EsValido(); }

public class Servicio<T> where T : IPersistible, IValidable
{
    public void Guardar(T entidad)
    {
        if (!entidad.EsValido())
            throw new InvalidOperationException($"La entidad {entidad.Id} no es válida.");
        Console.WriteLine($"Guardando entidad {entidad.Id}");
    }
}
```

### 5.3 Covarianza e Invarianza — `out` e `in`

Definen si un genérico puede "subir" o "bajar" en la jerarquía de tipos:

```csharp
// Covarianza (out) — IEnumerable<Gato> puede tratarse como IEnumerable<Animal>
// El tipo sólo SALE (se produce)
public interface IProductor<out T>
{
    T Producir();
}

// Contravarianza (in) — IComparador<Animal> puede tratarse como IComparador<Gato>
// El tipo sólo ENTRA (se consume)
public interface IConsumidor<in T>
{
    void Consumir(T item);
}

// Ejemplo real — IEnumerable<T> usa out
IEnumerable<string>  strings  = ["hola", "mundo"];
IEnumerable<object>  objects  = strings;   // ✓ covarianza: string es object

// IComparer<T> usa in (contravarianza)
IComparer<object>  cmpObj = Comparer<object>.Default;
IComparer<string>  cmpStr = cmpObj;        // ✓ contravarianza
```

---

## 6. Interfaces de colecciones de .NET

.NET define una jerarquía de interfaces para colecciones. Conocerlas permite escribir código más genérico y reutilizable:

```
IEnumerable<T>
    └─► ICollection<T>
            ├─► IList<T>
            └─► ISet<T>
    └─► IReadOnlyCollection<T>
            ├─► IReadOnlyList<T>
            └─► IReadOnlyDictionary<TK,TV>
IDictionary<TKey,TValue>
```

### 6.1 `IEnumerable<T>` — la interfaz base de todo

```csharp
// Acepta cualquier colección — array, lista, set, query LINQ...
public static void Imprimir<T>(IEnumerable<T> items)
{
    foreach (var item in items)
        Console.WriteLine(item);
}

// Cualquiera de estas pasa sin problema
Imprimir(new int[]          { 1, 2, 3 });
Imprimir(new List<string>   { "a", "b" });
Imprimir(new HashSet<double>{ 1.1, 2.2 });
Imprimir(Enumerable.Range(1, 10));   // secuencia lazy
```

Solo garantiza iteración. No permite acceso por índice ni modificación.

### 6.2 `ICollection<T>` — colección modificable

Agrega `Count`, `Add`, `Remove`, `Contains`, `Clear`:

```csharp
public static void AgregarSiNoExiste<T>(ICollection<T> coleccion, T item)
{
    if (!coleccion.Contains(item))
        coleccion.Add(item);
}

// Funciona con List<T>, HashSet<T>, etc.
var lista = new List<int> { 1, 2, 3 };
AgregarSiNoExiste(lista, 4);
AgregarSiNoExiste(lista, 2);   // no agrega — ya existe
```

### 6.3 `IList<T>` — acceso por índice

Agrega `this[int]`, `IndexOf`, `Insert`, `RemoveAt`:

```csharp
public static void Intercambiar<T>(IList<T> lista, int i, int j)
    => (lista[i], lista[j]) = (lista[j], lista[i]);

var arr   = new int[]         { 1, 2, 3, 4 };
var lista = new List<string>  { "a", "b", "c" };

Intercambiar(arr,   0, 3);   // [4, 2, 3, 1]
Intercambiar(lista, 0, 2);   // ["c", "b", "a"]
```

### 6.4 `IReadOnlyCollection<T>` y `IReadOnlyList<T>`

Para exponer colecciones sin permitir modificación externa:

```csharp
public class Inventario
{
    private readonly List<Producto> _productos = [];

    // Expone la lista como sólo lectura — el cliente no puede Add/Remove
    public IReadOnlyList<Producto>       Productos   => _productos.AsReadOnly();
    public IReadOnlyCollection<Producto> Disponibles => _productos.Where(p => p.Stock > 0).ToList();

    public void Agregar(Producto p) => _productos.Add(p);
}
```

### 6.5 `IDictionary<TKey, TValue>` y `IReadOnlyDictionary<TKey, TValue>`

```csharp
public class Cache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _store = [];

    public void Set(TKey key, TValue value)   => _store[key] = value;
    public bool TryGet(TKey key, out TValue? value) => _store.TryGetValue(key, out value);

    // Exponé como read-only para el exterior
    public IReadOnlyDictionary<TKey, TValue> Snapshot
        => _store.AsReadOnly();
}
```

### 6.6 `ISet<T>` — operaciones de conjuntos

```csharp
public static ISet<T> Diferencia<T>(ISet<T> a, ISet<T> b)
{
    var resultado = new HashSet<T>(a);
    resultado.ExceptWith(b);
    return resultado;
}

var rolesActuales  = new HashSet<string> { "admin", "editor", "viewer" };
var rolesNuevos    = new HashSet<string> { "editor", "viewer", "auditor" };

var aAgregar  = Diferencia(rolesNuevos,    rolesActuales);  // { "auditor" }
var aEliminar = Diferencia(rolesActuales,  rolesNuevos);    // { "admin" }
```

---

## 7. Enumeración — `IEnumerable<T>` e `IEnumerator<T>`

`foreach` funciona con cualquier tipo que implemente `IEnumerable<T>`. Entender cómo funciona por dentro permite crear secuencias propias.

### 7.1 Cómo funciona `foreach` por dentro

```csharp
// Lo que escribe el programador
foreach (var item in coleccion)
    Console.WriteLine(item);

// Lo que genera el compilador (aproximado)
var enumerador = coleccion.GetEnumerator();
try
{
    while (enumerador.MoveNext())
    {
        var item = enumerador.Current;
        Console.WriteLine(item);
    }
}
finally { enumerador.Dispose(); }
```

`IEnumerator<T>` tiene tres miembros: `MoveNext()`, `Current` y `Reset()`.

### 7.2 Implementar `IEnumerable<T>` manualmente

```csharp
public class RangoEnteros : IEnumerable<int>
{
    private readonly int _inicio;
    private readonly int _fin;
    private readonly int _paso;

    public RangoEnteros(int inicio, int fin, int paso = 1)
    {
        _inicio = inicio;
        _fin    = fin;
        _paso   = paso;
    }

    public IEnumerator<int> GetEnumerator()   => new RangoEnumerador(_inicio, _fin, _paso);
    IEnumerator IEnumerable.GetEnumerator()   => GetEnumerator();   // interfaz no genérica

    // Enumerador interno
    private class RangoEnumerador(int inicio, int fin, int paso) : IEnumerator<int>
    {
        private int _actual = inicio - paso;

        public int  Current => _actual;
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _actual += paso;
            return _actual <= fin;
        }

        public void Reset()   => _actual = inicio - paso;
        public void Dispose() { }
    }
}

// Uso
var rango = new RangoEnteros(1, 10, 2);
foreach (var n in rango)
    Console.Write($"{n} ");   // 1 3 5 7 9

// También funciona con LINQ porque implementa IEnumerable<T>
var suma = rango.Sum();   // 25
```

### 7.3 `yield return` — la forma idiomática

`yield` convierte un método en un generador: el compilador genera toda la infraestructura de `IEnumerator<T>` automáticamente:

```csharp
public class Calendario
{
    // Genera todos los días hábiles entre dos fechas
    public static IEnumerable<DateOnly> DiasHabiles(DateOnly desde, DateOnly hasta)
    {
        for (var d = desde; d <= hasta; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                yield return d;
        }
    }

    // Genera la secuencia de Fibonacci indefinidamente (lazy — sólo calcula lo pedido)
    public static IEnumerable<long> Fibonacci()
    {
        long a = 0, b = 1;
        while (true)
        {
            yield return a;
            (a, b) = (b, a + b);
        }
    }
}

// Uso
var hoy    = DateOnly.FromDateTime(DateTime.Today);
var proxima = hoy.AddDays(14);

foreach (var dia in Calendario.DiasHabiles(hoy, proxima))
    Console.WriteLine(dia.ToString("dd/MM/yyyy ddd"));

// Tomar los primeros 10 Fibonacci
var fibs = Calendario.Fibonacci().Take(10).ToList();
// [0, 1, 1, 2, 3, 5, 8, 13, 21, 34]
```

> La evaluación con `yield` es **lazy**: los elementos se producen uno a uno a medida que se consumen. Si hacés `Take(5)`, sólo se calculan 5 elementos, nunca el resto.

### 7.4 `yield break` — terminar la secuencia condicionalmente

```csharp
public static IEnumerable<string> LeerLineasHasta(string ruta, string marcadorFin)
{
    foreach (var linea in File.ReadLines(ruta))
    {
        if (linea == marcadorFin)
            yield break;   // termina la secuencia
        yield return linea;
    }
}
```

### 7.5 `IAsyncEnumerable<T>` — enumeración asíncrona (C# 8+)

Para fuentes de datos lentas (base de datos, red, archivos grandes):

```csharp
public static async IAsyncEnumerable<Producto> ObtenerProductosAsync(int paginas)
{
    for (int pagina = 1; pagina <= paginas; pagina++)
    {
        var lote = await ObtenerPaginaAsync(pagina);
        foreach (var producto in lote)
            yield return producto;
    }
}

// Consumo con await foreach
await foreach (var producto in ObtenerProductosAsync(5))
    Console.WriteLine(producto.Nombre);
```

---

## 8. Interfaces importantes de .NET

### 8.1 `IComparable<T>` — ordenamiento natural

```csharp
public class Prioridad(int valor) : IComparable<Prioridad>
{
    public int Valor => valor;

    public int CompareTo(Prioridad? otro)
    {
        if (otro is null) return 1;
        return valor.CompareTo(otro.Valor);
    }
}

// Ahora List<Prioridad>.Sort() y LINQ OrderBy funcionan
var prioridades = new List<Prioridad> { new(3), new(1), new(2) };
prioridades.Sort();
var ordenada = prioridades.OrderBy(p => p).ToList();
```

### 8.2 `IComparer<T>` — comparador externo

Cuando necesitás múltiples criterios de ordenamiento sin modificar la clase:

```csharp
public record Alumno(string Nombre, double Promedio, int Edad);

public class OrdenarPorPromedio : IComparer<Alumno>
{
    public int Compare(Alumno? x, Alumno? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        return y.Promedio.CompareTo(x.Promedio);   // descendente
    }
}

// Uso
var alumnos = new List<Alumno>
{
    new("Ana",    9.5, 20),
    new("Carlos", 7.0, 22),
    new("Laura",  8.2, 21),
};

alumnos.Sort(new OrdenarPorPromedio());

// Más compacto con Comparer<T>.Create
var ordenador = Comparer<Alumno>.Create((a, b) => a.Nombre.CompareTo(b.Nombre));
alumnos.Sort(ordenador);
```

### 8.3 `IEquatable<T>` — igualdad por valor

```csharp
public class Email : IEquatable<Email>
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (!valor.Contains('@'))
            throw new ArgumentException("Email inválido.");
        Valor = valor.ToLower().Trim();
    }

    public bool   Equals(Email? otro)    => otro is not null && Valor == otro.Valor;
    public override bool Equals(object? obj) => Equals(obj as Email);
    public override int  GetHashCode()   => Valor.GetHashCode();

    public static bool operator ==(Email? a, Email? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(Email? a, Email? b) => !(a == b);

    public override string ToString() => Valor;
}

var e1 = new Email("ANA@Ejemplo.COM");
var e2 = new Email("ana@ejemplo.com");
Console.WriteLine(e1 == e2);   // true — normalización en el constructor
```

### 8.4 `IDisposable` — liberación de recursos

Para tipos que manejan recursos externos (archivos, conexiones, handles del SO):

```csharp
public class ConexionDB : IDisposable
{
    private bool _disposed = false;
    private readonly string _connectionString;

    public ConexionDB(string connectionString)
    {
        _connectionString = connectionString;
        Abrir();
    }

    private void Abrir()  => Console.WriteLine("Conexión abierta");
    private void Cerrar() => Console.WriteLine("Conexión cerrada");

    public void Dispose()
    {
        if (_disposed) return;
        Cerrar();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

// using garantiza que Dispose() se llame aunque haya excepciones
using var db = new ConexionDB("Server=localhost;...");
// ... trabajar con db ...
// al salir del bloque, Dispose() se llama automáticamente
```

---

## 9. Diseño con interfaces — ejemplo integrador

Un sistema de notificaciones con múltiples canales y comportamientos:

```csharp
// Contratos
public interface INotificador
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpo);
    string Canal { get; }
}

public interface INotificadorConPlantilla : INotificador
{
    Task EnviarConPlantillaAsync(string destinatario, string plantillaId, object datos);

    // Método por defecto — implementación base reutilizable
    string ResolverPlantilla(string plantillaId, object datos)
        => $"[{plantillaId}] {System.Text.Json.JsonSerializer.Serialize(datos)}";
}

public interface IReintentable
{
    int MaxReintentos { get; }
    TimeSpan EsperaEntreReintentos { get; }
}

// Implementaciones
public class NotificadorEmail : INotificadorConPlantilla, IReintentable
{
    public string Canal           => "email";
    public int    MaxReintentos   => 3;
    public TimeSpan EsperaEntreReintentos => TimeSpan.FromSeconds(5);

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        await Task.Delay(10);   // simula I/O
        Console.WriteLine($"[Email] → {destinatario} | {asunto}");
    }

    public async Task EnviarConPlantillaAsync(string destinatario, string plantillaId, object datos)
    {
        var cuerpo = ResolverPlantilla(plantillaId, datos);   // usa el default de la interfaz
        await EnviarAsync(destinatario, $"[{plantillaId}]", cuerpo);
    }
}

public class NotificadorSMS : INotificador, IReintentable
{
    public string Canal           => "sms";
    public int    MaxReintentos   => 1;
    public TimeSpan EsperaEntreReintentos => TimeSpan.FromSeconds(30);

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        await Task.Delay(5);
        Console.WriteLine($"[SMS] → {destinatario}: {cuerpo[..Math.Min(160, cuerpo.Length)]}");
    }
}

// Servicio genérico que trabaja con la interfaz
public class ServicioNotificacion(IEnumerable<INotificador> notificadores)
{
    public async Task NotificarTodosAsync(string destinatario, string asunto, string cuerpo)
    {
        foreach (var n in notificadores)
            await n.EnviarAsync(destinatario, asunto, cuerpo);
    }

    public async Task NotificarPorCanalAsync(string canal, string dest, string asunto, string cuerpo)
    {
        var notificador = notificadores.FirstOrDefault(n => n.Canal == canal)
            ?? throw new InvalidOperationException($"Canal '{canal}' no disponible.");
        await notificador.EnviarAsync(dest, asunto, cuerpo);
    }
}

// Configuración y uso
var servicio = new ServicioNotificacion([new NotificadorEmail(), new NotificadorSMS()]);
await servicio.NotificarTodosAsync("ana@ejemplo.com", "Bienvenida", "Hola Ana!");
```

---

## Resumen

```
¿Quiero definir un contrato de comportamiento?
    └─► interface

¿Un tipo necesita cumplir múltiples contratos?
    └─► Implementar múltiples interfaces

¿Quiero compartir estado e implementación base?
    └─► Clase abstracta (no interfaz)

¿Quiero dar una implementación por defecto opcional?
    └─► Método por defecto en la interfaz (C# 8+)

¿Quiero que la interfaz trabaje con cualquier tipo?
    └─► Interfaz genérica IRepositorio<T>, IServicio<T>

¿Quiero restringir qué tipos acepta el genérico?
    └─► where T : IAlgunaInterfaz

¿Quiero que un tipo sea recorrible con foreach?
    └─► Implementar IEnumerable<T>

¿Quiero una secuencia lazy sin boilerplate?
    └─► Método con yield return

¿Quiero una secuencia lazy y asíncrona?
    └─► IAsyncEnumerable<T> con await foreach
```
