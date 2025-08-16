using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController(IUnitOfWork unitOfWork) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            var villaNumberVM = new VillaNumberVM()
            {
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
            bool roomNumberExists = unitOfWork.VillaNumber.Any(v => v.Villa_Number == villa.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                unitOfWork.VillaNumber.Add(villa.VillaNumber);
                unitOfWork.Save();
                TempData["success"] = "The Villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (roomNumberExists)
            {
                TempData["error"] = "The Villa Number already exists.";
            }

            villa.VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = unitOfWork.VillaNumber.Get(v => v.Villa_Number == villaNumberId)
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
                unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                unitOfWork.Save();
                TempData["success"] = "The Villa Number has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = unitOfWork.VillaNumber.Get(v => v.Villa_Number == villaNumberId)
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
            var villaFromDb = unitOfWork.VillaNumber.Get(v => v.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (villaFromDb is not null)
            {
                unitOfWork.VillaNumber.Remove(villaFromDb);
                unitOfWork.Save();
                TempData["success"] = "The Villa Number has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Villa Number could not be deleted.";
            return View();
        }
    }
}
