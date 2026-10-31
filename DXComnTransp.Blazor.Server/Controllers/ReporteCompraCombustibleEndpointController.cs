using DXComnTransp.Blazor.Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Sirve el reporte imprimible de una compra de combustible.
/// El HTML se genera en la acción "Imprimir Reporte" y se guarda en caché.
/// </summary>
[Route("reportes/compra-combustible")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ReporteCompraCombustibleEndpointController : ControllerBase
{
    [HttpGet("{token}")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Ver(string token)
    {
        string html = ReporteCompraCache.Obtener(token);

        if (string.IsNullOrEmpty(html))
        {
            return Content(PaginaExpirada(), "text/html; charset=utf-8");
        }

        return Content(html, "text/html; charset=utf-8");
    }

    private static string PaginaExpirada()
    {
        return @"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""utf-8"" />
    <title>Reporte no disponible</title>
    <style>
        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: #eef1f5;
            color: #1f2937;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
        }
        .caja {
            background: #fff;
            padding: 32px 38px;
            border-radius: 10px;
            box-shadow: 0 12px 32px rgba(15, 23, 42, .16);
            max-width: 460px;
            text-align: center;
        }
        h1 { font-size: 18px; color: #14345c; margin: 0 0 8px; }
        p { color: #5b6b7f; margin: 0 0 18px; line-height: 1.5; }
        button {
            border: 0;
            border-radius: 6px;
            padding: 9px 18px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            background: #1d6fe0;
            color: #fff;
        }
    </style>
</head>
<body>
    <div class=""caja"">
        <h1>El reporte ya no está disponible</h1>
        <p>Por seguridad, los reportes generados tienen una vigencia limitada.
           Vuelva a la compra y presione de nuevo el botón <strong>Imprimir Reporte</strong>.</p>
        <button type=""button"" onclick=""window.close()"">Cerrar</button>
    </div>
</body>
</html>";
    }
}
