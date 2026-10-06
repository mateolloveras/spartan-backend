# Contrato HTTP — Setup (T-001)

**Base URL:** `http://localhost:5000/api` (desarrollo)

Todos los endpoints se escriben relativos a esta base. La ruta completa de cada endpoint es `/api/{path}`.

---

## Health — Endpoint de infraestructura

Verifica que el pipeline HTTP y el proceso ASP.NET responden correctamente. No toca la base de datos.

- **Método:** `GET`
- **Ruta:** `/health` (completa: `GET /api/health`)
- **Autenticación:** Público, sin token
- **Request:** Sin parámetros ni body.

### Respuesta 200 OK

```json
{
  "status": "healthy"
}
```

| campo | tipo | nuleable | notas |
|---|---|---|---|
| `status` | string | no | Siempre `"healthy"`. |

---

## Formato de errores — Transversal

Los errores se devuelven con `Content-Type: application/problem+json` y código de estado HTTP.

### Campos garantizados

| campo | tipo | nuleable | notas |
|---|---|---|---|
| `status` | integer | no | Código HTTP: 400, 401, 403, 404, 409, 500. |
| `title` | string | no | Resumen en español. |
| `detail` | string | no | Detalle adicional en español. |
| `type` | string | sí | URI de tipo de problema (agregado por ASP.NET en algunos casos). El front no debe depender de este campo. |
| `trace_id` | string | sí | Identificador de trazabilidad (agregado por ASP.NET). El front no debe depender de este campo. |
| `errors` | object | sí | Solo en 400 de validación: `{ "campo_snake_case": ["mensaje", ...] }` (propiedades de primer nivel). |

### Status 400 — Datos inválidos

**Cuándo:** Falla de validación de FluentValidation (formato incorrecto del request) o `ValidationException` de la lógica de dominio.

#### Variante: FluentValidation (con `errors`)

```json
{
  "title": "Datos inválidos",
  "status": 400,
  "detail": "Uno o más campos no son válidos.",
  "errors": {
    "page_size": ["El tamaño de página debe estar entre 1 y 100."],
    "user_id": ["El ID de usuario no puede estar vacío.", "El ID debe ser un UUID válido."]
  }
}
```

Las claves de `errors` son snake_case y corresponden a las propiedades del request. Los mensajes están en español.

#### Variante: ValidationException de dominio (sin `errors`)

```json
{
  "title": "Datos inválidos",
  "status": 400,
  "detail": "El usuario ya tiene una suscripción activa."
}
```

#### Variante: JSON malformado o binding incorrecto (sin `errors` garantizados)

**Nota:** ASP.NET genera su propio 400 cuando el JSON está malformado o los tipos no coinciden. Este formato **no está customizado en T-001** y puede variar. Ejemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { ... }
}
```

Los mensajes están en inglés y no tienen garantía de estructura. El frontend debe tratar todos los 400 con `errors` como "al menos un campo es inválido" y mostrar un mensaje genérico.

---

### Status 401 — No autorizado

**Cuándo:** Sin token, token vencido, o la operación lanza `UnauthorizedException` de dominio.

```json
{
  "title": "No autorizado",
  "status": 401,
  "detail": "Se requiere autenticación válida."
}
```

O, si la excepción de dominio proporciona un mensaje:

```json
{
  "title": "No autorizado",
  "status": 401,
  "detail": "El código de acceso ha expirado."
}
```

---

### Status 403 — Acceso denegado

**Cuándo:** Token válido pero rol insuficiente para acceder al recurso.

```json
{
  "title": "Acceso denegado",
  "status": 403,
  "detail": "No tiene permisos para realizar esta operación."
}
```

---

### Status 404 — Recurso no encontrado

**Cuándo:** 
- La operación lanza `NotFoundException` de dominio (recurso esperado no existe).
- La ruta HTTP no existe.

```json
{
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "El usuario solicitado no existe."
}
```

O, si es una ruta inexistente:

```json
{
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "El recurso solicitado no existe."
}
```

---

### Status 409 — Regla de negocio violada

**Cuándo:** La operación lanza `DomainException` de dominio (alguna regla de negocio se incumple).

```json
{
  "title": "Regla de negocio violada",
  "status": 409,
  "detail": "El usuario ya tiene una membresía activa. No se puede crear otra hasta cancelar la actual."
}
```

---

### Status 500 — Error interno

**Cuándo:** Excepción no esperada en el servidor.

```json
{
  "title": "Error interno",
  "status": 500,
  "detail": "Ocurrió un error inesperado. Intente nuevamente más tarde.",
  "trace_id": "0HN5J2C5K3P8M:00000001"
}
```

El campo `detail` **nunca** expone detalles internos de la excepción (stack trace, nombres de métodos, etc.). El `trace_id` permite correlacionar con logs del servidor.

---

### Status 405 — Método no permitido

**Cuándo:** Se usa un verbo HTTP incorrecto en una ruta (p. ej., POST a un endpoint que solo acepta GET).

```json
{
  "status": 405,
  "title": "Method Not Allowed",
  "trace_id": "0HN5J2C5K3P8M:00000002"
}
```

**Nota:** El `title` está en inglés (generado por ASP.NET). Los campos `detail` y `errors` **no aparecen**. El `trace_id` puede estar presente.

---

### Status 415 — Unsupported Media Type

**Cuándo:** El `Content-Type` del request no es soportado (p. ej., se envía XML en vez de JSON).

```json
{
  "status": 415,
  "title": "Unsupported Media Type",
  "trace_id": "0HN5J2C5K3P8M:00000003"
}
```

**Nota:** Al igual que 405, el `title` está en inglés y no hay `detail` ni `errors`.

---

## Reglas de serialización

- **PropertyNamingPolicy:** `snake_case` (propiedades del JSON).
- **Enums:** Convertidos a string en `snake_case` (p. ej., `NoShow` → `"no_show"`).
- **DateTimeOffset UTC:** Formato ISO 8601 con zona horaria `+00:00` (p. ej., `"2026-01-15T10:30:00+00:00"`). No usa la letra `Z`.
- **PagedResult:** Los listados paginados serializan como:
  ```json
  {
    "items": [ ... ],
    "page": 1,
    "page_size": 20,
    "total_count": 150
  }
  ```

---

## Notas para el frontend

1. **Campos extra:** `type` y `trace_id` pueden no estar presentes. El frontend no debe depender de ellos para lógica o UX.
2. **Múltiples errores:** Un 400 de FluentValidation puede tener varias reglas para el mismo campo:
   ```json
   {
     "errors": {
       "email": [
         "El correo es obligatorio.",
         "El correo debe ser válido."
       ]
     }
   }
   ```
   Se recomienda mostrar todos los mensajes o solo el primero, según la UX.
3. **CORS:** El frontend en `http://localhost:4200` puede hacer requests. El header `Access-Control-Allow-Origin` está presente en todas las respuestas.
4. **Timeout y reintentos:** No hay endpoint de salud pública aparte de `GET /api/health`. Úsalo para sondear disponibilidad del servidor.
