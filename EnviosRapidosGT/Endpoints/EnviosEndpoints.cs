using EnviosRapidosGT.Models;
using EnviosRapidosGT.Services;

namespace EnviosRapidosGT.Endpoints;

public static class EnviosEndpoints
{
    public static void MapEnvios(this WebApplication app)
    {
        // RF-1: Registrar envío
        app.MapPost("/api/envios", async (RegistrarEnvioRequest req, EnvioService svc) =>
        {
            if (string.IsNullOrWhiteSpace(req.NombreRemitente) ||
                string.IsNullOrWhiteSpace(req.NombreDestinatario) || req.PesoKg <= 0)
                return Results.BadRequest("Datos incompletos o peso inválido.");
            var envio = await svc.RegistrarAsync(req);
            return Results.Created($"/api/envios/{envio.Id}", envio);
        });

        // RF-2: Rastrear por código
        app.MapGet("/api/envios/rastrear/{codigo}", async (string codigo, EnvioService svc) =>
        {
            var envio = await svc.RastrearAsync(codigo);
            return envio is null
                ? Results.NotFound("Código no encontrado.")
                : Results.Ok(envio);
        });

        // RF-3: Listar todos
        app.MapGet("/api/envios", async (EnvioService svc) =>
            Results.Ok(await svc.ListarAsync()));

        // RF-4: Historial de un envío
        app.MapGet("/api/envios/{id:int}/historial", async (int id, EnvioService svc) =>
            Results.Ok(await svc.HistorialAsync(id)));

        // RF-5: Actualizar estado
        app.MapPut("/api/envios/{id:int}/estado", async (int id, ActualizarEstadoRequest req, EnvioService svc) =>
        {
            if (string.IsNullOrWhiteSpace(req.NuevoEstado) || string.IsNullOrWhiteSpace(req.Oficina))
                return Results.BadRequest("NuevoEstado y Oficina son requeridos.");
            var (ok, msg, envio) = await svc.ActualizarEstadoAsync(id, req);
            return ok ? Results.Ok(new { msg, envio }) : Results.BadRequest(new { msg });
        });

        // RF-6: Filtrar por estado
        app.MapGet("/api/envios/estado/{estado}", async (string estado, EnvioService svc) =>
            Results.Ok(await svc.PorEstadoAsync(estado)));

        // RF-7: Intento fallido
        app.MapPost("/api/envios/{id:int}/intento-fallido", async (int id, ActualizarEstadoRequest req, EnvioService svc) =>
        {
            var (ok, msg, envio) = await svc.IntentoFallidoAsync(id, req.Oficina, req.Notas);
            return ok ? Results.Ok(new { msg, envio }) : Results.BadRequest(new { msg });
        });

        // RF-8: Reporte eficiencia
        app.MapGet("/api/envios/reporte", async (EnvioService svc) =>
            Results.Ok(await svc.ReporteAsync()));

        // RF-9: Eliminar
        app.MapDelete("/api/envios/{id:int}", async (int id, EnvioService svc) =>
        {
            var (ok, msg) = await svc.EliminarAsync(id);
            return ok ? Results.Ok(new { msg }) : Results.BadRequest(new { msg });
        });
    }
}
