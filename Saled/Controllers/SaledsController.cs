using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Microsoft.AspNetCore.Authorization;
namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
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
        public ActionResult<Saleds> Get(int id)
        {
            var saled = _service.Get(id);
            if (saled == null)
                return NotFound();

            return saled;
        }

        [HttpPost]
        [Authorize] // כל משתמש מחובר
        public async Task<IActionResult> Create()
        {
            Saleds s = null;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                s = new Saleds
                {
                    Name = form["Name"],
                    weight = double.TryParse(form["weight"], out var w) ? w : 0,
                    ImageUrl = form["ImageUrl"]
                };

                var file = form.Files["imageFile"];
                if (file != null && file.Length > 0)
                {
                    var fileName = $"salad_{DateTime.Now.Ticks}{System.IO.Path.GetExtension(file.FileName)}";
                    var savePath = System.IO.Path.Combine("wwwroot", "img", fileName);
                    using (var stream = System.IO.File.Create(savePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    s.ImageUrl = $"img/{fileName}";
                }
            }
            else
            {
                using var reader = new StreamReader(Request.Body);
                var bodyText = await reader.ReadToEndAsync();
                s = System.Text.Json.JsonSerializer.Deserialize<Saleds>(bodyText);
            }

            if (s == null)
                return BadRequest("Invalid salad data");

            _service.Add(s);
            return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
        }

        [HttpPut("{id}")]
        [Authorize] // כל משתמש מחובר
        public async Task<IActionResult> Update(int id)
        {
            var userIdClaim = User?.FindFirst("Id")?.Value ?? "<none>";
            var userNameClaim = User?.FindFirst("username")?.Value ?? "<none>";
            Console.WriteLine($"SaledsController.Update called for id={id} by user={userIdClaim} ({userNameClaim}), content-type={Request.ContentType}");
            Saleds s = null;
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                Console.WriteLine($"SaledsController.Update: received form keys: {string.Join(",", form.Keys)}");
                s = new Saleds
                {
                    Id = int.Parse(form["id"]),
                    Name = form["Name"],
                    weight = double.Parse(form["weight"]),
                    ImageUrl = form["ImageUrl"]
                };
                var file = form.Files["imageFile"];
                if (file != null && file.Length > 0)
                {
                    var fileName = $"salad_{id}_{DateTime.Now.Ticks}{System.IO.Path.GetExtension(file.FileName)}";
                    var savePath = System.IO.Path.Combine("wwwroot", "img", fileName);
                    using (var stream = System.IO.File.Create(savePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    s.ImageUrl = $"img/{fileName}";
                }
            }
            else
            {
                // Use ASP.NET Core JSON formatter settings (case-insensitive)
                s = await Request.ReadFromJsonAsync<Saleds>();
                if (s == null)
                {
                    Console.WriteLine("SaledsController.Update: failed to deserialize JSON body");
                    return BadRequest("Invalid JSON payload");
                }
            }

            if (s is null)
            {
                Console.WriteLine("SaledsController.Update: deserialized object is null");
                return BadRequest("Unable to parse salad payload");
            }

            if (id != s.Id)
            {
                Console.WriteLine($"SaledsController.Update: path id ({id}) does not match body id ({s.Id})");
                return BadRequest("ID mismatch");
            }

            var existing = _service.Get(id);
            if (existing == null)
            {
                Console.WriteLine($"SaledsController.Update: salad not found id={id}");
                return NotFound();
            }

            // Preserve owner / user id to ensure permission checks in the service work correctly.
            s.UserId = existing.UserId;

            _service.Update(s);
            return NoContent();
        }

        [HttpDelete("{id}")]
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
