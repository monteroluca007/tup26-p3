using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

string rutaArchivo = "empleados.csv"; 

if (!File.Exists(rutaArchivo))
{
    Console.WriteLine("No se pudo localizar el archivo de datos.");
    return;
}

List<Empleado> nomina = new List<Empleado>();

foreach (string fila in File.ReadLines(rutaArchivo))
{
    string[] datos = fila.Split(',');
    
    if (datos.Length >= 3)
    {
        Empleado emp = new Empleado();
        emp.Nombre = datos[0].Trim();
        emp.Apellido = datos[1].Trim();
        emp.Sueldo = Convert.ToDecimal(datos[2].Trim());
        
        nomina.Add(emp);
    }
}

string parametro = args.Length > 0 ? args[0] : "--apellido";


IEnumerable<Empleado> nominaOrdenada = parametro switch
{
    "--sueldo" => nomina.OrderByDescending(x => x.Sueldo),
    "--nombre" => nomina.OrderBy(x => x.Nombre),
    _ => nomina.OrderBy(x => x.Apellido)
};

Console.WriteLine("--- Listado Procesado ---");
foreach (Empleado e in nominaOrdenada)
{
    Console.WriteLine($"{e.Apellido.ToUpper()}, {e.Nombre} - Salario: ${e.Sueldo}");
}

public class Empleado
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public decimal Sueldo { get; set; }
}


