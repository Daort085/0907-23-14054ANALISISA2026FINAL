using Dapper;
using EnviosRapidosGT.Data;
using EnviosRapidosGT.Models;
using EnviosRapidosGT.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class IntegrationTests : IDisposable
{
    // Conexión viva para que SQLite :memory: no se destruya entre llamadas
    private readonly SqliteConnection _alive;
    private readonly EnvioService _svc;

    public IntegrationTests()
    {
        _alive = new SqliteConnection("Data Source=file::memory:?cache=shared&mode=memory");
        _alive.Open();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Data Source=file::memory:?cache=shared&mode=memory"
            })
            .Build();

        _svc = new EnvioService(new AppDb(cfg));
    }

    private RegistrarEnvioRequest Base(decimal kg = 2m) => new()
    {
        NombreRemitente    = "Juan Pérez",
        NitRemitente       = "12345678",
        NombreDestinatario = "María López",
        NitDestinatario    = "87654321",
        DireccionDestino   = "5a Calle 3-20, Zona 1, Guatemala",
        PesoKg             = kg,
        OficinaOrigen      = "Oficina Central"
    };

    [Fact]
    public async Task Registrar_ConNit_AplicaDescuento()
    {
        var e = await _svc.RegistrarAsync(Base(2m)); // base Q45 → 42.75
        Assert.Equal(42.75m, e.Tarifa);
    }

    [Fact]
    public async Task Registrar_SinNit_SinDescuento()
    {
        var r = Base(2m);
        r.NitRemitente = r.NitDestinatario = "";
        var e = await _svc.RegistrarAsync(r);
        Assert.Equal(45.00m, e.Tarifa);
    }

    [Fact]
    public async Task Registrar_EstadoInicial_Registrado()
    {
        var e = await _svc.RegistrarAsync(Base());
        Assert.Equal("Registrado", e.Estado);
    }

    [Fact]
    public async Task Rastrear_CodigoExiste_RetornaEnvio()
    {
        var e = await _svc.RegistrarAsync(Base());
        var r = await _svc.RastrearAsync(e.CodigoRastreo);
        Assert.NotNull(r);
        Assert.Equal(e.CodigoRastreo, r!.CodigoRastreo);
    }

    [Fact]
    public async Task Rastrear_CodigoInexistente_Null()
    {
        var r = await _svc.RastrearAsync("ENV-00000000-0000");
        Assert.Null(r);
    }

    [Fact]
    public async Task ActualizarEstado_Valido_OK()
    {
        var e = await _svc.RegistrarAsync(Base());
        var (ok, _, upd) = await _svc.ActualizarEstadoAsync(e.Id,
            new ActualizarEstadoRequest { NuevoEstado = "EnTransito", Oficina = "Bodega Norte" });
        Assert.True(ok);
        Assert.Equal("EnTransito", upd!.Estado);
    }

    [Fact]
    public async Task ActualizarEstado_Invalido_Error()
    {
        var e = await _svc.RegistrarAsync(Base());
        var (ok, msg, _) = await _svc.ActualizarEstadoAsync(e.Id,
            new ActualizarEstadoRequest { NuevoEstado = "Entregado", Oficina = "X" });
        Assert.False(ok);
        Assert.Contains("inválida", msg);
    }

    [Fact]
    public async Task IntentoFallido_Tercero_EnDevolucion()
    {
        var e = await _svc.RegistrarAsync(Base());
        await _svc.ActualizarEstadoAsync(e.Id, new() { NuevoEstado="EnTransito", Oficina="A" });
        await _svc.ActualizarEstadoAsync(e.Id, new() { NuevoEstado="EnReparto",  Oficina="B" });
        await _svc.IntentoFallidoAsync(e.Id, "B");
        await _svc.IntentoFallidoAsync(e.Id, "B");
        var (ok, _, upd) = await _svc.IntentoFallidoAsync(e.Id, "B");
        Assert.True(ok);
        Assert.Equal("EnDevolucion", upd!.Estado);
    }

    [Fact]
    public async Task Historial_RegistraCambios()
    {
        var e = await _svc.RegistrarAsync(Base());
        await _svc.ActualizarEstadoAsync(e.Id, new() { NuevoEstado="EnTransito", Oficina="X" });
        var h = (await _svc.HistorialAsync(e.Id)).ToList();
        Assert.True(h.Count >= 2);
        Assert.Contains(h, x => x.Estado == "Registrado");
        Assert.Contains(h, x => x.Estado == "EnTransito");
    }

    [Fact]
    public async Task Eliminar_EstadoRegistrado_OK()
    {
        var e = await _svc.RegistrarAsync(Base());
        var (ok, _) = await _svc.EliminarAsync(e.Id);
        Assert.True(ok);
    }

    [Fact]
    public async Task Eliminar_EnTransito_Error()
    {
        var e = await _svc.RegistrarAsync(Base());
        await _svc.ActualizarEstadoAsync(e.Id, new() { NuevoEstado="EnTransito", Oficina="A" });
        var (ok, _) = await _svc.EliminarAsync(e.Id);
        Assert.False(ok);
    }

    public void Dispose() => _alive.Dispose();
}
