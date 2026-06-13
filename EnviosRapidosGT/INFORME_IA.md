# Informe de Utilización de IA
**Alumno:** Daniel Alexander Ortiz Cabrera | **Carné:** 0907-23-14065-1
**Herramienta:** Claude (Anthropic) | **Fecha:** 13/Jun/2026

## Prompts enviados

### Prompt 1
**Enviado:** Imagen del examen con "Resolver usar el lenguaje más fácil"
**Respuesta:** Claude analizó el examen, identificó los 7 requisitos de negocio y propuso C# (.NET 8) + SQLite + Dapper + xUnit. Generó la estructura completa del proyecto.
**Reflexión:** La estructura propuesta (Minimal API, separación en Services/Endpoints/Models) es el mismo patrón de ReservaCancha, lo que permite validarla fácilmente.

### Prompt 2
**Enviado:** "Dame lo en zip para ejecutarlo"
**Respuesta:** Claude generó todos los archivos del proyecto. Sin embargo, los archivos tenían referencias a tipos del nombre incorrecto (RegistrarEnvioDto, HistorialEstado) mezclados de una versión anterior.

### Prompt 3
**Enviado:** Error de compilación con CS0246 sobre tipos no encontrados
**Respuesta:** Claude identificó el problema (archivos mezclados), regeneró todo desde cero con nombres consistentes.

## Correcciones realizadas

| Área | Error | Corrección |
|------|-------|------------|
| Nombres de tipos | `RegistrarEnvioDto`, `ActualizarEstadoDto` no existían | Renombrado a `RegistrarEnvioRequest`, `ActualizarEstadoRequest` |
| Clase Database | `Database.cs` con método `GetConnection()` distinto | Reemplazado por `AppDb.cs` con método `Conn()` |
| Tests integración | SQLite :memory: se cerraba entre tests | Conexión compartida con `cache=shared&mode=memory` |

## Reflexión
La IA aceleró la generación del código base pero requirió supervisión activa: los errores de compilación surgieron por inconsistencia entre iteraciones. El conocimiento previo del stack (.NET + SQLite) permitió identificar y corregir los problemas con criterio propio.

**Estimado generado por IA:** 75% | **Requirió ajuste manual:** 35% del total
