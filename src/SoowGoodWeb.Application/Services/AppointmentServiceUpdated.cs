using Abp.Authorization;
using Microsoft.AspNetCore.Identity;
using SoowGoodWeb.Core.GenericModels;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Application.Service.Services.TokenService;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.Core.Service;
using Nancy;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using SoowGoodWeb.Application.Service.Services.Password;
using SoowGood.Core.Service;
using SoowGoodWeb.Domain.Service.Repositories.Appointment;
using SoowGoodWeb.Domain.Service.Models.AppointmentDto;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SoowGoodWeb.Services
{
    public class AppointmentServiceUpdated : ApplicationService
    {
        private readonly IAppointmentQueryRepository _appointmentQueryRepository;

        public AppointmentServiceUpdated(IAppointmentQueryRepository appointmentQueryRepository)
        {
            _appointmentQueryRepository = appointmentQueryRepository;
        }
        public async Task<IActionResult> GetDoctorPatientProfileId(
    long doctorProfileId,
    string? searchTerm,
    int pageNumber,
    int pageSize)
        {
            var response = new ApiResponse<List<AppointmentPatientDto>>();

            try
            {
                // Fetch all matched data (unpaged)
                var data = await _appointmentQueryRepository.GetDoctorPatientProfileId(
                    doctorProfileId,
                    searchTerm
                );

                int totalCount = data?.Count ?? 0;

                // Apply pagination in controller
                var pagedData = data?
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList() ?? new List<AppointmentPatientDto>();

                if (pagedData.Any())
                {
                    ApiResponseHelper.SetSuccessResponse(
                        response,
                        pagedData,
                        ApiResponseMessage.common_get_successfully,
                        null,
                        null
                    );
                }
                else
                {
                    ApiResponseHelper.SetFailedResponse(
                        response,
                        new List<AppointmentPatientDto>(),
                        ApiResponseMessage.common_no_data_available,
                        null,
                        null
                    );
                }

                return new OkObjectResult(new
                {
                    results = pagedData,
                    count = totalCount
                });
            }
            catch (Exception ex)
            {
                ApiResponseHelper.SetFailedResponse(
                    response,
                    new List<AppointmentPatientDto>(),
                    "Something went wrong!",
                    ex.Message
                );
                return null;
            }
        }



    }
}


