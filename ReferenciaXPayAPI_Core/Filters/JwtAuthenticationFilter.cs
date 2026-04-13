using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ReferenciaXPayAPI_Core.Services;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ReferenciaXPayAPI_Core.Filters
{
    public class JwtAuthenticationFilter : IActionFilter
    {
        private readonly IJwtService _jwtService;

        public JwtAuthenticationFilter(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Skip authentication for AuthController and Swagger
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            if (controllerName == "Auth" || controllerName == "Swagger")
            {
                return;
            }

            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedObjectResult(new 
                { 
                    code = "401", 
                    message = "Token Bearer es obligatorio" 
                });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = _jwtService.ValidateToken(token);

            if (principal == null)
            {
                context.Result = new UnauthorizedObjectResult(new 
                { 
                    code = "401", 
                    message = "Token inválido o expirado" 
                });
                return;
            }

            // Agregar el usuario al contexto para uso en los controladores
            context.HttpContext.Items["User"] = principal;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No implementation needed
        }
    }

    public class RoleAuthorizationFilter : IActionFilter
    {
        private readonly int[] _allowedRoles;

        public RoleAuthorizationFilter(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var principal = context.HttpContext.Items["User"] as System.Security.Claims.ClaimsPrincipal;
            
            if (principal == null)
            {
                context.Result = new UnauthorizedObjectResult(new 
                { 
                    code = "401", 
                    message = "Usuario no autenticado" 
                });
                return;
            }

            var roleClaim = principal.FindFirst("RolXPayId")?.Value;
            if (string.IsNullOrEmpty(roleClaim) || !_allowedRoles.Contains(int.Parse(roleClaim)))
            {
                context.Result = new StatusCodeResult(403);
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No implementation needed
        }
    }

    // Attributes para uso directo en controladores
    public class RequireAuthenticationAttribute : TypeFilterAttribute
    {
        public RequireAuthenticationAttribute() : base(typeof(JwtAuthenticationFilter))
        {
        }
    }

    public class RequireRoleAttribute : TypeFilterAttribute
    {
        public RequireRoleAttribute(params int[] roles) : base(typeof(RoleAuthorizationFilter))
        {
            Arguments = new object[] { roles };
        }
    }
}
