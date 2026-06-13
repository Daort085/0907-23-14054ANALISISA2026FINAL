using Microsoft.Data.Sqlite;

namespace EnviosRapidosGT.Data;

public class AppDb
{
    private readonly string _cs;

    public AppDb(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("Default") ?? "Data Source=envios.db";
        Init();
    }

    public SqliteConnection Conn() => new(_cs);

    private void Init()
    {
        using var c = Conn();
        c.Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Clientes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Nit TEXT UNIQUE NOT NULL,
    Telefono TEXT,
    Email TEXT
);
CREATE TABLE IF NOT EXISTS Envios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CodigoRastreo TEXT UNIQUE NOT NULL,
    NombreRemitente TEXT NOT NULL,
    NitRemitente TEXT NOT NULL,
    NombreDestinatario TEXT NOT NULL,
    NitDestinatario TEXT NOT NULL,
    DireccionDestino TEXT NOT NULL,
    PesoKg REAL NOT NULL,
    Tarifa REAL NOT NULL,
    Estado TEXT NOT NULL DEFAULT 'Registrado',
    OficinaOrigen TEXT NOT NULL,
    IntentoEntrega INTEGER NOT NULL DEFAULT 0,
    FechaRegistro TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Historial (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EnvioId INTEGER NOT NULL,
    Estado TEXT NOT NULL,
    Oficina TEXT NOT NULL,
    Notas TEXT,
    Timestamp TEXT NOT NULL,
    FOREIGN KEY (EnvioId) REFERENCES Envios(Id)
);";
        cmd.ExecuteNonQuery();
    }
}
