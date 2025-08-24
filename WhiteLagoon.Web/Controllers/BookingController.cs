using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController(IBookingService bookingService, 
        IVillaService villaService,
        IVillaNumberService villaNumberService, 
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment webHostEnvironment) : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdendity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdendity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ApplicationUser user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = villaService.GetVillaById(villaId),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Email = user.Email,
                Name = user.Name,
                Phone = user.PhoneNumber,
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = Constants.StatusPending;
            booking.BookingDate = DateTime.Now;

            
            if (!villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
            {
                TempData["error"] = "Room has been sold out!";

                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaid = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            bookingService.CreateBooking(booking);

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100), // Convert to cents
                    Currency = "php",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description,
                    },
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            bookingService.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = bookingService.GetBookingById(bookingId);

            if (bookingFromDb.Status == Constants.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    bookingService.UpdateStatus(bookingFromDb.Id, Constants.StatusApproved, 0);
                    bookingService.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = bookingService.GetBookingById(bookingId);

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == Constants.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = villaNumberService.GetAllVillaNumbers().Where(v =>
                    v.VillaId == bookingFromDb.VillaId &&
                    availableVillaNumber.Any(a => a == v.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {
            string basePath = webHostEnvironment.WebRootPath;

            WordDocument document = new WordDocument();

            //Loading the Template
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            //update template
            var bookingFromDb = bookingService.GetBookingById(id);
            TextSelection textSelection = document.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;

            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Phone;

            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();
            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();
            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();
            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c");
            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID - " + bookingFromDb.Id.ToString();
            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString();

            WTable table = new(document);

            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingFromDb.VillaNumber > 0 ? 3 : 2;

            table.ResetCells(rows, 4);

            WTableRow row0 = table.Rows[0];
            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAL");
            row0.Cells[3].Width = 80;


            WTableRow row1 = table.Rows[1];
            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingFromDb.TotalCost / bookingFromDb.Nights).ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;

            if (bookingFromDb.VillaNumber > 0)
            {
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;

            table.ApplyStyle("CustomStyle");

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            document.Replace("<ADDTABLEHERE>",bodyPart,false,false);

            using DocIORenderer renderer = new();
            MemoryStream stream = new();

            if (downloadType == "word")
            {
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = renderer.ConvertToPDF(document);
                pdfDocument.Save(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", "BookingDetails.pdf");
            }

        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            bookingService.UpdateStatus(booking.Id, Constants.StatusCheckedIn, booking.VillaNumber);
            TempData["Success"] = "Booking Update Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            bookingService.UpdateStatus(booking.Id, Constants.StatusCompleted, booking.VillaNumber);
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            bookingService.UpdateStatus(booking.Id, Constants.StatusCancelled, booking.VillaNumber);
            TempData["Success"] = "Booking Cancelled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = villaNumberService.GetAllVillaNumbers().Where(v => v.VillaId == villaId);
            var checkedInVilla = bookingService.GetCheckedInVillaNumbers(villaId);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;

            string userId = "";

            if (string.IsNullOrEmpty(status))
            {
                status = "";
            }

            if (!User.IsInRole(Constants.Role_Admin))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            }

            bookings = bookingService.GetAllBookings(userId, status);

            return Json(new { data = bookings });
        }

        #endregion
    }
}
