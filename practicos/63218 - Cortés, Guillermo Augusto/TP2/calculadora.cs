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
}
