using Dapper;
using EnviosRapidosGT.Data;
using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.Services;

public class ClienteService
{
    private readonly AppDb _db;
    public ClienteService(AppDb db) => _db = db;

    public async Task<Cliente> RegistrarAsync(RegistrarClienteRequest r)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>(@"
            INSERT INTO Clientes (Nombre,Nit,Telefono,Email)
            VALUES (@Nombre,@Nit,@Telefono,@Email);
            SELECT last_insert_rowid();", r);
        return new Cliente { Id = id, Nombre = r.Nombre, Nit = r.Nit, Telefono = r.Telefono, Email = r.Email };
    }

    public async Task<IEnumerable<Cliente>> ListarAsync()
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryAsync<Cliente>("SELECT * FROM Clientes ORDER BY Nombre");
    }

    public async Task<Cliente?> PorNitAsync(string nit)
    {
        using var c = _db.Conn();
        await c.OpenAsync();
        return await c.QueryFirstOrDefaultAsync<Cliente>(
            "SELECT * FROM Clientes WHERE Nit=@nit", new { nit });
    }
}
