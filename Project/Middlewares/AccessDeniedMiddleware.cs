namespace Project.Middlewares
{
    public class AccessDeniedMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessDeniedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 403)
            {
                context.Response.Redirect("/Admin/api/Authorization/AccessDenied");
            }
        }
    }

    public static class AccessDeniedMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccessDeniedMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AccessDeniedMiddleware>();
        }
    }
}
