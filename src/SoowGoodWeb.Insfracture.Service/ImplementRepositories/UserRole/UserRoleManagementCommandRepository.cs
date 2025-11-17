using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SoowGood.Core.Service;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Domain.Service.Models.UserRole;
using SoowGoodWeb.Domain.Service.Repositories.RestApiCallService;
using SoowGoodWeb.Domain.Service.Repositories.UserRole;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.UserRole
{
    public class UserRoleManagementCommandRepository : IUserRoleManagementCommandRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        
        public UserRoleManagementCommandRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
         
        }

        public Task<Response<bool>> Delete(UserRoleInsertDto entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<bool>> Insert(UserRoleInsertDto entity)
        {
            var response = new Response<bool>();

            try
            {
                await _dataAccess.SaveDataUsingProcedure<UserRoleInsertDto>("AbpUserRole_insert", entity);
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




        public Task<Response<bool>> Update(UserRoleInsertDto entity)
        {
            throw new NotImplementedException();
        }
    }
}
