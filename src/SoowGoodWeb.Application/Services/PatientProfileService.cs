using AutoMapper;
using Castle.DynamicProxy.Internal;
using Microsoft.IdentityModel.Tokens;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Application.Service.Services.Patient;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Repositories.SignUp;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SoowGoodWeb.Services
{
    public class PatientProfileService : SoowGoodWebAppService, IPatientProfileService
    {
        private readonly IRepository<PatientProfile> _patientProfileRepository;
        private readonly IRepository<PatientProfile, long> _patientRepository;
        private readonly IRepository<AgentProfile> _agentProfileRepository;
        //private readonly IRepository<AgentDegree> _agentDegreeRepository;
        //private readonly IRepository<AgentSpecialization> _agentSpecializationRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly PatientService _patientService;

        private readonly ISignUpRepository _signUpRepository;

        public PatientProfileService(IRepository<PatientProfile> patientProfileRepository,
            IRepository<PatientProfile, long> patientRepository,
            IRepository<AgentProfile> agentProfileRepository,
            IUnitOfWorkManager unitOfWorkManager,
            //, PatientService patientService
            ISignUpRepository signUpRepository
            )
        {
            _patientProfileRepository = patientProfileRepository;
            _patientRepository = patientRepository;
            _agentProfileRepository = agentProfileRepository;
            _unitOfWorkManager = unitOfWorkManager;
            //_patientService = patientService;
            _signUpRepository = signUpRepository;
        }

        public async Task<PatientProfileDto> CreateAsync(PatientProfileInputDto input)
        {
            var result = new PatientProfileDto();
            var signUpResponse = new Response<UserSignUpResponseDto>();
            try
            {
                //var singUpModel = ObjectMapper.Map<PatientProfileInputDto, UserSingupRequestDto>(input);
                if (input.CreatorRole != null)
                {
                    var newSignUp = new UserSingupRequestDto
                    {
                        UserName = input.PatientMobileNo ?? input.PatientMobileNo,
                        Password = "Coppa@123",
                        PhoneNumber = input.PatientMobileNo ?? input.PatientMobileNo,
                        RoleId = "Patient",
                        Email = input.PatientMobileNo + "@gmail.com",
                        Surname = input.PatientName,
                        Name = input.PatientName
                    };

                     signUpResponse = await _signUpRepository.SignUpUser(newSignUp);
                }
               


                var queryable = await _patientProfileRepository.GetQueryableAsync();
                var patientProfile = new PatientProfile();

                if (!string.IsNullOrWhiteSpace(input.MobileNo))
                {
                    patientProfile = queryable

                    .Where(x => x.MobileNo.Trim() == input.MobileNo.Trim()) // Ensure proper matching
                    .FirstOrDefault();
                }
                else
                {
                    patientProfile = queryable

                      .Where(x => x.MobileNo.Trim() == input.PatientMobileNo.Trim()) // Ensure proper matching
                       .FirstOrDefault();

                }

                if (patientProfile != null) // Update existing patient
                {
                    ObjectMapper.Map(input, patientProfile); // Map only the changes
                    patientProfile = await _patientProfileRepository.UpdateAsync(patientProfile);
                }
                else // Insert new patient
                {
                    var totalPatients = await _patientProfileRepository.GetListAsync();
                    var count = totalPatients.Count();
                    var date = DateTime.Now;
                    input.PatientCode = "SGP" + date.ToString("yyyyMMdd") + (count + 1);

                    var newEntity = ObjectMapper.Map<PatientProfileInputDto, PatientProfile>(input);
                    if (input.CreatorRole != null)
                    {
                        newEntity.UserId = signUpResponse.Result.UserId;

                    }
                    patientProfile = await _patientProfileRepository.InsertAsync(newEntity);
                }

                result = ObjectMapper.Map<PatientProfile, PatientProfileDto>(patientProfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log the error
            }
            return result;
        }



        public async Task<PatientProfileDto> GetAsync(long id)
        {
            var item = await _patientProfileRepository.GetAsync(x => x.Id == id);
            if (item.CreatorRole != null)
            {
                item.PatientMobileNo = item.PatientMobileNo;  // assuming you want to copy MobileNo to PatientMobile
                item.FullName = item.PatientName;
            }
            else
            {
                item.PatientMobileNo = item.MobileNo;
                item.FullName = item.FullName;
            }

            return ObjectMapper.Map<PatientProfile, PatientProfileDto>(item);
           



            //var item = await _patientProfileRepository.WithDetailsAsync();
            //var profile = item.FirstOrDefault(item => item.Id == id);
            //var result = profile != null ? ObjectMapper.Map<PatientProfile, PatientProfileDto>(profile) : null;

            //return result;
        }

        public async Task<PatientProfileDto> GetByPhoneAndCodeAsync(string pCode, string pPhone)
        {
            var item = await _patientProfileRepository.GetAsync(x => x.PatientCode == pCode && x.PatientMobileNo == pPhone);//.WithDetailsAsync();
                                                                                                                            //var patient = item.Where(x => x.PatientCode == pCode && x.PatientMobileNo == pPhone).FirstOrDefault();
                                                                                                                            //if(patient!=null)
            return ObjectMapper.Map<PatientProfile, PatientProfileDto>(item);

            //var item = await _patientProfileRepository.WithDetailsAsync();
            //var profile = item.FirstOrDefault(item => item.Id == id);
            //var result = profile != null ? ObjectMapper.Map<PatientProfile, PatientProfileDto>(profile) : null;

            //return null;
        }

        public async Task<PatientProfileDto> GetByUserNameAsync(string userName)
        {
            var item = await _patientProfileRepository.GetAsync(x => x.MobileNo == userName|| x.PatientMobileNo==userName);

            return ObjectMapper.Map<PatientProfile, PatientProfileDto>(item);
        }

        public async Task<List<PatientProfileDto>> GetListAsync()
        {
            var profiles = await _patientProfileRepository.GetListAsync();
            return ObjectMapper.Map<List<PatientProfile>, List<PatientProfileDto>>(profiles);
        }

        public async Task<List<PatientProfileDto>> GetListPatientListByAdminAsync()
        {
            List<PatientProfileDto>? result = null;
            var allProfile = await _patientProfileRepository.GetListAsync();
            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);

            if (!allProfile.Any())
            {
                return result;
            }

            result = new List<PatientProfileDto>();
            foreach (var item in allProfile)
            {
                var agent = item.CreatorRole == "agent" ? agentDetails.Where(a => a.Id == item.CreatorEntityId).FirstOrDefault() : null;
                result.Add(new PatientProfileDto()
                {
                    Id = item.Id,
                    FullName = item.FullName,
                    Email = item.Email,
                    MobileNo = item.MobileNo,
                    PatientCode = item.PatientCode,
                    DateOfBirth = item.DateOfBirth,
                    Gender = item.Gender,
                    GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                    BloodGroup = item.BloodGroup,
                    Address = item.Address,
                    ProfileRole = "Patient",
                    PatientName = item.PatientName,
                    PatientMobileNo = item.PatientMobileNo,
                    CreatorRole = item.CreatorRole,
                    BoothName = item.CreatorRole == "agent" ? agent?.Address : "N/A",
                    AgentMasterName = item.CreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                    AgentSupervisorName = item.CreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",

                });
            }
            return result.OrderByDescending(d => d.Id).ToList();
        }

        public async Task<List<PatientProfileDto>> GetListPatientListByAgentMasterAsync(long masterId)
        {
            List<PatientProfileDto>? result = null;
            var allProfile = await _patientProfileRepository.GetListAsync();
            var profiles = allProfile.Where(c => c.CreatorRole == "agent").ToList();
            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var agentsByMaster = agentDetails.Where(a => a.AgentMasterId == masterId).ToList();

            var itemProfiles = (from p in profiles join agents in agentsByMaster on p.CreatorEntityId equals agents.Id select p).ToList();

            if (!itemProfiles.Any())
            {
                return result;
            }

            result = new List<PatientProfileDto>();
            foreach (var item in itemProfiles)
            {
                var agent = item.CreatorRole == "agent" ? agentsByMaster.Where(a => a.Id == item.CreatorEntityId).FirstOrDefault() : null;
                result.Add(new PatientProfileDto()
                {
                    Id = item.Id,
                    PatientName = item.PatientName,
                    PatientEmail = item.PatientEmail,
                    PatientMobileNo = item.PatientMobileNo,
                    PatientCode = item.PatientCode,
                    DateOfBirth = item.DateOfBirth,
                    Gender = item.Gender,
                    GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                    BloodGroup = item.BloodGroup,
                    Address = item.Address,
                    ProfileRole = "Patient",
                    CreatorRole = item.CreatorRole,
                    BoothName = item.CreatorRole == "agent" ? agent?.Address : "N/A",
                    AgentMasterName = item.CreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                    AgentSupervisorName = item.CreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",

                });
            }
            return result.OrderByDescending(d => d.Id).ToList();
        }

        public async Task<List<PatientProfileDto>> GetListPatientListByAgentSuperVisorAsync(long supervisorId)
        {
            List<PatientProfileDto>? result = null;
            var allProfile = await _patientProfileRepository.GetListAsync();
            var profiles = allProfile.Where(c => c.CreatorRole == "agent").ToList();
            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var agentsBySuperVisor = agentDetails.Where(a => a.AgentSupervisorId == supervisorId).ToList();

            var itemProfiles = (from p in profiles join agents in agentsBySuperVisor on p.CreatorEntityId equals agents.Id select p).ToList();



            //var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);

            if (!itemProfiles.Any())
            {
                return result;
            }

            result = new List<PatientProfileDto>();
            foreach (var item in itemProfiles)
            {
                var agent = item.CreatorRole == "agent" ? agentsBySuperVisor.Where(a => a.Id == item.CreatorEntityId).FirstOrDefault() : null;
                result.Add(new PatientProfileDto()
                {
                    Id = item.Id,
                    PatientName = item.PatientName,
                    PatientEmail = item.PatientEmail,
                    PatientMobileNo = item.PatientMobileNo,
                    PatientCode = item.PatientCode,
                    DateOfBirth = item.DateOfBirth,
                    Gender = item.Gender,
                    GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                    BloodGroup = item.BloodGroup,
                    Address = item.Address,
                    ProfileRole = "Patient",
                    CreatorRole = item.CreatorRole,
                    BoothName = item.CreatorRole == "agent" ? agent?.Address : "N/A",
                    AgentMasterName = item.CreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                    AgentSupervisorName = item.CreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",

                });
            }
            return result.OrderByDescending(d => d.Id).ToList();
        }

        public async Task<PatientProfileDto> GetByUserIdAsync(Guid userId)
        {
            var item = await _patientProfileRepository.GetAsync(x => x.UserId == userId);
            return ObjectMapper.Map<PatientProfile, PatientProfileDto>(item);
        }

        public async Task<PatientProfileDto> UpdateAsync(PatientProfileInputDto input)
        {
            var result = new PatientProfileDto();
            try
            {
                var itemPatient = await _patientRepository.FindAsync(input.Id);
                if (itemPatient != null)
                {
                    itemPatient.FullName = !string.IsNullOrEmpty(input.FullName) ? input.FullName : itemPatient.FullName;
                    itemPatient.DateOfBirth = input.DateOfBirth != null ? input.DateOfBirth : itemPatient.DateOfBirth;
                    itemPatient.Age = input.Age > 0 ? input.Age : itemPatient.Age;
                    itemPatient.Gender = input.Gender != null ? input.Gender : itemPatient.Gender;
                    itemPatient.BloodGroup = input.BloodGroup != null ? input.BloodGroup : itemPatient.BloodGroup;
                    itemPatient.Address = !string.IsNullOrEmpty(input.Address) ? input.Address : itemPatient.Address;
                    itemPatient.City = !string.IsNullOrEmpty(input.City) ? input.City : itemPatient.City;
                    itemPatient.ZipCode = !string.IsNullOrEmpty(input.ZipCode) ? input.ZipCode : itemPatient.ZipCode;
                    itemPatient.Country = !string.IsNullOrEmpty(input.Country) ? input.Country : itemPatient.Country;
                    itemPatient.Email = !string.IsNullOrEmpty(input.Email) ? input.Email : itemPatient.Email;
                    itemPatient.PatientName = input.PatientName; //!string.IsNullOrEmpty(itemPatient.PatientName) ? itemPatient.PatientName : 
                    itemPatient.PatientMobileNo = input.PatientMobileNo; //!string.IsNullOrEmpty(itemPatient.PatientMobileNo) ? itemPatient.PatientMobileNo :
                    itemPatient.PatientEmail = input.PatientEmail; //!string.IsNullOrEmpty(itemPatient.PatientEmail) ? itemPatient.PatientEmail : 

                    var item = await _patientRepository.UpdateAsync(itemPatient);
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    result = ObjectMapper.Map<PatientProfile, PatientProfileDto>(item);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }


        public async Task<List<PatientProfileDto>> GetPatientListByUserProfileIdAsync(long profileId, string role)
        {
            var profiles = await _patientProfileRepository.GetListAsync(p => p.CreatorEntityId == profileId && p.CreatorRole == role);
            foreach (var profile in profiles)
            {
                if (profile.CreatorRole != null)
                {
                    profile.PatientMobileNo = profile.PatientMobileNo;  // assuming you want to copy MobileNo to PatientMobile
                }
                else
                {
                    profile.PatientMobileNo = profile.MobileNo;
                }
            }
            return ObjectMapper.Map<List<PatientProfile>, List<PatientProfileDto>>(profiles);
        }

        public async Task<List<PatientProfileDto>> GetDoctorListFilterAsync(DataFilterModel? patientFilterModel, FilterModel filterModel)
        {
            List<PatientProfileDto> result = null;
            var profileWithDetails = await _patientProfileRepository.WithDetailsAsync();
            var profiles = profileWithDetails.ToList();
            var schedules = await _patientProfileRepository.WithDetailsAsync();
            //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
            if (!profileWithDetails.Any())
            {
                return result;
            }
            result = new List<PatientProfileDto>();

            if (!string.IsNullOrEmpty(patientFilterModel?.name))
            {
                profiles = profiles.Where(p => p.FullName.ToLower().Contains(patientFilterModel.name.ToLower().Trim())).ToList();
            }

            profiles = profiles.Skip(filterModel.Offset)
                               .Take(filterModel.Limit).ToList();

            foreach (var item in profiles)
            {
                result.Add(new PatientProfileDto()
                {
                    Id = item.Id,
                    PatientName = item.PatientName,
                    PatientEmail = item.PatientEmail,
                    PatientMobileNo = item.PatientMobileNo,
                    PatientCode = item.PatientCode,
                    DateOfBirth = item.DateOfBirth,
                    Gender = item.Gender,
                    GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                    BloodGroup = item.BloodGroup,
                    Address = item.Address,
                    ProfileRole = "Patient",
                });
            }
            return result;
        }

        public async Task<List<PatientProfileDto>> GetDoctorListByCreatorIdFilterAsync(long profileId, DataFilterModel? patientFilterModel, FilterModel filterModel)
        {
            List<PatientProfileDto> result = null;
            var profileWithDetails = await _patientProfileRepository.WithDetailsAsync();
            var profiles = profileWithDetails.Where(c => c.CreatorEntityId == profileId).ToList();
            var schedules = await _patientProfileRepository.WithDetailsAsync();
            //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
            if (!profileWithDetails.Any())
            {
                return result;
            }
            result = new List<PatientProfileDto>();

            if (!string.IsNullOrEmpty(patientFilterModel?.name))
            {
                profiles = profiles.Where(p => p.FullName.ToLower().Contains(patientFilterModel.name.ToLower().Trim())).ToList();
            }

            profiles = profiles.Skip(filterModel.Offset)
                               .Take(filterModel.Limit).ToList();

            foreach (var item in profiles)
            {
                result.Add(new PatientProfileDto()
                {
                    Id = item.Id,
                    PatientName = item.PatientName,
                    PatientEmail = item.PatientEmail,
                    PatientMobileNo = item.PatientMobileNo,
                    PatientCode = item.PatientCode,
                    DateOfBirth = item.DateOfBirth,
                    Gender = item.Gender,
                    GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                    BloodGroup = item.BloodGroup,
                    Address = item.Address,
                    ProfileRole = "Patient",
                });
            }
            return result;
        }

        public async Task<List<PatientProfileDto>> GetPatientListBySearchUserProfileIdAsync(long profileId, string role, string name)
        {
            var profiles = await _patientProfileRepository.GetListAsync(p => p.CreatorEntityId == profileId && p.CreatorRole == role);
            if (!string.IsNullOrEmpty(name))
            {
                profiles = profiles.Where(p => p.FullName.Contains(name)).ToList();
            }
            return ObjectMapper.Map<List<PatientProfile>, List<PatientProfileDto>>(profiles);
        }
        public async Task<List<PatientProfileDto>> GetPatientListFilterByAdminAsync(long? userId, string? role, DataFilterModel? patientFilterModel, FilterModel filterModel)
        {
            List<PatientProfileDto> result = null;
            List<PatientProfile>? patients = null;
            List<PatientProfile>? itemPatients = null;
            try
            {
                var profileWithDetails = await _patientProfileRepository.GetListAsync();
                var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
                var profiles = profileWithDetails
                                .Where(p => !string.IsNullOrEmpty(p.PatientName) || !string.IsNullOrEmpty(p.FullName))
                                .ToList();

                //var schedules = await _patientProfileRepository.WithDetailsAsync();
                //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
                if (!string.IsNullOrEmpty(role) && role != "sgadmin")
                {
                    patients = profiles.Where(c => c.CreatorRole == "agent").ToList();

                    var agentsByMasterSupervisors = agentDetails.Where(a => role == "masteragent" ? a.AgentMasterId == userId : a.AgentSupervisorId == userId).ToList();

                    itemPatients = (from app in patients join agents in agentsByMasterSupervisors on app.CreatorEntityId equals agents.Id select app).ToList();

                }
                //else
                //{
                //    itemPatients = profiles.Where(a => a.AppointmentStatus > 0).ToList();//allAppoinment.Where(d => (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed || d.AppointmentStatus == AppointmentStatus.Pending || d.AppointmentStatus == AppointmentStatus.Cancelled || d.AppointmentStatus == AppointmentStatus.InProgress || d.AppointmentStatus == AppointmentStatus.Failed)).ToList();
                //}
                if (!profileWithDetails.Any())
                {
                    return result;
                }
                result = new List<PatientProfileDto>();
                //if (patientFilterModel != null && !string.IsNullOrEmpty(patientFilterModel.name))
                //{
                //    //profiles = profiles.Where(p => p.PatientName.ToLower().Contains(patientFilterModel.name.ToLower().Trim())).ToList();
                //    profiles = profiles.Where(p => (p.PatientName != null && p.PatientName.ToLower().Contains(patientFilterModel.name.ToLower().Trim())) ||
                //                                   (p.PatientMobileNo != null && p.PatientMobileNo.ToLower().Contains(patientFilterModel.name.ToLower().Trim()))).ToList();

                //}
                foreach (var item in profiles)
                {
                    var agent = item.CreatorRole == "agent" ? agentDetails.Where(a => a.Id == item.CreatorEntityId).FirstOrDefault() : null;
                    result.Add(new PatientProfileDto()
                    {
                        Id = item.Id,
                        FullName = item.FullName,
                        Email = item.Email,
                        MobileNo = item.MobileNo,
                        PatientCode = item.PatientCode,
                        DateOfBirth = item.DateOfBirth,
                        Gender = item.Gender,
                        GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                        BloodGroup = item.BloodGroup,
                        Address = item.Address,
                        ProfileRole = "Patient",
                        CreatorRole = item.CreatorRole,
                        PatientMobileNo = item.PatientMobileNo,
                        PatientName = item.PatientName,
                        BoothName = item.CreatorRole == "agent" ? agent?.Address : "N/A",
                        AgentMasterName = item.CreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                        AgentSupervisorName = item.CreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",
                    });
                }

                if (patientFilterModel != null && !string.IsNullOrEmpty(patientFilterModel.name))
                {
                   
                   
                    result = result.Where(p => (p.FullName != null && p.FullName.ToLower().Contains(patientFilterModel.name.ToLower().Trim()))||
                    (p.PatientName != null && p.PatientName.ToLower().Contains(patientFilterModel.name.ToLower().Trim()))||
                                                   (p.MobileNo != null && p.MobileNo.ToLower().Contains(patientFilterModel.name.ToLower().Trim()))
                                                   || (p.PatientMobileNo != null && p.PatientMobileNo.ToLower().Contains(patientFilterModel.name.ToLower().Trim()))
                                                   || (p.BoothName != null && p.BoothName.ToLower().Contains(patientFilterModel.name.ToLower()))
                                                   || (p.CreatorRole != null && p.CreatorRole.ToLower().Contains(patientFilterModel.name.ToLower()))).ToList();
                }
                //result = result.Skip(filterModel.Offset)
                //                   .Take(filterModel.Limit).ToList();
            }
            catch
            {
                return null;
            }

            return result;
        }
    
        //public async Task<ApiResponse<List<PatientProfileDto>>> GetPatientProfileByMobileNo(string? mobileNo = null, string? creatorEntityId = null)
        //{
        //    var apiResponse = new ApiResponse<List<PatientProfileDto>>();
        //    var getPatientProfile = await _patientService.GetPatientsBySearchByMobileNo(mobileNo, creatorEntityId);
        //    if (getPatientProfile.Result.Count == 0)
        //    {
        //        ApiResponseHelper.SetFailedResponse(apiResponse, null, "No Data Available", null);
        //        return apiResponse;
        //    }
        //    var result = getPatientProfile.Result;

        //    var mappedPatientProfile = ObjectMapper.Map<List<PatientProfile>, List<PatientProfileDto>>(result);

        //    ApiResponseHelper.SetSuccessResponse(apiResponse, mappedPatientProfile, "Success", null);

        //    return apiResponse;

        //}

    }
}
