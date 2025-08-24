using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = Constants.Role_Admin)]
    public class AmenityController(IAmenityService amenityService, IVillaService villaService) : Controller
    {
        public IActionResult Index()
        {
            var amenities = amenityService.GetAllAmenities();
            return View(amenities);
        }
        public IActionResult Create()
        {
            var AmenityVM = new AmenityVM()
            {
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
                amenityService.CreateAmenity(villa.Amenity);
                TempData["success"] = "The Amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            villa.VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = amenityService.GetAmenityById(amenityId)
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
                amenityService.UpdateAmenity(amenityVM.Amenity);
                TempData["success"] = "The Amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
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
                VillaList = villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = amenityService.GetAmenityById(amenityId)
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
            var villaFromDb = amenityService.GetAmenityById(amenityVM.Amenity.Id);
            if (villaFromDb is not null)
            {
                var isDeleted = amenityService.DeleteAmenity(villaFromDb.Id);

                if (isDeleted)
                {
                    TempData["success"] = "The Amenity has been deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "The Amenity could not be deleted.";
            return View();
        }
    }
}
