using System;
using System.Collections.Concurrent;

namespace DXComnTransp.Blazor.Server.Services;

/// <summary>
/// Almacena temporalmente los PDF de los reportes DevExpress generados,
/// para que el navegador pueda abrirlos en una pestaña nueva mediante un
/// token sin volver a consultar la base de datos.
/// Lo usa la acción "Imprimir Compra" (CompraCombustibleImprimirController).
/// </summary>
public static class ReporteCompraPdfCache
{
    private sealed class Entrada
    {
        public byte[] Pdf { get; init; }
        public string NombreArchivo { get; init; }
        public DateTime Expira { get; init; }
    }

    private static readonly ConcurrentDictionary<string, Entrada> Entradas =
        new ConcurrentDictionary<string, Entrada>();

    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(15);

    // Tope de PDF en memoria: si se imprimen muchas compras seguidas, se
    // descartan los más antiguos para no acumular nada indefinidamente.
    private const int MaximoEntradas = 10;

    public static string Guardar(byte[] pdf, string nombreArchivo)
    {
        Limpiar();

        string token = Guid.NewGuid().ToString("N");

        Entradas[token] = new Entrada
        {
            Pdf = pdf ?? Array.Empty<byte>(),
            NombreArchivo = string.IsNullOrWhiteSpace(nombreArchivo)
                ? "reporte.pdf"
                : nombreArchivo,
            Expira = DateTime.UtcNow.Add(Vigencia)
        };

        Recortar();

        return token;
    }

    public static (byte[] Pdf, string NombreArchivo) Obtener(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (null, null);

        if (!Entradas.TryGetValue(token, out Entrada entrada))
            return (null, null);

        if (entrada.Expira < DateTime.UtcNow)
        {
            Entradas.TryRemove(token, out _);
            return (null, null);
        }

        return (entrada.Pdf, entrada.NombreArchivo);
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

    /// <summary>
    /// Si hay más PDF de los permitidos, elimina los que expiran antes
    /// (es decir, los generados primero).
    /// </summary>
    private static void Recortar()
    {
        while (Entradas.Count > MaximoEntradas)
        {
            string masViejo = null;
            DateTime expiraMasViejo = DateTime.MaxValue;

            foreach (var par in Entradas)
            {
                if (par.Value.Expira < expiraMasViejo)
                {
                    expiraMasViejo = par.Value.Expira;
                    masViejo = par.Key;
                }
            }

            if (masViejo == null)
                return;

            Entradas.TryRemove(masViejo, out _);
        }
    }
}
