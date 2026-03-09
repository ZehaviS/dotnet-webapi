using Models;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class ActiveUserService : IActiveUser
    {
        public User ActiveUser { get; private set; }

        public ActiveUserService(IHttpContextAccessor context)
        {
            var userId = context?.HttpContext?.User?.FindFirst("Id");
            if (userId != null)
            {
                ActiveUser = new User
                {
                    Id = int.Parse(userId.Value),
                    Name = context.HttpContext.User.FindFirst("username")?.Value ?? "",
                };
            }
        }
    }
}
