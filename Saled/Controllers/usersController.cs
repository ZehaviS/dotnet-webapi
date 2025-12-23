using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Models;
using Services;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "AllUsers")]

    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
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
            if (User.Name != "אילה")
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim("username", User.Name),
                new Claim("type", "Admin"),
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

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create(User u)
        {
            _userService.Add(u);
            return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, User u)
        {
            if (int.Parse(id) != u.Id)
                return BadRequest();

            var existing = _userService.Get(int.Parse(id));
            if (existing == null)
                return NotFound();

            _userService.Update(u);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            _userService.Delete(id);
            return Content(_userService.Count.ToString());
        }
    }
}
