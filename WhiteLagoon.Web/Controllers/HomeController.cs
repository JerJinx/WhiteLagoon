using Microsoft.AspNetCore.Mvc;
using Syncfusion.Drawing;
using Syncfusion.Presentation;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : Controller
    {

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = unitOfWork.Booking.GetAll(b => 
                    b.Status == Constants.StatusApproved || 
                    b.Status == Constants.StatusCheckedIn)
                .ToList();


            foreach (var villa in villaList)
            {
                int roomsAvailable = Constants.VillaRoomsAvailableCount(
                    villa.Id,
                    villaNumberList,
                    checkInDate,
                    nights,
                    bookedVillas);

                villa.IsAvailable = roomsAvailable > 0;
            }

            HomeVM homeVM = new()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate
            };

            return PartialView("_VillaList", homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").FirstOrDefault(v => v.Id == id);
            if (villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);
            
            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaName") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0}", villa.Sqft);
            }
            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size : {0} sqft", villa.Sqft);
            }
            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("PHP {0}/night", villa.Price.ToString("C"));
            }

            shape = slide.Shapes.FirstOrDefault(v => v.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                var listItems = villa.VillaAmenity.Select(v => v.Name).ToList();
                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);
                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(v => v.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;

                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "villa.pptx");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
