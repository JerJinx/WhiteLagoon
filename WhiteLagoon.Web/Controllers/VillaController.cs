using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : Controller
    {
        public IActionResult Index()
        {
            var villas = unitOfWork.Villa.GetAll();
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
                if (villa.Image is not null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    var imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"images\VillaImage");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

                unitOfWork.Villa.Add(villa);
                unitOfWork.Save();
                TempData["success"] = "The Villa has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            var villa = unitOfWork.Villa.Get(v => v.Id == villaId);
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
                if (villa.Image is not null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    var imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"images\VillaImage");

                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }

                unitOfWork.Villa.Update(villa);
                unitOfWork.Save();
                TempData["success"] = "The Villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            var villa = unitOfWork.Villa.Get(v => v.Id == villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaFromDb = unitOfWork.Villa.Get(v => v.Id == villa.Id);
            if (villaFromDb is not null)
            {
                if (!string.IsNullOrEmpty(villaFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, villaFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                unitOfWork.Villa.Remove(villaFromDb);
                unitOfWork.Save();
                TempData["success"] = "The Villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Villa could not be deleted.";
            return View();
        }
    }
}
