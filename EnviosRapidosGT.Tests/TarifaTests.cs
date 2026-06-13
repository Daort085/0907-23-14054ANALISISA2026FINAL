using EnviosRapidosGT.Services;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class TarifaTests
{
    [Fact]
    public void Tarifa_Peso05_Es25() =>
        Assert.Equal(25.00m, EnvioService.CalcularTarifa(0.5m));

    [Fact]
    public void Tarifa_Peso1_Es25() =>
        Assert.Equal(25.00m, EnvioService.CalcularTarifa(1m));

    [Fact]
    public void Tarifa_Peso3_Es45() =>
        Assert.Equal(45.00m, EnvioService.CalcularTarifa(3m));

    [Fact]
    public void Tarifa_Peso5_Es45() =>
        Assert.Equal(45.00m, EnvioService.CalcularTarifa(5m));

    [Fact]
    public void Tarifa_Peso7_Es75() =>
        Assert.Equal(75.00m, EnvioService.CalcularTarifa(7m));

    [Fact]
    public void Tarifa_Peso10_Es75() =>
        Assert.Equal(75.00m, EnvioService.CalcularTarifa(10m));

    [Fact]
    public void Tarifa_Peso15_Es100() =>
        Assert.Equal(100.00m, EnvioService.CalcularTarifa(15m));

    [Fact]
    public void NitValido_Numeros_True() =>
        Assert.True(EnvioService.NitValido("12345678"));

    [Fact]
    public void NitValido_ConGuion_True() =>
        Assert.True(EnvioService.NitValido("1234567-8"));

    [Fact]
    public void NitValido_Vacio_False() =>
        Assert.False(EnvioService.NitValido(""));

    [Fact]
    public void NitValido_Letras_False() =>
        Assert.False(EnvioService.NitValido("ABC123"));

    [Fact]
    public void Codigo_FormatoENV()
    {
        var codigo = EnvioService.GenerarCodigo();
        Assert.StartsWith("ENV-", codigo);
        Assert.Matches(@"^ENV-\d{8}-\d{4}$", codigo);
    }
}
