using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AmenityController(IUnitOfWork unitOfWork) : Controller
    {
        public IActionResult Index()
        {
            var amenities = unitOfWork.Amenity.GetAll(includeProperties: "Villa");
            return View(amenities);
        }
        public IActionResult Create()
        {
            var AmenityVM = new AmenityVM()
            {
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM villa)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Amenity.Add(villa.Amenity);
                unitOfWork.Save();
                TempData["success"] = "The Amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            villa.VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(villa);
        }

        public IActionResult Update(int amenityId)
        {
            var AmenityVM = new AmenityVM()
            {
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = unitOfWork.Amenity.Get(v => v.Id == amenityId)
            };
            if (AmenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Update(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Amenity.Update(amenityVM.Amenity);
                unitOfWork.Save();
                TempData["success"] = "The Amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(amenityVM);
        }

        public IActionResult Delete(int amenityId)
        {
            var AmenityVM = new AmenityVM()
            {
                VillaList = unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = unitOfWork.Amenity.Get(v => v.Id == amenityId)
            };
            if (AmenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            var villaFromDb = unitOfWork.Amenity.Get(v => v.Id == amenityVM.Amenity.Id);
            if (villaFromDb is not null)
            {
                unitOfWork.Amenity.Remove(villaFromDb);
                unitOfWork.Save();
                TempData["success"] = "The Amenity has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Amenity could not be deleted.";
            return View();
        }
    }
}
