#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

public class Integer : IComparable<Integer>
{
    private List<int> digitos;
    private bool negativo;

    public Integer(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número inválido");

        if (number[0] == '-')
        {
            negativo = true;
            number = number.Substring(1);
        }
        else negativo = false;

        digitos = number.Select(c =>
        {
            if (!char.IsDigit(c)) throw new ArgumentException("Número inválido");
            return c - '0';
        }).ToList();

        Normalizar();
    }

    private Integer(List<int> digitos, bool negativo)
    {
        this.digitos = digitos;
        this.negativo = negativo;
        Normalizar();
    }

    private void Normalizar()
    {
        while (digitos.Count > 1 && digitos[0] == 0)
            digitos.RemoveAt(0);

        if (digitos.Count == 1 && digitos[0] == 0)
            negativo = false;
    }
    public static Integer Parse(string texto) => new Integer(texto);

public static implicit operator Integer(int valor)
{
    return new Integer(valor.ToString());
}

public override string ToString()
{
    return (negativo ? "-" : "") + string.Join("", digitos);
}

public bool IsZero() => digitos.Count == 1 && digitos[0] == 0;
public bool IsNegative() => negativo;

public int CompareTo(Integer other)
{
    if (negativo != other.negativo)
        return negativo ? -1 : 1;

    if (digitos.Count != other.digitos.Count)
    {
        int cmp = digitos.Count.CompareTo(other.digitos.Count);
        return negativo ? -cmp : cmp;
    }

    for (int i = 0; i < digitos.Count; i++)
    {
        if (digitos[i] != other.digitos[i])
        {
            int cmp = digitos[i].CompareTo(other.digitos[i]);
            return negativo ? -cmp : cmp;
        }
    }

    return 0;
}

public bool Equals(Integer other) => CompareTo(other) == 0;
public static Integer operator +(Integer a, Integer b)
{
    if (a.negativo == b.negativo)
        return new Integer(Sumar(a.digitos, b.digitos), a.negativo);

    if (AbsMayor(a, b))
        return new Integer(Restar(a.digitos, b.digitos), a.negativo);

    return new Integer(Restar(b.digitos, a.digitos), b.negativo);
}

public static Integer operator -(Integer a, Integer b)
{
    return a + new Integer(b.digitos, !b.negativo);
}

private static List<int> Sumar(List<int> a, List<int> b)
{
    var res = new List<int>();
    int carry = 0;

    a = a.ToList(); b = b.ToList();
    a.Reverse(); b.Reverse();

    int max = Math.Max(a.Count, b.Count);

    for (int i = 0; i < max; i++)
    {
        int da = i < a.Count ? a[i] : 0;
        int db = i < b.Count ? b[i] : 0;

        int suma = da + db + carry;
        res.Add(suma % 10);
        carry = suma / 10;
    }

    if (carry > 0) res.Add(carry);
    res.Reverse();
    return res;
}

private static List<int> Restar(List<int> a, List<int> b)
{
    var res = new List<int>();
    int borrow = 0;

    a = a.ToList(); b = b.ToList();
    a.Reverse(); b.Reverse();

    for (int i = 0; i < a.Count; i++)
    {
        int da = a[i];
        int db = i < b.Count ? b[i] : 0;

        int r = da - db - borrow;
        if (r < 0)
        {
            r += 10;
            borrow = 1;
        }
        else borrow = 0;

        res.Add(r);
    }

    res.Reverse();
    return res;
}

private static bool AbsMayor(Integer a, Integer b)
{
    if (a.digitos.Count != b.digitos.Count)
        return a.digitos.Count > b.digitos.Count;

    for (int i = 0; i < a.digitos.Count; i++)
    {
        if (a.digitos[i] != b.digitos[i])
            return a.digitos[i] > b.digitos[i];
    }

    return true;
}
public static Integer operator *(Integer a, Integer b)
{
    int[] res = new int[a.digitos.Count + b.digitos.Count];

    for (int i = a.digitos.Count - 1; i >= 0; i--)
    {
        for (int j = b.digitos.Count - 1; j >= 0; j--)
        {
            int mul = a.digitos[i] * b.digitos[j];
            int suma = res[i + j + 1] + mul;

            res[i + j + 1] = suma % 10;
            res[i + j] += suma / 10;
        }
    }

    return new Integer(res.ToList(), a.negativo ^ b.negativo);
}
public static Integer operator /(Integer a, Integer b)
{
    if (b.IsZero())
        throw new DivideByZeroException();

    Integer resto = new Integer("0");
    Integer cociente = new Integer("0");

    foreach (var d in a.digitos)
    {
        resto = resto * 10 + new Integer(d.ToString());

        int count = 0;
        while (resto.CompareTo(b) >= 0)
        {
            resto -= b;
            count++;
        }

        cociente = cociente * 10 + new Integer(count.ToString());
    }

    return cociente;
}

public static Integer operator %(Integer a, Integer b)
{
    return a - (a / b) * b;
}
}
