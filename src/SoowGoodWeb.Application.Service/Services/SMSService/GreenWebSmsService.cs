using SoowGood.Core.Service.Utility;
using SoowGood.Domain.Service.Repositories.OtpService;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;

namespace SoowGoodWeb.Application.Service.Services.SMSService
{
    public class GreenWebSmsService
    {
        private readonly ISmsService _smsService;
        private readonly IOtpUserQueryService _otpService;
        public GreenWebSmsService(ISmsService smsService,
            IOtpUserQueryService otpService)
        {
            _smsService = smsService;
            _otpService = otpService;
        }
        public async Task<Response<OtpResultDto>> SendSMS(string clientKey, string mobileNo)
        {
            var response = new Response<OtpResultDto>();
            if (!string.IsNullOrEmpty(clientKey) && clientKey.Equals("SoowGood_App", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(mobileNo))
            {
                int otp = SharedService.GetRandomNo(1000, 9999);
                Otp otpEntity = new Otp();
                otpEntity.OtpNo = otp;
                otpEntity.MobileNo = mobileNo;
                otpEntity.ExpireDateTime = DateTime.Now.AddMinutes(3);
                otpEntity.OtpStatus = OtpStatus.New;
                // stp start

                var message = " is your OTP to authenticate your Phone No. Do not share this OTP with anyone.";

                SmsRequestParamDto otpInput = new SmsRequestParamDto();
                otpInput.Sms = String.Format(otp + message);
                otpInput.Msisdn = mobileNo;
                otpInput.CsmsId = GenerateTransactionId(16);
                try
                {
                    var res = await _smsService.SendSmsGreenWeb(otpInput);
                    var result = new OtpResultDto
                    { 
                        OtpSent = true,
                        otp = otp
                    };
                    response.Result = result;

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            return response;
        }
        public async Task<Response<OtpResultDto>> SendSMS(string clientKey, string mobileNo, string? message = null)
        {
            var response = new Response<OtpResultDto>();
            if (!string.IsNullOrEmpty(clientKey) && clientKey.Equals("SoowGood_App", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(mobileNo))
            {
                // stp start
                message = message ?? " Please add a message";
                SmsRequestParamDto otpInput = new SmsRequestParamDto();
                otpInput.Sms = String.Format(message);
                otpInput.Msisdn = mobileNo;
                otpInput.CsmsId = GenerateTransactionId(16);
                try
                {
                    var res = await _smsService.SendSmsGreenWeb(otpInput);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            return response;
        }

        private static string GenerateTransactionId(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<Response<bool>> ValidateOtp(string mobileNo, int otp)
        { 
            var response = new Response<bool>();

            var getOtpInfo = await _otpService.ValidateOtp(mobileNo, otp);

            if (getOtpInfo.Result == null)
            {
                response.Result = false;
                response.IsSuccess = false;
                response.StatusCode = 400;
                response.Message = "Otp cannot verify";
                return response;
            }

            response.IsSuccess = true;
            response.Result = true;
            response.Message = "Verify Successfull";
            response.StatusCode = 200;
            return response;
        }
    }
}
