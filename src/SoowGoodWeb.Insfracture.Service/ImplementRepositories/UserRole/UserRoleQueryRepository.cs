using SoowGood.Domain.Service.Models.UserRole;
using SoowGood.Domain.Service.Repositories.UserRole;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;

namespace SoowGood.Insfracture.Service.ImplementRepositories.UserRole
{
    public class UserRoleQueryRepository : IUserRoleQueryRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public UserRoleQueryRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<List<UserRoleResponseDto>> GetAll()
        {
            var response = new List<UserRoleResponseDto>();
            try
            {
                var result = await _dataAccess.LoadDataUsingProcedure<UserRoleResponseDto, dynamic>("AbpUserRole_GetAll", new
                {
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new List<UserRoleResponseDto>();
            }

            return response;
        }

        public async Task<Role> GetRoleByRoleName(string roleName)
        {
            var response = new Role();
            try
            {
                var result = await _dataAccess.LoadSingleDataUsingProcedure<Role, dynamic>("AbpUserRole_getByUserRoleByUserId", new
                {
                    roleName = roleName
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new Role();
            }

            return response;
        }

        public async Task<UserRoleResponseDto> GetUserRoleByUserId(string userId)
        {
            var response = new UserRoleResponseDto();
            try
            {
                var result = await _dataAccess.LoadSingleDataUsingProcedure<UserRoleResponseDto, dynamic>("AbpUserRole_getByUserRoleByUserId", new
                {
                    userId = userId
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new UserRoleResponseDto();
            }

            return response;
        }
    }
}
