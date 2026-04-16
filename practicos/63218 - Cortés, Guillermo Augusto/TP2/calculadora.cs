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
}