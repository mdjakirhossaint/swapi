using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SignalRTieredDemo.Users;
using SoowGood.Core.Service;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGood.Domain.Service.Repositories.User;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Domain.Service.Repositories.RestApiCallService;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;
using static System.Net.WebRequestMethods;

namespace SoowGood.Insfracture.Service.ImplementRepositories.User
{
    public class UserCommanRepository : IUserCommanRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;

        private readonly IBaseRestClientApiService _baseRestClientApiService;
        private readonly IConfiguration _configuration;
        private string _apiBaseURL;
        public UserCommanRepository(SqlDataAccessLayer dataAccess, IBaseRestClientApiService baseRestClientApiService, IConfiguration configuration)
        {
            _dataAccess = dataAccess;
            _baseRestClientApiService = baseRestClientApiService;
            _configuration = configuration;
            _apiBaseURL = _configuration.GetSection("GeneralSettings:ApiBaseURLAuthenticaton").Value;
        }

        public Task<Response<bool>> Delete(UserInsertDto entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<bool>> Insert(UserInsertDto entity)
        {
            var response = new Response<bool>();

            try
            {
                await _dataAccess.SaveDataUsingProcedure<UserInsertDto>("AbpUser_Insert", entity);
                response.Result = true;
                response.IsSuccess = true;
                response.Message = StandardDataAccessMessages.SuccessMessaage;

            }
            catch (Exception ex)
            {
                response.Message = StandardDataAccessMessages.GetSqlErrorMessage(ex);
                response.IsSuccess = false;
                response.Result = false;
            }

            return response;
        }

        public async Task<Response<int>> InsertOtp(InsertOtpDto otp)
        {
            var response = new Response<int>();

            try
            {
                await _dataAccess.SaveDataUsingProcedure<InsertOtpDto>("SgOtp_Insert", otp);
                response.Result = 0;
                response.IsSuccess = true;
                response.Message = StandardDataAccessMessages.SuccessMessaage;

            }
            catch (Exception ex)
            {
                response.Message = StandardDataAccessMessages.GetSqlErrorMessage(ex);
                response.IsSuccess = false;
                response.Result = 0;
            }

            return response;
        }

        public async Task<Response<bool>> Update(UserInsertDto entity)
        {
            var response = new Response<bool>();

            try
            {
                await _dataAccess.UpdateDataUsingProcedure<UserInsertDto>("SgOtp_Insert", entity);
                response.Result = true;
                response.IsSuccess = true;
                response.Message = StandardDataAccessMessages.SuccessMessaage;

            }
            catch (Exception ex)
            {
                response.Message = StandardDataAccessMessages.GetSqlErrorMessage(ex);
                response.IsSuccess = false;
                response.Result = true;
            }

            return response;
        }

        public async Task<Response<int>> UpdateUserPassword(UserPasswordUpdateDto user)
        {
            var response = new Response<int>();

            try
            {
                await _dataAccess.UpdateDataUsingProcedure<UserPasswordUpdateDto>("AbpUser_userPassword", user);
                response.Result = 0;
                response.IsSuccess = true;
                response.Message = StandardDataAccessMessages.SuccessMessaage;

            }
            catch (Exception ex)
            {
                response.Message = StandardDataAccessMessages.GetSqlErrorMessage(ex);
                response.IsSuccess = false;
                response.Result = 0;
            }

            return response;
        }



        public async Task<Response<DoctorProfileInputDto>> DoctorProfileInsert(DoctorProfileInputDto requestModel)
        {

            var response = new Response<DoctorProfileInputDto>();
            try
            {
                var baseUrl = _apiBaseURL;
                var endPoint = "/api/app/doctor-profile";
                // Add Authorization header
                string token = _configuration.GetSection("Settings:Token").Value;
                var responseJson = await _baseRestClientApiService.MakeApiCall(baseUrl, endPoint, Method.Post, requestModel, token, 3, 1000);
                var deSerializedJsonResult = JsonConvert.DeserializeObject<JObject>(responseJson.Content);
                int results = deSerializedJsonResult["results"]?.Value<int>() ?? 0;

                var doctordegreeApiResponse = new DoctorProfileInputDto
                {
                    Id = deSerializedJsonResult["data"]?["id"]?.Value<long>() ?? 0,
                };

                if (results != null)
                {
                    response.Result = doctordegreeApiResponse;
                    response.IsSuccess = true;
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }

        }


    }
}
