using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class AmenityService(IUnitOfWork unitOfWork) : IAmenityService
    {
        public void CreateAmenity(Amenity amenity)
        {
            unitOfWork.Amenity.Add(amenity);
            unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {
                var amenityFromDb = unitOfWork.Amenity.Get(a => a.Id == id);
                if (amenityFromDb is not null)
                {
                    unitOfWork.Amenity.Remove(amenityFromDb);
                    unitOfWork.Save();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public IEnumerable<Amenity> GetAllAmenities()
        {
            return unitOfWork.Amenity.GetAll(includeProperties: "Villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return unitOfWork.Amenity.Get(a => a.Id == id, includeProperties: "Villa");
        }

        public void UpdateAmenity(Amenity amenity)
        {
            unitOfWork.Amenity.Update(amenity);
            unitOfWork.Save();
        }
    }
}
