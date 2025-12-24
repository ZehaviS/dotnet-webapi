using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Microsoft.AspNetCore.Authorization;
namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SaledsController : ControllerBase
    {
        private readonly ISaledsService _service;

        public SaledsController(ISaledsService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<List<Saleds>> GetAll() =>
            _service.GetAll();

        [HttpGet("{id}")]
                [Authorize(Policy = "Admin")]

        public ActionResult<Saleds> Get(int id)
        {
            var saled = _service.Get(id);
            if (saled == null)
                return NotFound();

            return saled;
        }

        [HttpPost]
                [Authorize(Policy = "Admin")]

        public IActionResult Create(Saleds s)
        {
            _service.Add(s);
            return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
        }


        [HttpPut("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Update(int id, [FromBody]Saleds s)
        {
            if (id != s.Id)
                return BadRequest();

            var existing = _service.Get(id);
            if (existing == null)
                return NotFound();

            _service.Update(s);
            return NoContent();
        }

        [HttpDelete("{id}")]
                [Authorize(Policy = "Admin")]

        public IActionResult Delete(int id)
        {
            var saled = _service.Get(id);
            if (saled == null)
                return NotFound();

            _service.Delete(id);
            return Content(_service.Count.ToString());
        }
    }
}
