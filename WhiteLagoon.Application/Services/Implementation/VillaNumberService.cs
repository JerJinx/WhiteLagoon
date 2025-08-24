using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaNumberService(IUnitOfWork unitOfWork) : IVillaNumberService
    {
        public bool CheckVillaNumberExists(int villaNumber)
        {
            return unitOfWork.VillaNumber.Any(v => v.Villa_Number == villaNumber);
        }

        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            unitOfWork.VillaNumber.Add(villaNumber);
            unitOfWork.Save();
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                var villaNumberFromDb = unitOfWork.VillaNumber.Get(v => v.Villa_Number == id);

                if (villaNumberFromDb is not null)
                {
                    unitOfWork.VillaNumber.Remove(villaNumberFromDb);
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

        public IEnumerable<VillaNumber> GetAllVillaNumbers()
        {
            return unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
        }

        public VillaNumber GetVillaNumberById(int id)
        {
            return unitOfWork.VillaNumber.Get(v => v.Villa_Number == id, includeProperties: "Villa");
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            unitOfWork.VillaNumber.Update(villaNumber);
            unitOfWork.Save();
        }
    }
}
