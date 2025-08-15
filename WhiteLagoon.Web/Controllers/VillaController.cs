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
                TempData["success"] = "The Villa has been created successfully.";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            var villa = context.Villas.FirstOrDefault(v => v.Id == villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid && villa.Id > 0)
            {
                context.Villas.Update(villa);
                context.SaveChanges();
                TempData["success"] = "The Villa has been updated successfully.";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            var villa = context.Villas.FirstOrDefault(v => v.Id == villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaFromDb = context.Villas.FirstOrDefault(v => v.Id == villa.Id);
            if (villaFromDb is not null)
            {
                context.Villas.Remove(villaFromDb);
                context.SaveChanges();
                TempData["success"] = "The Villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "The Villa could not be deleted.";
            return View();
        }
    }
}
