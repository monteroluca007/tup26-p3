# Trabajo Práctico 2 — Calculadora de Enteros Grandes: `Calculadora`

**Entrega:**  16 de ABRIL de 2025 a las 23:59hs

---

## Descripción

Desarrollar una calculadora de enteros grandes llamada **`Calculadora`** que permita realizar operaciones aritméticas con números de hasta 1024 bits (309 dígitos decimales).

---

## Sintaxis

```
calculadora [formula] [--help]
```

---

## Opciones

| Opción larga | Corta | Descripción                           |
|--------------|-------|---------------------------------------|
| `formula`    | —     | La fórmula a evaluar, entre comillas. |
| `--help`     | `-h`  | Muestra la ayuda y termina.           |




---

## Comportamiento esperado

Si se le pasa la formula debe retornar el resultado de la misma. 
Si se le pasa `--help` o `-h`, debe mostrar la ayuda y terminar con código de salida 0.
Si se pasa sin argumentos debe implementar una interface interactiva, donde el usuario pueda ingresar fórmulas una por una, y el programa las evalúe y muestre el resultado, hasta que el usuario decida salir (por ejemplo, ingresando "fin" o "salir").

## Requisitos 

La calculadora debe soportar las siguientes operaciones:
- Suma: `+`
- Resta: `-`
- Multiplicación: `*`
- División entera: `/`
- Módulo: `%`
- Parentesis: `(` y `)`
- Números enteros de hasta 1024 bits (309 dígitos decimales).
- El programa debe manejar correctamente el orden de las operaciones y los paréntesis.

## Ejemplos de uso

```bash
calculadora "12345678901234567890 + 98765432109876543210" 
# Salida: 111111111011111111100

calculadora "2 * (3 + 4)" 
# Salida: 14

calculadora "10 / 3" // Salida: 
# Salida:3

calculadora "10 % 3" // Salida: 1
```

## Diseño requerido

El programa debe implementar dos funciones principales.

1. Modelar un tipo de dato nuevo para representar enteros grandes, con las operaciones aritméticas basicas.
```cs
class Integer {
    // Constructor que recibe un string con el número decimal
    public Integer(string number);

    public Integer operator -(Integer numero);                  // Operador '-' (negación unaria)
    public Integer operator +(Integer numero);                  // Operador '+' (unario, para indicar que el número es positivo)
    public Integer operator +(Integer left, Integer right);     // Operador '+' (suma),
    public Integer operator -(Integer left, Integer right);     // Operador '-' (resta),
    public Integer operator *(Integer left, Integer right);     // Operador '*' (multiplicación),
    public Integer operator /(Integer left, Integer right);     // Operador '/' (división),
    public Integer operator %(Integer left, Integer right);     // Operador '%' (módulo),

    public bool Equals(Integer other);                          // Método para comparar dos enteros grandes
    public int CompareTo(Integer other);                        // Método para comparar dos enteros grandes (menor, igual, mayor)
    public bool IsZero();                                       // Método para verificar si el entero es cero
    public bool IsNegative(); // Método para verificar si el entero es negativo
    public 

    // Método para convertir el número a string
    public static Integer Parse(string texto);                  // Método para convertir un número entero (como un string) en un objeto Integer
    public static implicit operator Integer(int valor);         // Operador de conversión implícita para convertir un entero normal a Integer
    
    public override string ToString();
}
```

```cs 
class Calculadora {
    public static Integer Evaluar(string formula){
        var tokens = Tokenizar(formula);
        var ast = Parsear(tokens);
        return Evaluar(ast);
    }; // Método para evaluar una fórmula y retornar el resultado como un objeto Integer

    private static List<Token> Tokenizar(string formula);  // Método para convertir la fórmula en una lista de tokens (tokenización o análisis léxico)
    private static Node Parsear(List<Token> tokens);       // Método para convertir la lista de tokens en una
    private static Integer Evaluar(Node ast);              // Método para evaluar la estructura de datos (AST) y retornar el resultado como un objeto Integer
}

## Ideas para el desarrollo.

Para implementar el tipo de dato `Integer`, se puede representar el número como una lista de dígitos, junto con un signo para indicar si es positivo o negativo. 
Las operaciones aritméticas se pueden implementar utilizando algoritmos clásicos de suma, resta, multiplicación y división para números grandes (el que aprendimos en la primaria).

Para evaluar la expresion aritmética, se puede seguir el siguiente proceso:
1. Tokenizar la fórmula: convertir la cadena de texto en una lista de tokens (números, operadores, paréntesis).
2. Parsear los tokens: convertir la lista de tokens en una estructura de datos que represente la fórmula, como un árbol de sintaxis abstracta. 
2.1 Para convertir se puede usar el algoritmo de analisis descendente recursivo, o el algoritmo de Shunting Yard de Dijkstra.
2.2 Para representar la formula se puede usar un Arbol de Sintaxis Abstracta (AST), donde cada nodo representa una operación o un número.
3. Evaluar el AST: recorrer el árbol y evaluar cada operación, utilizando las operaciones defin

```
1. Implementar un parser para convertir la fórmula ingresada en una estructura de datos que pueda ser evaluada, respetando el orden de las operaciones y los paréntesis.
   
El programa debe seguir el siguiente pipeline, implementando cada paso como una función local independiente:

```
1. Si pasa la formula como argumento, evaluarla y mostrar el resultado.
2. Si se llama sin argumentos, iniciar una interface interactiva para ingresar fórmulas una por una.
3. Evaluar una expresion aritmética con números enteros grandes, respetando el orden de las operaciones y los paréntesis.
3.1. Convertir la formula en una lista de tokens (tokenización o análisis léxico).
3.2. Convertir la lista de tokens en una estructura de datos que represente la fórmula (análisis sintáctico o parsing).
3.3. Evaluar la estructura de datos
1. Mostrar el resultado.
```



