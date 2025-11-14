
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Models;

namespace SoowGood.Domain.Service.Repositories.OtpService
{
    public interface IOtpUserQueryService
    {
        Task<Response<Otp>> ValidateOtp(string mobileNo, int otp);
        
    }
}
