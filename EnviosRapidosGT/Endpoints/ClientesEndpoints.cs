using EnviosRapidosGT.Models;
using EnviosRapidosGT.Services;

namespace EnviosRapidosGT.Endpoints;

public static class ClientesEndpoints
{
    public static void MapClientes(this WebApplication app)
    {
        // RF-10: Registrar cliente
        app.MapPost("/api/clientes", async (RegistrarClienteRequest req, ClienteService svc) =>
        {
            if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Nit))
                return Results.BadRequest("Nombre y NIT son requeridos.");
            var c = await svc.RegistrarAsync(req);
            return Results.Created($"/api/clientes/{c.Id}", c);
        });

        // RF-11: Listar clientes
        app.MapGet("/api/clientes", async (ClienteService svc) =>
            Results.Ok(await svc.ListarAsync()));

        // RF-12: Buscar por NIT
        app.MapGet("/api/clientes/nit/{nit}", async (string nit, ClienteService svc) =>
        {
            var c = await svc.PorNitAsync(nit);
            return c is null ? Results.NotFound("Cliente no encontrado.") : Results.Ok(c);
        });
    }
}
