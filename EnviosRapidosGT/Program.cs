using EnviosRapidosGT.Data;
using EnviosRapidosGT.Endpoints;
using EnviosRapidosGT.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AppDb>();
builder.Services.AddScoped<EnvioService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

app.MapGet("/", () => Results.Ok(new { status = "Envíos Rápidos GT API OK", version = "1.0.0" }));

app.MapEnvios();
app.MapClientes();

app.Run();

public partial class Program { }
