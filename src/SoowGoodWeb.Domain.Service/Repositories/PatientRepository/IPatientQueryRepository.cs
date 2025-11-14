using Nancy;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.PatientRepository
{
    public interface IPatientQueryRepository
    {
        Task<Response<List<PatientReturnDto>>> GetPatientsBySearchByMobileNo(string? mobileNo = null, string? creatorEntityId = null);
    }
}
