using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class VillaController(IVillaService villaService) : Controller
    {
        public IActionResult Index()
        {
            var villas = villaService.GetAllVillas();
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
                villaService.CreateVilla(villa);
                TempData["success"] = "The Villa has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            var villa = villaService.GetVillaById(villaId);
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
                villaService.UpdateVilla(villa);
                TempData["success"] = "The Villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            var villa = villaService.GetVillaById(villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            bool isDeleted = villaService.DeleteVilla(villa.Id);

            if (isDeleted)
            {
                TempData["success"] = "The Villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "The Villa could not be deleted.";
            }

            return View();
        }
    }
}
