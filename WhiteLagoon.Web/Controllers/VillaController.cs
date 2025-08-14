using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController(ApplicationDbContext context) : Controller
    {
        public IActionResult Index()
        {
            var villas = context.Villas.ToList();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("name", "The description cannot exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                context.Villas.Add(villa);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(villa);
        }
    }
}
