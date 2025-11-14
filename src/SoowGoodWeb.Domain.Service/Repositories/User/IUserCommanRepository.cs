using Nancy;
using SignalRTieredDemo.Users;

using SoowGood.Domain.Service.Models.UserInfo;
using SoowGood.Domain.Service.Repositories.BaseInterface;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Repositories.User
{
    public interface IUserCommanRepository : IBaseCommandRepository<UserInsertDto>
    {
        Task<Response<int>> InsertOtp(InsertOtpDto otp);
        Task<Response<int>> UpdateUserPassword(UserPasswordUpdateDto user);
    }
}
