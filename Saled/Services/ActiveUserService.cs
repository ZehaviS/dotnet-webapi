using Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Services
{
    public class ActiveUserService : IActiveUser
    {
        public User ActiveUser { get; private set; }

        public ActiveUserService(IHttpContextAccessor context)
        {
            var userIdClaim = context?.HttpContext?.User?.FindFirst("Id")
                ?? context?.HttpContext?.User?.FindFirst("id")
                ?? context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var userName = context.HttpContext.User.FindFirst("username")?.Value
                    ?? context.HttpContext.User.FindFirst("name")?.Value
                    ?? context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value
                    ?? $"משתמש {userId}";

                var clearanceString = context.HttpContext.User.FindFirst("ClearanceLevel")?.Value;
                int clearanceLevel = 0;
                if (!string.IsNullOrEmpty(clearanceString))
                    int.TryParse(clearanceString, out clearanceLevel);

                ActiveUser = new User
                {
                    Id = userId,
                    Name = userName,
                    ClearanceLevel = clearanceLevel
                };
            }
        }
    }
}
