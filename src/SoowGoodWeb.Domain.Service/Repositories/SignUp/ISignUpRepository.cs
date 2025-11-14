using Nancy;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.SignUp
{
    public interface ISignUpRepository
    {
        //Task<bool> IsEmailExist(string email);
        //Task<bool> IsPhoneNoExist(string phoneNo);
        //Task<bool> IsUserNameExist(string userName);
        //Task<bool> IsUserNameExist(string userName, int id);
        //Task<bool> IsEmailExist(string email, int id);
        //Task<bool> IsPhoneNoExist(string phoneNo, int id);

        Task<Response<UserSignUpResponseDto>> SignUpUser(UserSingupRequestDto userSingupRequestDto);
    }
}
