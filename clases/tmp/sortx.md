# Tutorial: crear una app CLI en .NET para ordenar archivos CSV

Este tutorial explica, paso a paso, cómo crear una aplicación de consola en .NET que reciba un archivo CSV y lo ordene según uno o más campos.

La idea es mantenernos en el camino feliz:

- vamos a usar una app de consola simple
- vamos a parsear los argumentos manualmente
- vamos a asumir un CSV sencillo, separado por comas
- no vamos a cubrir todos los casos raros del formato CSV

El objetivo didáctico es entender cómo se arma una CLI en .NET de punta a punta.

## Qué vamos a construir

Vamos a crear una app llamada `CsvSorter` que pueda ejecutarse así:

```bash
dotnet run -- data.csv --sort fecha:date:asc --sort total:decimal:desc
```

Ese comando significa:

- tomar el archivo `data.csv`
- ordenar primero por `fecha` como `date` en orden ascendente
- si hay empate, ordenar por `total` como `decimal` en orden descendente

La salida se imprimirá por consola.

## Requisitos

Necesitás tener instalado el SDK de .NET.

Podés verificarlo con:

```bash
dotnet --version
```

Si ese comando devuelve una versión, ya estás listo.

## Paso 1: crear el proyecto

Desde una terminal:

```bash
dotnet new console -n CsvSorter
cd CsvSorter
```

### Qué hizo .NET acá

- `dotnet new console` creó una app de consola
- `-n CsvSorter` le dio nombre al proyecto
- se creó una carpeta con un archivo `CsvSorter.csproj`
- también se creó un `Program.cs` inicial

La estructura mínima queda parecida a esta:

```text
CsvSorter/
├── CsvSorter.csproj
└── Program.cs
```

## Paso 2: entender el punto de entrada

El `Program.cs` inicial suele verse así:

```csharp
Console.WriteLine("Hello, World!");
```

En una CLI, ese archivo es el punto de entrada. Ahí vamos a:

1. leer argumentos
2. validar lo necesario
3. cargar el CSV
4. ordenar filas
5. imprimir el resultado

## Paso 3: definir el contrato de nuestra CLI

Antes de escribir código, conviene decidir cómo se va a usar.

Vamos a usar esta forma:

```bash
dotnet run -- <archivo.csv> --sort <campo:tipo:orden> --sort <campo:tipo:orden>
```

### Ejemplos

Ordenar por una sola columna:

```bash
dotnet run -- data.csv --sort apellido:string:asc
```

Ordenar por dos columnas:

```bash
dotnet run -- data.csv --sort fecha:date:asc --sort monto:decimal:desc
```

### Significado de cada parte

Cada regla de orden tiene este formato:

```text
campo:tipo:orden
```

Donde:

- `campo`: nombre exacto de la columna en el encabezado del CSV
- `tipo`: cómo interpretar el valor
- `orden`: `asc` o `desc`

Para mantenerlo simple, soportaremos estos tipos:

- `string`
- `int`
- `decimal`
- `date`

## Paso 4: pegar el código completo

Reemplazá el contenido de `Program.cs` por esto:

```csharp
using System.Globalization;

if (args.Length < 3)
{
    PrintUsage();
    return;
}

var inputPath = args[0];
var sortSpecs = ParseSortSpecs(args.Skip(1).ToArray());

if (!File.Exists(inputPath))
{
    Console.WriteLine($"No existe el archivo: {inputPath}");
    return;
}

if (sortSpecs.Count == 0)
{
    Console.WriteLine("Debes indicar al menos una regla --sort.");
    return;
}

var lines = File.ReadAllLines(inputPath);

if (lines.Length < 2)
{
    Console.WriteLine("El CSV debe tener encabezado y al menos una fila.");
    return;
}

var headers = lines[0].Split(',');
var rows = lines
    .Skip(1)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => line.Split(','))
    .ToList();

var headerIndexes = headers
    .Select((name, index) => new { name, index })
    .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

foreach (var spec in sortSpecs)
{
    if (!headerIndexes.ContainsKey(spec.Field))
    {
        Console.WriteLine($"La columna '{spec.Field}' no existe en el CSV.");
        return;
    }
}

IOrderedEnumerable<string[]>? orderedRows = null;

for (var i = 0; i < sortSpecs.Count; i++)
{
    var spec = sortSpecs[i];
    var columnIndex = headerIndexes[spec.Field];

    Func<string[], IComparable> keySelector = row =>
    {
        var rawValue = columnIndex < row.Length ? row[columnIndex] : string.Empty;
        return ParseValue(rawValue, spec.Type);
    };

    if (i == 0)
    {
        orderedRows = spec.Descending
            ? rows.OrderByDescending(keySelector)
            : rows.OrderBy(keySelector);
    }
    else
    {
        orderedRows = spec.Descending
            ? orderedRows!.ThenByDescending(keySelector)
            : orderedRows!.ThenBy(keySelector);
    }
}

Console.WriteLine(string.Join(',', headers));

foreach (var row in orderedRows!)
{
    Console.WriteLine(string.Join(',', row));
}

static List<SortSpec> ParseSortSpecs(string[] cliArgs)
{
    var specs = new List<SortSpec>();

    for (var i = 0; i < cliArgs.Length; i++)
    {
        if (!string.Equals(cliArgs[i], "--sort", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (i + 1 >= cliArgs.Length)
        {
            Console.WriteLine("Falta el valor después de --sort.");
            return new List<SortSpec>();
        }

        var parts = cliArgs[i + 1].Split(':', StringSplitOptions.TrimEntries);

        if (parts.Length != 3)
        {
            Console.WriteLine($"Regla inválida: {cliArgs[i + 1]}");
            return new List<SortSpec>();
        }

        var field = parts[0];
        var type = parts[1].ToLowerInvariant();
        var order = parts[2].ToLowerInvariant();

        if (order is not ("asc" or "desc"))
        {
            Console.WriteLine($"Orden inválido en regla: {cliArgs[i + 1]}");
            return new List<SortSpec>();
        }

        specs.Add(new SortSpec(
            field,
            type,
            order == "desc"
        ));

        i++;
    }

    return specs;
}

static IComparable ParseValue(string rawValue, string type) =>
    type switch
    {
        "string" => rawValue,
        "int" => int.Parse(rawValue, CultureInfo.InvariantCulture),
        "decimal" => decimal.Parse(rawValue, CultureInfo.InvariantCulture),
        "date" => DateTime.Parse(rawValue, CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Tipo no soportado: {type}")
    };

static void PrintUsage()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  dotnet run -- <archivo.csv> --sort <campo:tipo:orden> [--sort <campo:tipo:orden>]");
    Console.WriteLine();
    Console.WriteLine("Tipos soportados: string, int, decimal, date");
    Console.WriteLine("Orden soportado: asc, desc");
}

record SortSpec(string Field, string Type, bool Descending);
```

## Paso 5: entender el código por partes

### 1. Validación inicial de argumentos

```csharp
if (args.Length < 3)
{
    PrintUsage();
    return;
}
```

Acá verificamos que el usuario haya pasado algo parecido a:

- la ruta del archivo
- al menos un `--sort`
- el valor de ese `--sort`

Si no, mostramos ayuda y terminamos.

### 2. Archivo de entrada

```csharp
var inputPath = args[0];
```

Tomamos el primer argumento como archivo CSV de entrada.

En esta versión mínima no usamos una opción como `--input`, porque para aprender conviene mantener una convención simple.

### 3. Reglas de orden

```csharp
var sortSpecs = ParseSortSpecs(args.Skip(1).ToArray());
```

Todo lo que viene después del nombre del archivo se procesa como configuración adicional.

La función `ParseSortSpecs` busca pares como:

```text
--sort fecha:date:asc
```

Y los convierte en objetos `SortSpec`.

### 4. Lectura del archivo

```csharp
var lines = File.ReadAllLines(inputPath);
```

Leemos todo el archivo de una vez.

Esto está perfecto para un ejemplo didáctico y archivos chicos o medianos. Si tuviéramos archivos enormes, más adelante convendría pensar otra estrategia.

### 5. Encabezado y filas

```csharp
var headers = lines[0].Split(',');
var rows = lines
    .Skip(1)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => line.Split(','))
    .ToList();
```

La primera línea se interpreta como encabezado.

Las demás líneas son datos.

Importante: esto funciona bien solo para CSV simples. Si hubiera comas dentro de valores entre comillas, este enfoque ya no alcanzaría.

### 6. Índice de columnas

```csharp
var headerIndexes = headers
    .Select((name, index) => new { name, index })
    .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
```

Esto nos permite pasar rápidamente de un nombre de columna como `fecha` a su posición, por ejemplo `2`.

Es útil porque el usuario piensa en nombres de columnas, no en índices numéricos.

### 7. Validación de columnas existentes

```csharp
foreach (var spec in sortSpecs)
{
    if (!headerIndexes.ContainsKey(spec.Field))
    {
        Console.WriteLine($"La columna '{spec.Field}' no existe en el CSV.");
        return;
    }
}
```

Antes de ordenar, confirmamos que todas las columnas pedidas existan en el archivo.

Eso hace que el error aparezca temprano y sea fácil de entender.

### 8. Ordenamiento dinámico

La parte más importante es esta:

```csharp
if (i == 0)
{
    orderedRows = spec.Descending
        ? rows.OrderByDescending(keySelector)
        : rows.OrderBy(keySelector);
}
else
{
    orderedRows = spec.Descending
        ? orderedRows!.ThenByDescending(keySelector)
        : orderedRows!.ThenBy(keySelector);
}
```

La idea es:

- la primera regla usa `OrderBy` o `OrderByDescending`
- las siguientes usan `ThenBy` o `ThenByDescending`

Así obtenemos ordenamiento compuesto.

### 9. Conversión por tipo

```csharp
static IComparable ParseValue(string rawValue, string type) =>
    type switch
    {
        "string" => rawValue,
        "int" => int.Parse(rawValue, CultureInfo.InvariantCulture),
        "decimal" => decimal.Parse(rawValue, CultureInfo.InvariantCulture),
        "date" => DateTime.Parse(rawValue, CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Tipo no soportado: {type}")
    };
```

Esta función transforma cada texto del CSV al tipo indicado por el usuario.

Por ejemplo:

- `"15"` pasa a `int`
- `"2500.50"` pasa a `decimal`
- `"2026-03-20"` pasa a `DateTime`

Eso es importante porque ordenar números como texto produce resultados incorrectos.

Por ejemplo, como `string`, `"100"` quedaría antes que `"20"`, lo cual no queremos.

## Paso 6: crear un CSV de prueba

Creá un archivo llamado `data.csv` en la carpeta del proyecto:

```csv
id,cliente,fecha,total
3,Ana,2026-03-20,150.50
1,Pedro,2026-03-18,220.00
2,Ana,2026-03-18,300.00
4,Juan,2026-03-18,80.00
```

## Paso 7: ejecutar la app

### Ordenar por fecha ascendente y total descendente

```bash
dotnet run -- data.csv --sort fecha:date:asc --sort total:decimal:desc
```

### Salida esperada

```csv
id,cliente,fecha,total
2,Ana,2026-03-18,300.00
1,Pedro,2026-03-18,220.00
4,Juan,2026-03-18,80.00
3,Ana,2026-03-20,150.50
```

### Qué pasó

- primero se agruparon implícitamente las filas por `fecha`
- dentro del mismo día, se ordenaron por `total` descendente

## Paso 8: probar otros casos simples

Ordenar por cliente:

```bash
dotnet run -- data.csv --sort cliente:string:asc
```

Ordenar por id descendente:

```bash
dotnet run -- data.csv --sort id:int:desc
```

## Paso 9: publicar la app como ejecutable

Mientras estamos desarrollando, `dotnet run` está perfecto.

Si quisieras generar una versión distribuible:

```bash
dotnet publish -c Release -o out
```

Eso crea los archivos publicados en `out/`.

Luego podrías ejecutar el binario generado según tu sistema operativo.

## Limitaciones intencionales de esta versión

Elegimos estas limitaciones a propósito para que el ejemplo sea claro:

- asume separador `,`
- asume que la primera línea es encabezado
- no maneja comillas ni comas escapadas
- no valida formatos inválidos de fecha o decimal con mensajes más amigables
- imprime el resultado por consola en vez de escribir otro archivo

Todas esas mejoras son muy buenas siguientes iteraciones, pero no son necesarias para aprender la base.

## Qué conceptos de CLI aprendiste en este ejemplo

Con esta app ya recorriste varias piezas centrales de una CLI real:

1. crear un proyecto de consola con `dotnet new console`
2. recibir argumentos desde `args`
3. definir una convención de uso clara
4. validar entrada
5. leer archivos
6. transformar texto en tipos útiles
7. ordenar datos dinámicamente
8. devolver el resultado por consola

## Próximos pasos naturales

Si después querés evolucionar esta app, estas son buenas mejoras:

- agregar `--output archivo.csv` para guardar el resultado
- soportar separador configurable, por ejemplo `--delimiter ';'`
- usar una librería real de CSV como `CsvHelper`
- usar `System.CommandLine` para una CLI más robusta
- mostrar errores de parseo más claros por fila y columna
- soportar `bool`, `double` y fechas con formato explícito

## Resumen final

La forma más simple de aprender a crear una CLI en .NET es:

1. arrancar con `dotnet new console`
2. diseñar una interfaz de línea de comandos pequeña
3. procesar `args` manualmente
4. resolver un problema concreto de punta a punta

El ejemplo del ordenamiento de CSV es bueno para aprender porque mezcla:

- lectura de archivos
- validación de entrada
- modelado simple
- lógica de negocio
- salida por consola

Con eso ya tenés una base muy sólida para pasar luego a CLIs más completas.
