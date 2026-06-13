# Historias de Usuario — Envíos Rápidos GT
**Alumno:** Daniel Alexander Ortiz Cabrera | **Carné:** 0907-23-14065-1

## HU-01 — Registrar envío con tarifa automática
**Como** operador **quiero** registrar un envío con peso **para** que el sistema calcule la tarifa automáticamente.
- ≤1kg=Q25 | 1.01-5kg=Q45 | 5.01-10kg=Q75 | >10kg=Q100
- Genera código ENV-YYYYMMDD-XXXX | Estado inicial: Registrado
**Endpoint:** POST /api/envios

## HU-02 — Descuento por NIT válido
**Como** sistema **quiero** aplicar 5% de descuento si remitente o destinatario tiene NIT válido **para** incentivar clientes registrados.
- NIT válido: solo dígitos (con o sin guión)
**Endpoint:** POST /api/envios

## HU-03 — Actualizar estado con validación de flujo
**Como** operador **quiero** cambiar el estado del envío **para** mantener el rastreo actualizado.
- Solo transiciones válidas: Registrado→EnTransito→EnReparto→Entregado/EnDevolucion→Devuelto
- Cada cambio registra oficina y timestamp
**Endpoint:** PUT /api/envios/{id}/estado

## HU-04 — Rastrear envío por código
**Como** cliente **quiero** consultar mi paquete con el código de rastreo **para** saber dónde está sin llamar.
- Formato código: ENV-YYYYMMDD-XXXX | Retorna 404 si no existe
**Endpoint:** GET /api/envios/rastrear/{codigo}

## HU-05 — Ver historial completo
**Como** supervisor **quiero** ver todos los cambios de estado de un envío **para** auditar el proceso.
- Muestra estado, oficina, timestamp, notas en orden cronológico
**Endpoint:** GET /api/envios/{id}/historial

## HU-06 — Registrar intento fallido de entrega
**Como** repartidor **quiero** registrar cuando no puedo entregar **para** que al 3er intento se active la devolución automática.
- Solo aplica a envíos en EnReparto | Al 3er fallo → EnDevolucion automático
**Endpoint:** POST /api/envios/{id}/intento-fallido

## HU-07 — Filtrar envíos por estado
**Como** coordinador **quiero** ver envíos por estado específico **para** priorizar operaciones.
**Endpoint:** GET /api/envios/estado/{estado}

## HU-08 — Reporte de eficiencia
**Como** gerente **quiero** ver resumen de entregas exitosas/fallidas/en proceso **para** evaluar desempeño.
- Muestra total, entregados, devueltos, en proceso, % de éxito
**Endpoint:** GET /api/envios/reporte

## HU-09 — Eliminar envío mal registrado
**Como** operador **quiero** eliminar un envío erróneo **para** mantener limpia la base de datos.
- Solo si está en estado Registrado
**Endpoint:** DELETE /api/envios/{id}

## HU-10 — Registrar cliente con NIT
**Como** ejecutivo **quiero** registrar clientes con su NIT **para** identificarlos en futuras transacciones.
- Campos requeridos: Nombre, NIT (único)
**Endpoint:** POST /api/clientes

## HU-11 — Listar clientes
**Como** ejecutivo **quiero** ver todos los clientes **para** gestionar cuentas comerciales.
**Endpoint:** GET /api/clientes

## HU-12 — Buscar cliente por NIT
**Como** operador **quiero** buscar un cliente por NIT al registrar un envío **para** autocompletar sus datos.
**Endpoint:** GET /api/clientes/nit/{nit}
