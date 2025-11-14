using SoowGoodWeb.Domain.Service.Models.AppointmentDto;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.Appointment
{
    public interface IAppointmentQueryRepository
    {
        Task<List<AppointmentPatientDto>> GetDoctorPatientProfileId(
         long profileId,
         string? searchTerm
     );
    }
}
