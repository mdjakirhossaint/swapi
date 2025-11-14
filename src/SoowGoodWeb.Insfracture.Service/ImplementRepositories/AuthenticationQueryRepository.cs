

using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories
{
    public class AuthenticationQueryRepository : IAuthenticationQueryRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public AuthenticationQueryRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<UserSignInReturnDto> GetUserByUserId(Guid userId)
        {
            var response = new UserSignInReturnDto();
            try
            {

                var result = await _dataAccess.LoadSingleDataUsingProcedure<UserSignInReturnDto, dynamic>("AbpUser_getByUserId", new
                {
                    userId = userId
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new UserSignInReturnDto();
            }

            return response;
        }

        public async Task<UserSignInReturnDto> GetUserByUserName(string username)
        {
            var response = new UserSignInReturnDto();
            try
            {

                var result = await _dataAccess.LoadSingleDataUsingProcedure<UserSignInReturnDto, dynamic>("AbpUser_getByUserName", new
                {
                    username = username
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new UserSignInReturnDto();
            }

            return response;
        }

        public async Task<List<Role>> GetUserRolesByUserId(Guid userId)
        {
            var response = new List<Role>();
            try
            {

                var result = await _dataAccess.LoadDataUsingProcedure<Role, dynamic>("AbpUser_getRoleByUserId", new
                {
                    userId = userId
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new();
            }

            return response;
        }

        public async Task<List<UserDto>> GetUsersByUserRole(string RoleName)
        {
            var response =new List<UserDto>();
            try
            {

                var result = await _dataAccess.LoadDataUsingProcedure<UserDto, dynamic>("AbpUser_GetAllDoctor", new
                {
                    RoleName = RoleName
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new();
            }

            return response;
        }
    }
}
