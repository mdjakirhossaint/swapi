using SignalRTieredDemo.Users;
using SoowGood.Core.Service;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGood.Domain.Service.Repositories.User;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;
using static System.Net.WebRequestMethods;

namespace SoowGood.Insfracture.Service.ImplementRepositories.User
{
    public class UserCommanRepository : IUserCommanRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public UserCommanRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
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
    }
}
