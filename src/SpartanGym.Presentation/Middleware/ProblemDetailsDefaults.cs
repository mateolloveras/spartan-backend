namespace SpartanGym.Presentation.Middleware;

public static class ProblemDetailsDefaults
{
    public static void Customize(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;

        switch (problem.Status)
        {
            case StatusCodes.Status401Unauthorized:
                problem.Title = "No autorizado";
                problem.Detail ??= "Se requiere autenticación válida.";
                break;
            case StatusCodes.Status403Forbidden:
                problem.Title = "Acceso denegado";
                problem.Detail ??= "No tiene permisos para realizar esta operación.";
                break;
            case StatusCodes.Status404NotFound:
                problem.Title = "Recurso no encontrado";
                problem.Detail ??= "El recurso solicitado no existe.";
                break;
            case StatusCodes.Status500InternalServerError:
                // Se sobrescribe siempre para no filtrar detalles internos
                problem.Title = "Error interno";
                problem.Detail = "Ocurrió un error inesperado. Intente nuevamente más tarde.";
                break;
        }
    }
}
