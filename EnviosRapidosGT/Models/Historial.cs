namespace EnviosRapidosGT.Models;

public class Historial
{
    public int Id { get; set; }
    public int EnvioId { get; set; }
    public string Estado { get; set; } = "";
    public string Oficina { get; set; } = "";
    public string? Notas { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
}

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Nit { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
}

public class RegistrarClienteRequest
{
    public string Nombre { get; set; } = "";
    public string Nit { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
}
