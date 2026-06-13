# Envíos Rápidos GT — API REST

**Alumno:** Daniel Alexander Ortiz Cabrera | **Carné:** 0907-23-14065-1

## Ejecutar localmente
```
cd EnviosRapidosGT
dotnet restore
dotnet run
```
API en: http://localhost:5000

## Ejecutar pruebas
```
cd EnviosRapidosGT.Tests
dotnet test
```

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | / | Health check |
| POST | /api/envios | Registrar envío |
| GET | /api/envios | Listar todos |
| GET | /api/envios/rastrear/{codigo} | Rastrear por código |
| PUT | /api/envios/{id}/estado | Actualizar estado |
| GET | /api/envios/{id}/historial | Ver historial |
| GET | /api/envios/estado/{estado} | Filtrar por estado |
| POST | /api/envios/{id}/intento-fallido | Registrar fallo de entrega |
| GET | /api/envios/reporte | Reporte de eficiencia |
| DELETE | /api/envios/{id} | Eliminar (solo si Registrado) |
| POST | /api/clientes | Registrar cliente |
| GET | /api/clientes | Listar clientes |
| GET | /api/clientes/nit/{nit} | Buscar por NIT |

## Diagrama de flujo del proceso

Ver [DIAGRAMA_FLUJO.md](DIAGRAMA_FLUJO.md) para el diagrama de secuencia de
estados del envío y las reglas de negocio aplicadas.

## Despliegue en Render

El proyecto incluye un `Dockerfile` dentro de `EnviosRapidosGT/`.

1. Subir todo el repositorio a GitHub con el nombre `CARNET_ANALISISA2026FINAL`
   (carné: `0907231406S1_ANALISISA2026FINAL`).
2. En [Render.com](https://render.com) → **New** → **Web Service**.
3. Conectar el repositorio de GitHub.
4. Configurar:
   - **Environment:** `Docker`
   - **Dockerfile Path:** `EnviosRapidosGT/Dockerfile`
   - **Docker Build Context Directory:** `EnviosRapidosGT`
   - **Region:** la más cercana (Oregon u Ohio)
   - **Instance Type:** Free
5. El Dockerfile ya expone el puerto `10000` y configura
   `ASPNETCORE_URLS=http://+:10000`. En Render, agregar variable de entorno
   `PORT=10000` si lo solicita.
6. Deploy. La URL pública será algo como
   `https://enviosrapidosgt.onrender.com`.
7. Verificar: `GET https://<tu-app>.onrender.com/` debe responder
   `{"status":"Envíos Rápidos GT API OK","version":"1.0.0"}`.

### Probar localmente con Docker (opcional)
```
cd EnviosRapidosGT
docker build -t enviosrapidosgt .
docker run -p 10000:10000 enviosrapidosgt
```
