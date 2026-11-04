using System;

namespace DXComnTransp.Blazor.Server.Services;

/// <summary>
/// Estado del visor del reporte. Hay una instancia por circuito Blazor
/// (Scoped), así que siempre pertenece a la pestaña donde se hizo clic.
///
/// No guarda nada pesado: solo la URL del PDF (el PDF está en
/// ReporteCompraPdfCache, que se limpia solo). Al cerrar, la URL se
/// anula: no queda ningún estado en memoria.
/// </summary>
public class VisorReporteService
{
    public string Url { get; private set; }

    /// <summary>
    /// Notifica al componente VisorReporte.razor que cambió el estado.
    /// </summary>
    public event Action Cambio;

    public void Abrir(string url)
    {
        Url = url;
        Cambio?.Invoke();
    }

    public void Cerrar()
    {
        Url = null;
        Cambio?.Invoke();
    }
}
