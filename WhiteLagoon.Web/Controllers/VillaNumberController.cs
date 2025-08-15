using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController(ApplicationDbContext context) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = context.VillaNumbers.Include(v => v.Villa).ToList();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            var villaNumberVM = new VillaNumberVM()
            {
                VillaList = context.Villas.ToList().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM villa)
        {
            bool roomNumberExists = context.VillaNumbers.Any(v => v.Villa_Number == villa.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                context.VillaNumbers.Add(villa.VillaNumber);
                context.SaveChanges();
                TempData["success"] = "The Villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (roomNumberExists)
            {
                TempData["error"] = "The Villa Number already exists.";
            }

            villa.VillaList = context.Villas.ToList().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(villa);
        }

        public IActionResult Update(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM()
            {
                VillaList = context.Villas.ToList().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == villaNumberId)
            };
            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                context.VillaNumbers.Update(villaNumberVM.VillaNumber);
                context.SaveChanges();
                TempData["success"] = "The Villa Number has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = context.Villas.ToList().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM()
            {
                VillaList = context.Villas.ToList().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == villaNumberId)
            };
            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            var villaFromDb = context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (villaFromDb is not null)
            {
                context.VillaNumbers.Remove(villaFromDb);
                context.SaveChanges();
                TempData["success"] = "The Villa Number has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Villa Number could not be deleted.";
            return View();
        }
    }
}
