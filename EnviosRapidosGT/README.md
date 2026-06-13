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
## Historias de Usuario

### HU-01 — Registrar envío con tarifa automática
**Como** operador **quiero** registrar un envío con peso **para** que el sistema calcule la tarifa automáticamente.
- ≤1kg=Q25 | 1.01-5kg=Q45 | 5.01-10kg=Q75 | >10kg=Q100
- Genera código ENV-YYYYMMDD-XXXX | Estado inicial: Registrado
**Endpoint:** POST /api/envios

### HU-02 — Descuento por NIT válido
**Como** sistema **quiero** aplicar 5% de descuento si remitente o destinatario tiene NIT válido **para** incentivar clientes registrados.
- NIT válido: solo dígitos (con o sin guión)
**Endpoint:** POST /api/envios

### HU-03 — Actualizar estado con validación de flujo
**Como** operador **quiero** cambiar el estado del envío **para** mantener el rastreo actualizado.
- Solo transiciones válidas: Registrado→EnTransito→EnReparto→Entregado/EnDevolucion→Devuelto
- Cada cambio registra oficina y timestamp
**Endpoint:** PUT /api/envios/{id}/estado

### HU-04 — Rastrear envío por código
**Como** cliente **quiero** consultar mi paquete con el código de rastreo **para** saber dónde está sin llamar.
- Formato código: ENV-YYYYMMDD-XXXX | Retorna 404 si no existe
**Endpoint:** GET /api/envios/rastrear/{codigo}

### HU-05 — Ver historial completo
**Como** supervisor **quiero** ver todos los cambios de estado de un envío **para** auditar el proceso.
- Muestra estado, oficina, timestamp, notas en orden cronológico
**Endpoint:** GET /api/envios/{id}/historial

### HU-06 — Registrar intento fallido de entrega
**Como** repartidor **quiero** registrar cuando no puedo entregar **para** que al 3er intento se active la devolución automática.
- Solo aplica a envíos en EnReparto | Al 3er fallo → EnDevolucion automático
**Endpoint:** POST /api/envios/{id}/intento-fallido

### HU-07 — Filtrar envíos por estado
**Como** coordinador **quiero** ver envíos por estado específico **para** priorizar operaciones.
**Endpoint:** GET /api/envios/estado/{estado}

### HU-08 — Reporte de eficiencia
**Como** gerente **quiero** ver resumen de entregas exitosas/fallidas/en proceso **para** evaluar desempeño.
- Muestra total, entregados, devueltos, en proceso, % de éxito
**Endpoint:** GET /api/envios/reporte

### HU-09 — Eliminar envío mal registrado
**Como** operador **quiero** eliminar un envío erróneo **para** mantener limpia la base de datos.
- Solo si está en estado Registrado
**Endpoint:** DELETE /api/envios/{id}

### HU-10 — Registrar cliente con NIT
**Como** ejecutivo **quiero** registrar clientes con su NIT **para** identificarlos en futuras transacciones.
- Campos requeridos: Nombre, NIT (único)
**Endpoint:** POST /api/clientes

### HU-11 — Listar clientes
**Como** ejecutivo **quiero** ver todos los clientes **para** gestionar cuentas comerciales.
**Endpoint:** GET /api/clientes

### HU-12 — Buscar cliente por NIT
**Como** operador **quiero** buscar un cliente por NIT al registrar un envío **para** autocompletar sus datos.
**Endpoint:** GET /api/clientes/nit/{nit}


## API desplegada

🔗 **URL en producción:** https://zero907-23-14054analisisa2026final.onrender.com/

> Nota: el plan Free de Render duerme el servicio tras ~15 min de inactividad.
> La primera petición puede tardar 30-60 segundos en responder mientras el
> contenedor "despierta".

### Probar el health check