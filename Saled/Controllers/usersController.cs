using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public IActionResult Create(User u)
        {
            _userService.Add(u);
            return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User u)
        {
            if (id != u.Id)
                return BadRequest();

            var existing = _userService.Get(id);
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
