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

namespace SoowGoodWeb.Services
{
    public class AgentSupervisorService : SoowGoodWebAppService, IAgentSupervisorService
    {
        private readonly IRepository<AgentSupervisor> _agentSupervisorRepository;
        //private readonly IRepository<AgentDegree> _agentDegreeRepository;
        //private readonly IRepository<AgentSpecialization> _agentSpecializationRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public AgentSupervisorService(IRepository<AgentSupervisor> agentSupervisorRepository
                                    , IUnitOfWorkManager unitOfWorkManager)
        {
            _agentSupervisorRepository = agentSupervisorRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<AgentSupervisorDto> CreateAsync(AgentSupervisorInputDto input)
        {
            var totalAgentSupervisors = await _agentSupervisorRepository.GetListAsync();
            var count = totalAgentSupervisors.Count();
            input.AgentSupervisorCode = "SGAS00" + (count + 1);
            var newEntity = ObjectMapper.Map<AgentSupervisorInputDto, AgentSupervisor>(input);

            var agentSupervisor = await _agentSupervisorRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<AgentSupervisor, AgentSupervisorDto>(agentSupervisor);
        }

        public async Task<AgentSupervisorDto> GetAsync(int id)
        {
            var item = await _agentSupervisorRepository.WithDetailsAsync(m=>m.AgentMaster);

            var profile = item.FirstOrDefault(item => item.Id == id);
            

            var result = profile != null ? ObjectMapper.Map<AgentSupervisor, AgentSupervisorDto>(profile) : null;
            result.AgentMasterName =profile.AgentMasterId>0? profile.AgentMaster.AgentMasterOrgName:"";
            return result;
        }

        public async Task<AgentSupervisorDto> UpdateAsync(AgentSupervisorInputDto input)
        {
            var updateItem = ObjectMapper.Map<AgentSupervisorInputDto, AgentSupervisor>(input);

            var item = await _agentSupervisorRepository.UpdateAsync(updateItem);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<AgentSupervisor, AgentSupervisorDto>(item);
        }
        public async Task<List<AgentSupervisorDto>> GetListAsync()
        {
            //var agentSupervisors = await _agentSupervisorRepository.GetListAsync();
            List<AgentSupervisorDto>? result = null;
            var allsupervisorwithDetails = await _agentSupervisorRepository.WithDetailsAsync(s=>s.AgentMaster);
            //var list = allsupervisorwithDetails.ToList();
            
            if (!allsupervisorwithDetails.Any())
            {
                return result;
            }
            result = new List<AgentSupervisorDto>();
            foreach(var item in allsupervisorwithDetails)
            {
                result.Add(new AgentSupervisorDto()
                {
                    Id = item.Id,
                    AgentMasterId = item.AgentMasterId,
                    AgentMasterName = item.AgentMasterId > 0 ? item.AgentMaster.AgentMasterOrgName : "",
                    SupervisorName = item.SupervisorName,
                    AgentSupervisorOrgName = item.AgentSupervisorOrgName,
                    AgentSupervisorCode = item.AgentSupervisorCode,
                    SupervisorIdentityNumber = item.SupervisorIdentityNumber,
                    SupervisorMobileNo = item.SupervisorMobileNo,
                    Address = item.Address,
                    City = item.City,
                    ZipCode = item.ZipCode,
                    Country = item.Country,
                    PhoneNo = item.PhoneNo,
                    Email = item.Email,
                    EmergencyContact = item.EmergencyContact,
                    AgentSupervisorDocNumber = item.AgentSupervisorDocNumber,
                    AgentSupervisorDocExpireDate = item.AgentSupervisorDocExpireDate,
                    IsActive = item.IsActive,
                    DisplayName= item.AgentSupervisorCode + "-" + item.AgentSupervisorOrgName + "-" + item.SupervisorName
                }) ; 
            }
            return result;
            //return ObjectMapper.Map<List<AgentSupervisor>, List<AgentSupervisorDto>>(agentSupervisors);
        }

        public async Task<List<AgentSupervisorDto>> GetListByMasterIdAsync(long masterId)
        {
            //var agentSupervisors = await _agentSupervisorRepository.GetListAsync();
            List<AgentSupervisorDto>? result = null;
            var allsupervisorwithDetails = await _agentSupervisorRepository.WithDetailsAsync(s => s.AgentMaster);
            var supervisorProfiles=allsupervisorwithDetails.Where(m=>m.AgentMasterId==masterId).ToList();
            if (!supervisorProfiles.Any())
            {
                return result;
            }
            result = new List<AgentSupervisorDto>();
            foreach (var item in supervisorProfiles)
            {
                result.Add(new AgentSupervisorDto()
                {
                    Id = item.Id,
                    AgentMasterId = item.AgentMasterId,
                    AgentMasterName = item.AgentMasterId > 0 ? item.AgentMaster.AgentMasterOrgName : "",
                    SupervisorName = item.SupervisorName,
                    AgentSupervisorOrgName = item.AgentSupervisorOrgName,
                    AgentSupervisorCode = item.AgentSupervisorCode,
                    SupervisorIdentityNumber = item.SupervisorIdentityNumber,
                    SupervisorMobileNo = item.SupervisorMobileNo,
                    Address = item.Address,
                    City = item.City,
                    ZipCode = item.ZipCode,
                    Country = item.Country,
                    PhoneNo = item.PhoneNo,
                    Email = item.Email,
                    EmergencyContact = item.EmergencyContact,
                    AgentSupervisorDocNumber = item.AgentSupervisorDocNumber,
                    AgentSupervisorDocExpireDate = item.AgentSupervisorDocExpireDate,
                    IsActive = item.IsActive,
                    DisplayName = item.AgentSupervisorCode + "-" + item.AgentSupervisorOrgName + "-" + item.SupervisorName
                });
            }
            return result;
            //return ObjectMapper.Map<List<AgentSupervisor>, List<AgentSupervisorDto>>(agentSupervisors);
        }

        public async Task<List<AgentSupervisorDto>> GetAgentSupervisorsByAgentMasterListAsync(long agentMasterId)
        {
            List<AgentSupervisorDto>? result = null;
            var allsupervisorwithDetails = await _agentSupervisorRepository.WithDetailsAsync(s => s.AgentMaster);
            var allsupervisors = await _agentSupervisorRepository.GetListAsync(s => s.AgentMasterId==agentMasterId);

            if (!allsupervisorwithDetails.Any())
            {
                return result;
            }
            result = new List<AgentSupervisorDto>();
            foreach (var item in allsupervisors)
            {
                result.Add(new AgentSupervisorDto()
                {
                    Id = item.Id,
                    SupervisorName = item.SupervisorName,
                    DisplayName = item.AgentSupervisorCode + "-" + item.AgentSupervisorOrgName + "-" + item.SupervisorName

                });
            }
            return result;
        }
        public async Task<AgentSupervisorDto> GetByUserNameAsync(string userName)
        {
            var agentSupervisor = await _agentSupervisorRepository.WithDetailsAsync();
            var item = agentSupervisor.Where(x => x.SupervisorMobileNo == userName).FirstOrDefault();

            return ObjectMapper.Map<AgentSupervisor, AgentSupervisorDto>(item);
        }
        public async Task<List<AgentSupervisorDto>> GetSupervisorListFilterByAdminAsync(DataFilterModel? supervisorFilterModel, FilterModel filterModel)
        {
            List<AgentSupervisorDto> result = null;
            try
            {
                var profileWithDetails = await _agentSupervisorRepository.WithDetailsAsync(s => s.AgentMaster);
                var profiles = profileWithDetails.Where(p => !string.IsNullOrEmpty(p.SupervisorName)).ToList();
                //var schedules = await _patientProfileRepository.WithDetailsAsync();
                //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
                if (!profileWithDetails.Any())
                {
                    return result;
                }
                result = new List<AgentSupervisorDto>();
                if (supervisorFilterModel != null && !string.IsNullOrEmpty(supervisorFilterModel.name))
                {
                    //profiles = profiles.Where(p => p.PatientName.ToLower().Contains(patientFilterModel.name.ToLower().Trim())).ToList();
                    //profiles = profiles.Where(p => p.SupervisorName.ToLower().Contains(supervisorFilterModel.name.ToLower().Trim())).ToList();
                    profiles = profiles.Where(p => (p.SupervisorName != null && p.SupervisorName.ToLower().Contains(supervisorFilterModel.name.ToLower().Trim())) || (p.SupervisorMobileNo != null && p.SupervisorMobileNo.ToLower().Contains(supervisorFilterModel.name.ToLower().Trim()))).ToList();
                }
                if (supervisorFilterModel?.isActive != null)
                {
                    if (supervisorFilterModel?.isActive == true)
                    {
                        profiles = profiles.Where(p => p.IsActive == true).ToList();
                    }
                    else
                    {
                        profiles = profiles.Where(p => p.IsActive == false).ToList();
                    }
                }
                foreach (var item in profiles)
                {

                    result.Add(new AgentSupervisorDto()
                    {
                        Id = item.Id,
                        AgentMasterId = item.AgentMasterId,
                        AgentMasterName = item.AgentMasterId > 0 ? item.AgentMaster.AgentMasterOrgName : "",
                        SupervisorName = item.SupervisorName,
                        AgentSupervisorOrgName = item.AgentSupervisorOrgName,
                        AgentSupervisorCode = item.AgentSupervisorCode,
                        SupervisorIdentityNumber = item.SupervisorIdentityNumber,
                        SupervisorMobileNo = item.SupervisorMobileNo,
                        Address = item.Address,
                        City = item.City,
                        ZipCode = item.ZipCode,
                        Country = item.Country,
                        PhoneNo = item.PhoneNo,
                        Email = item.Email,
                        EmergencyContact = item.EmergencyContact,
                        AgentSupervisorDocNumber = item.AgentSupervisorDocNumber,
                        AgentSupervisorDocExpireDate = item.AgentSupervisorDocExpireDate,
                        IsActive = item.IsActive,

                    });
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
    }
}
