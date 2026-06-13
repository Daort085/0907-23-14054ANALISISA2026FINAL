namespace EnviosRapidosGT.Models;

public class Envio
{
    public int Id { get; set; }
    public string CodigoRastreo { get; set; } = "";
    public string NombreRemitente { get; set; } = "";
    public string NitRemitente { get; set; } = "";
    public string NombreDestinatario { get; set; } = "";
    public string NitDestinatario { get; set; } = "";
    public string DireccionDestino { get; set; } = "";
    public decimal PesoKg { get; set; }
    public decimal Tarifa { get; set; }
    public string Estado { get; set; } = "Registrado";
    public string OficinaOrigen { get; set; } = "";
    public int IntentoEntrega { get; set; } = 0;
    public string FechaRegistro { get; set; } = DateTime.UtcNow.ToString("o");
}

public class RegistrarEnvioRequest
{
    public string NombreRemitente { get; set; } = "";
    public string NitRemitente { get; set; } = "";
    public string NombreDestinatario { get; set; } = "";
    public string NitDestinatario { get; set; } = "";
    public string DireccionDestino { get; set; } = "";
    public decimal PesoKg { get; set; }
    public string OficinaOrigen { get; set; } = "";
}

public class ActualizarEstadoRequest
{
    public string NuevoEstado { get; set; } = "";
    public string Oficina { get; set; } = "";
    public string? Notas { get; set; }
}
