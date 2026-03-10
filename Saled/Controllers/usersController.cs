using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Models;
using Services;
using System.Linq;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "AllUsers")]

    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISaledsService _saledsService;
        private readonly IActiveUser _activeUser;

        public UsersController(IUserService userService, ISaledsService saledsService, IActiveUser activeUser)
        {
            _userService = userService;
            _saledsService = saledsService;
            _activeUser = activeUser;
        }

        // GET /users - Admin only
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public ActionResult<List<User>> GetAll() =>
            _userService.GetAll();

        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public ActionResult<String> Login([FromBody] User User)
        {
            var existing = _userService.GetAll().FirstOrDefault(u => u.Name == User.Name);
            if (existing == null)
            {
                return Unauthorized();
            }

            // קבע את סוג המשתמש לפי הרשאות (למשל, משתמש עם Id=1 הוא Admin)
            string userType = existing.Id == 1 ? "Admin" : "User";
            var claims = new List<Claim>
            {
                new Claim("Id", existing.Id.ToString()),
                new Claim("username", existing.Name),
                new Claim("type", userType),
            };

            var token = TokenService.GetToken(claims);
            return new OkObjectResult(TokenService.WriteToken(token));
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = "Admin")]
        public IActionResult GenerateBadge([FromBody] User user)
        {
            var claims = new List<Claim>
            {
                new Claim("username", user.Name),
                new Claim("type", "Agent"),
                new Claim("ClearanceLevel", user.ClearanceLevel.ToString()),
            };

            var token = TokenService.GetToken(claims);

            return new OkObjectResult(TokenService.WriteToken(token));
        }

        // POST /users - Admin only
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Create(User u)
        {
            _userService.Add(u);
            return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
        }

        // PUT /users/{id} - Admin can edit anyone, User can edit only self
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(string id, User u)
        {
            int userId = int.Parse(id);
            var existing = _userService.Get(userId);
            if (existing == null)
                return NotFound();

            var currentUser = _activeUser.ActiveUser;
            bool isAdmin = User.HasClaim("type", "Admin");
            if (!isAdmin && currentUser.Id != userId)
                return Forbid();

            _userService.Update(u);
            return NoContent();
        }

        // DELETE /users/{id} - Admin only, also deletes user's items
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            // מחיקת כל הפריטים של המשתמש
            var userSaleds = _saledsService.GetAll().Where(s => s.UserId == id).ToList();
            foreach (var saled in userSaleds)
                _saledsService.Delete(saled.Id);

            _userService.Delete(id);
            return Content(_userService.Count.ToString());
        }
            // GET /users/me - פרטי המשתמש המחובר
            [HttpGet("me")]
            [Authorize]
            public ActionResult<User> Me()
            {
                var currentUser = _activeUser.ActiveUser;
                if (currentUser == null)
                    return Unauthorized();
                return currentUser;
            }
    }
}
