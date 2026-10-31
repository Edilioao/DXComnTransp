using System;
using System.Collections.Concurrent;

namespace DXComnTransp.Blazor.Server.Services;

/// <summary>
/// Almacena temporalmente el HTML de los reportes generados para que el
/// navegador pueda abrirlos en una pestaña nueva sin volver a consultar la BD.
/// </summary>
public static class ReporteCompraCache
{
    private sealed class Entrada
    {
        public string Html { get; init; } = string.Empty;
        public DateTime Expira { get; init; }
    }

    private static readonly ConcurrentDictionary<string, Entrada> Entradas =
        new ConcurrentDictionary<string, Entrada>();

    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(30);

    public static string Guardar(string html)
    {
        Limpiar();

        string token = Guid.NewGuid().ToString("N");

        Entradas[token] = new Entrada
        {
            Html = html ?? string.Empty,
            Expira = DateTime.UtcNow.Add(Vigencia)
        };

        return token;
    }

    public static string Obtener(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        if (!Entradas.TryGetValue(token, out Entrada entrada))
            return null;

        if (entrada.Expira < DateTime.UtcNow)
        {
            Entradas.TryRemove(token, out _);
            return null;
        }

        return entrada.Html;
    }

    private static void Limpiar()
    {
        DateTime ahora = DateTime.UtcNow;

        foreach (var par in Entradas)
        {
            if (par.Value.Expira < ahora)
                Entradas.TryRemove(par.Key, out _);
        }
    }
}
