using Dapper;
using EnviosRapidosGT.Data;
using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.Services;

public class EnvioService
{
    private readonly AppDb _db;
    public EnvioService(AppDb db) => _db = db;

    // ── Regla 1: tarifa por peso ──────────────────────────────────────
    public static decimal CalcularTarifa(decimal kg) => kg switch
    {
        <= 1m  => 25.00m,
        <= 5m  => 45.00m,
        <= 10m => 75.00m,
        _      => 100.00m
    };

    // ── Regla 7: NIT válido ───────────────────────────────────────────
    public static bool NitValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit)) return false;
        return nit.Replace("-", "").All(char.IsDigit);
    }

    // ── Regla 5: código ENV-YYYYMMDD-XXXX ────────────────────────────
    public static string GenerarCodigo()
    {
        var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
        var num   = new Random().Next(1000, 9999);
        return $"ENV-{fecha}-{num}";
    }

    // ── RF-1: Registrar envío ─────────────────────────────────────────
    public async Task<Envio> RegistrarAsync(RegistrarEnvioRequest r)
    {
        var tarifa = CalcularTarifa(r.PesoKg);
        if (NitValido(r.NitRemitente) || NitValido(r.NitDestinatario))
            tarifa = Math.Round(tarifa * 0.95m, 2);

        var envio = new Envio
        {
            CodigoRastreo      = GenerarCodigo(),
            NombreRemitente    = r.NombreRemitente,
            NitRemitente       = r.NitRemitente,
            NombreDestinatario = r.NombreDestinatario,
            NitDestinatario    = r.NitDestinatario,
            DireccionDestino   = r.DireccionDestino,
            PesoKg             = r.PesoKg,
            Tarifa             = tarifa,
            Estado             = "Registrado",
            OficinaOrigen      = r.OficinaOrigen,
            IntentoEntrega     = 0,
            FechaRegistro      = DateTime.UtcNow.ToString("o")
        };

        using var c = _db.Conn();
        await c.OpenAsync();
        envio.Id = await c.ExecuteScalarAsync<int>(@"
            INSERT INTO Envios
                (CodigoRastreo,NombreRemitente,NitRemitente,NombreDestinatario,
                 NitDestinatario,DireccionDestino,PesoKg,Tarifa,Estado,
                 OficinaOrigen,IntentoEntrega,FechaRegistro)
            VALUES
                (@CodigoRastreo,@NombreRemitente,@NitRemitente,@NombreDestinatario,
                 @NitDestinatario,@DireccionDestino,@PesoKg,@Tarifa,@Estado,
                 @OficinaOrigen,@IntentoEntrega,@FechaRegistro);
            SELECT last_insert_rowid();", envio);

        await GuardarHistorialAsync(c, envio.Id, "Registrado", r.OficinaOrigen, "Envío registrado");
        return envio;
    }

    // ── RF-2: Rastrear ────────────────────────────────────────────────
    public async Task<Envio?> RastrearAsync(string codigo)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryFirstOrDefaultAsync<Envio>(
            "SELECT * FROM Envios WHERE CodigoRastreo=@codigo", new { codigo });
    }

    // ── RF-3: Listar todos ────────────────────────────────────────────
    public async Task<IEnumerable<Envio>> ListarAsync()
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryAsync<Envio>("SELECT * FROM Envios ORDER BY FechaRegistro DESC");
    }

    // ── RF-4: Historial ───────────────────────────────────────────────
    public async Task<IEnumerable<Historial>> HistorialAsync(int envioId)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryAsync<Historial>(
            "SELECT * FROM Historial WHERE EnvioId=@envioId ORDER BY Timestamp", new { envioId });
    }

    // ── RF-5: Actualizar estado (Reglas 2, 3, 4, 6) ──────────────────
    public async Task<(bool Ok, string Msg, Envio? Envio)> ActualizarEstadoAsync(
        int id, ActualizarEstadoRequest r)
    {
        using var c = _db.Conn();
        await c.OpenAsync();

        var envio = await c.QueryFirstOrDefaultAsync<Envio>(
            "SELECT * FROM Envios WHERE Id=@id", new { id });

        if (envio is null) return (false, "Envío no encontrado", null);
        if (!TransicionValida(envio.Estado, r.NuevoEstado))
            return (false, $"Transición inválida: {envio.Estado} → {r.NuevoEstado}", null);

        envio.Estado = r.NuevoEstado;
        await c.ExecuteAsync("UPDATE Envios SET Estado=@Estado WHERE Id=@Id", envio);
        await GuardarHistorialAsync(c, id, r.NuevoEstado, r.Oficina, r.Notas);
        return (true, "Estado actualizado", envio);
    }

    // ── RF-6: Filtrar por estado ──────────────────────────────────────
    public async Task<IEnumerable<Envio>> PorEstadoAsync(string estado)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryAsync<Envio>(
            "SELECT * FROM Envios WHERE Estado=@estado ORDER BY FechaRegistro DESC",
            new { estado });
    }

    // ── RF-7: Intento fallido (Regla 2) ──────────────────────────────
    public async Task<(bool Ok, string Msg, Envio? Envio)> IntentoFallidoAsync(
        int id, string oficina, string? notas = null)
    {
        using var c = _db.Conn();
        await c.OpenAsync();

        var envio = await c.QueryFirstOrDefaultAsync<Envio>(
            "SELECT * FROM Envios WHERE Id=@id", new { id });

        if (envio is null)      return (false, "Envío no encontrado", null);
        if (envio.Estado != "EnReparto") return (false, "El envío no está EnReparto", null);

        envio.IntentoEntrega++;
        envio.Estado = envio.IntentoEntrega >= 3 ? "EnDevolucion" : "EnReparto";

        await c.ExecuteAsync(
            "UPDATE Envios SET Estado=@Estado, IntentoEntrega=@IntentoEntrega WHERE Id=@Id", envio);

        var msg = envio.IntentoEntrega >= 3
            ? "3 intentos fallidos — pasando a EnDevolucion"
            : $"Intento {envio.IntentoEntrega} fallido";

        await GuardarHistorialAsync(c, id, envio.Estado, oficina, notas ?? msg);
        return (true, msg, envio);
    }

    // ── RF-8: Reporte de eficiencia ───────────────────────────────────
    public async Task<object> ReporteAsync()
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        var total      = await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Envios");
        var entregados = await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Envios WHERE Estado='Entregado'");
        var devueltos  = await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Envios WHERE Estado='Devuelto'");
        var enProceso  = total - entregados - devueltos;
        return new
        {
            Total      = total,
            Entregados = entregados,
            Devueltos  = devueltos,
            EnProceso  = enProceso,
            PorcentajeExito = total > 0 ? Math.Round((double)entregados / total * 100, 2) : 0.0
        };
    }

    // ── RF-9: Eliminar (solo si Registrado) ──────────────────────────
    public async Task<(bool Ok, string Msg)> EliminarAsync(int id)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        var envio = await c.QueryFirstOrDefaultAsync<Envio>(
            "SELECT * FROM Envios WHERE Id=@id", new { id });
        if (envio is null) return (false, "Envío no encontrado");
        if (envio.Estado != "Registrado") return (false, "Solo se pueden eliminar envíos en estado Registrado");
        await c.ExecuteAsync("DELETE FROM Historial WHERE EnvioId=@id", new { id });
        await c.ExecuteAsync("DELETE FROM Envios WHERE Id=@id", new { id });
        return (true, "Eliminado correctamente");
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private static async Task GuardarHistorialAsync(
        Microsoft.Data.Sqlite.SqliteConnection c,
        int envioId, string estado, string oficina, string? notas)
    {
        await c.ExecuteAsync(@"
            INSERT INTO Historial (EnvioId,Estado,Oficina,Notas,Timestamp)
            VALUES (@envioId,@estado,@oficina,@notas,@ts)",
            new { envioId, estado, oficina, notas, ts = DateTime.UtcNow.ToString("o") });
    }

    private static bool TransicionValida(string actual, string siguiente) =>
        (actual, siguiente) switch
        {
            ("Registrado",   "EnTransito")   => true,
            ("EnTransito",   "EnReparto")    => true,
            ("EnReparto",    "Entregado")    => true,
            ("EnReparto",    "EnDevolucion") => true,
            ("EnTransito",   "EnDevolucion") => true,
            ("EnDevolucion", "Devuelto")     => true,
            _ => false
        };
}
