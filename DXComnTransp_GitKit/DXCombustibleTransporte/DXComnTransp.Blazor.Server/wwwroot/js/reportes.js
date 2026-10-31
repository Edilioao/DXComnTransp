// Abre el reporte de compra en una pestaña nueva.
// Si el navegador bloquea la ventana emergente, navega en la pestaña actual.
window.dxAbrirReporte = function (url) {
    try {
        var ventana = window.open(url, '_blank');

        if (!ventana) {
            window.location.href = url;
        }
    } catch (e) {
        window.location.href = url;
    }
};
