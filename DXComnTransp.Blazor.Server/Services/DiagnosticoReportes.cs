using System;
using System.IO;

namespace DXComnTransp.Blazor.Server.Services;

/// <summary>
/// Diagnóstico del flujo de reportes. Escribe en %TEMP%\dx-reportes.log.
/// Nunca debe romper la generación ni la apertura del reporte.
/// </summary>
public static class DiagnosticoReportes
{
    private static readonly object Candado = new object();

    public static void Registrar(string mensaje)
    {
        try
        {
            string archivo = Path.Combine(
                Path.GetTempPath(),
                "dx-reportes.log");

            lock (Candado)
            {
                File.AppendAllText(
                    archivo,
                    $"{DateTime.Now:HH:mm:ss.fff} {mensaje}{Environment.NewLine}");
            }
        }
        catch
        {
            // El diagnóstico nunca debe romper el flujo del reporte.
        }
    }
}
