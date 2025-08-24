using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController(IVillaNumberService villaNumberService, IVillaService villaService) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = villaNumberService.GetAllVillaNumbers();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            var villaNumberVM = new VillaNumberVM()
            {
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
            bool roomNumberExists = villaNumberService.CheckVillaNumberExists(villa.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                villaNumberService.CreateVillaNumber(villa.VillaNumber);
                TempData["success"] = "The Villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (roomNumberExists)
            {
                TempData["error"] = "The Villa Number already exists.";
            }

            villa.VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = villaNumberService.GetVillaNumberById(villaNumberId)
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
                villaNumberService.UpdateVillaNumber(villaNumberVM.VillaNumber);
                TempData["success"] = "The Villa Number has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = villaNumberService.GetVillaNumberById(villaNumberId)
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
            var villaFromDb = villaNumberService.GetVillaNumberById(villaNumberVM.VillaNumber.Villa_Number);
            if (villaFromDb is not null)
            {
                var isDeleted = villaNumberService.DeleteVillaNumber(villaFromDb.Villa_Number);
                if (isDeleted)
                {
                    TempData["success"] = "The Villa Number has been deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "The Villa Number could not be deleted.";
            return View();
        }
    }
}
