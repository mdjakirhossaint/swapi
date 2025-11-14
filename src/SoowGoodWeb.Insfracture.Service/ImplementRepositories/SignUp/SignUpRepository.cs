
using Microsoft.Extensions.Configuration;
using RestSharp;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Repositories.RestApiCallService;
using SoowGoodWeb.Domain.Service.Repositories.SignUp;
using SoowGoodWeb.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.SignUp
{
    public class SignUpRepository : ISignUpRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IBaseRestClientApiService _baseRestClientApiService;
        public SignUpRepository(IConfiguration configuration, IBaseRestClientApiService baseRestClientApiService)
        {
            _configuration = configuration;
            _baseRestClientApiService = baseRestClientApiService;
        }

        public async Task<Response<UserSignUpResponseDto>> SignUpUser(UserSingupRequestDto userSingupRequestDto)
        {
            var response = new Response<UserSignUpResponseDto>();
            try
            {
                var baseUrl = _configuration.GetSection("GeneralSettings:ApiBaseURLAuthenticaton").Value;
                var endPoint = "api/app/user-manage-accounts/signup-user";
                // Add Authorization header
                string token = _configuration.GetSection("Settings:PublicApiKey").Value;
                var responseJson = await _baseRestClientApiService.MakeApiCall(baseUrl, endPoint, Method.Post, userSingupRequestDto, token, 3, 1000);
                var deSerializedResults = JsonHelper.DeserializeJsonToSignle<UserSignUpResponseDto>(responseJson);
                if (deSerializedResults != null)
                {
                    response.Result = deSerializedResults;
                    response.IsSuccess = true;
                }
                //response.Message = responseJson.
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
