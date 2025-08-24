using Microsoft.AspNetCore.Hosting;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : IVillaService
    {
        public void CreateVilla(Villa villa)
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
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                var villaFromDb = unitOfWork.Villa.Get(v => v.Id == id);
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
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
        }

        public Villa GetVillaById(int id)
        {
            return unitOfWork.Villa.Get(v => v.Id == id, includeProperties: "VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
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
            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumberList = unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = unitOfWork.Booking.GetAll(b =>
                    b.Status == Constants.StatusApproved ||
                    b.Status == Constants.StatusCheckedIn)
                .ToList();

            int roomsAvailable = Constants.VillaRoomsAvailableCount(
                villaId,
                villaNumberList,
                checkInDate,
                nights,
                bookedVillas);

            return roomsAvailable > 0;
        }

        public void UpdateVilla(Villa villa)
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
        }
    }
}
