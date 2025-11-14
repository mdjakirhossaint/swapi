using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoowGoodWeb.Core.GenericModels;
using SoowGood.Core.Service;
using SoowGoodWeb.Application.Service.Services.TokenService;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;
using SoowGoodWeb.Core.Service;

namespace SoowGoodWeb.Controllers
{
    [ApiController]
    public class DoctorProfileController : ControllerBase
    {
        private readonly IDoctorProfileService _doctorProfileservice;
        private readonly TokenService _tokenService;
        public DoctorProfileController(IDoctorProfileService doctorProfileservice
            , TokenService tokenService)
        {
            _doctorProfileservice = doctorProfileservice;
            _tokenService = tokenService;
        }
        [HttpGet]
        [Route("get-all-active-doctor")]
        public async Task<ApiResponse<List<DoctorProfileDto>>> GetAllActiveDoctorListAsync()
        {
            var apiResponse = new ApiResponse<List<DoctorProfileDto>>();
            // Get the HTTP Request from IHttpContextAccessor
            var authorizationToken = Request.Headers["Authorization"].FirstOrDefault();

            var (userId, matchKey, status, message) = await _tokenService.GetUserIdFromAuthorizationToken(authorizationToken);

            if (userId == Guid.Empty)
            {
                apiResponse.results = new();
                apiResponse.message = message;
                apiResponse.status = StatusResponseMessage.failed;
                apiResponse.status_code = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
            List<DoctorProfileDto> result = null;
            var profileWithDetails = await _doctorProfileservice.GetListAsync();

            //var mappedObject = ObjectMapper.Map<List<DoctorProfile>, List<DoctorProfileDto>>(profileWithDetails.ToList());
            apiResponse.results = profileWithDetails;
            return apiResponse;
        }
    }
}
