using SoowGood.Domain.Service.Repositories.OtpService;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;

namespace SoowGood.Insfracture.Service.ImplementRepositories.OtpService
{
    public class OtpUserQueryService : IOtpUserQueryService
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public OtpUserQueryService(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }
        public async Task<Response<Otp>> ValidateOtp(string mobileNo, int otp)
        {
            var response = new Response<Otp>();
            try
            {
                var result = await _dataAccess.LoadSingleDataUsingProcedure<Otp, dynamic>("sgOtp_GetOtpByMobileNoAndOtpNo", new
                {
                    mobileNo = mobileNo,
                    otp = otp
                });

                response.Result = result;
            }
            catch (Exception ex)
            {
                response = new Response<Otp>();
            }

            return response;
        }
    }
}
