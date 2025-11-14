using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.AppointmentDto;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using SoowGoodWeb.Domain.Service.Repositories.Appointment;
using SoowGoodWeb.Domain.Service.Repositories.DoctorProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Application.Service.Services.Appointment
{
    public class AppointmentService
    {
        private readonly IAppointmentQueryRepository _appointmentQueryRepository;

        public AppointmentService(IAppointmentQueryRepository appointmentQueryRepository)
        {
            _appointmentQueryRepository = appointmentQueryRepository;
        }

        public async Task<Response<List<AppointmentPatientDto>>> GetDoctorPatientProfileList(
            long doctorProfileId,
            int? appointmentStatus,
            string searchTerm,
            int? pageNumber ,
            int? pageSize )
        {
            var response = new Response<List<AppointmentPatientDto>>();
            try
            {
                var result = await _appointmentQueryRepository.GetDoctorPatientProfileId(
                    doctorProfileId,
                    searchTerm
                );

                response.Result = result;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
            }

            return response;
        }
    }
}
