using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoowGoodWeb.Application.Service.Services.UserRole;

namespace SoowGoodWeb.Controllers.UserRole
{
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly UserRoleService _userRoleService;
        public UserRoleController(UserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

    }
}
