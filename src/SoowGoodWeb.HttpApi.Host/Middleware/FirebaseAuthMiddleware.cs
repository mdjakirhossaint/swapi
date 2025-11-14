using Microsoft.AspNetCore.Http;
using SoowGoodWeb.Services;
using System.Threading.Tasks;

namespace SoowGoodWeb.Middleware
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FirebaseAuthService _authService;

        public FirebaseAuthMiddleware(RequestDelegate next, FirebaseAuthService authService)
        {
            _next = next;
            _authService = authService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var decoded = await _authService.VerifyTokenAsync(token);

                if (decoded == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid Firebase token");
                    return;
                }

                // Attach user to context
                context.Items["FirebaseUser"] = decoded;
            }

            await _next(context);
        }
    }
}
