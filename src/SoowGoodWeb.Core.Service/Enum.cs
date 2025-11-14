using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Core.Service
{
    public enum UserRetrievalStatus
    {
        Success,
        Unauthorized,
        NotFound,
        Inactive,
        TokenInvalid,
        Error
    }
    public enum OtpType
    {
        Signup = 1,
        Login = 2,
        ForgotPassword = 3,
    }
    public enum UserLoginType
    { 
        Patient = 1,
        Doctor = 2,
        Agent = 3,
        Admin = 4
    }
}
