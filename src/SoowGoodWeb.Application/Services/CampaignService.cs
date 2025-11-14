using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace SoowGoodWeb.Services
{
    public class CampaignService : SoowGoodWebAppService, ICampaignService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignDoctor> _campaignDoctorRepository;
        //private readonly IRepository<AgentDegree> _agentDegreeRepository;
        //private readonly IRepository<AgentSpecialization> _agentSpecializationRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public CampaignService(IRepository<Campaign> campaignRepository, IRepository<CampaignDoctor> campaignDoctorRepository
                                    , IUnitOfWorkManager unitOfWorkManager)
        {
            _campaignRepository = campaignRepository;
            _campaignDoctorRepository = campaignDoctorRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<CampaignDto> CreateAsync(CampaignInputDto input)
        {
            //var count = await _campaignRepository.GetCountAsync(); // Get count directly from the repository

            //// Generate CampaignCode
            //input.CampaignCode = "SGAM00" + (count + 1);

            var newEntity = ObjectMapper.Map<CampaignInputDto, Campaign>(input);

            var campaign = await _campaignRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<Campaign, CampaignDto>(campaign);
        }

        public async Task<CampaignDto> GetAsync(int id)
        {
            var item = await _campaignRepository.WithDetailsAsync();

            var profile = item.FirstOrDefault(item => item.Id == id);

            var result = profile != null ? ObjectMapper.Map<Campaign, CampaignDto>(profile) : null;

            return result;
        }

        public async Task<List<CampaignDto>> GetListAsync()
        {
            List<CampaignDto>? result = null;
            var allcampaignwithDetails = await _campaignRepository.GetListAsync();
            //var list = allsupervisorwithDetails.ToList();
            if (!allcampaignwithDetails.Any())
            {
                return result;
            }
            result = new List<CampaignDto>();
            foreach (var item in allcampaignwithDetails)
            {
                var campaignDoctors = await _campaignDoctorRepository.WithDetailsAsync(c=>c.Campaign,d=>d.DoctorProfile);
                //var DoctorById = campaignDoctors.Where(f => f.CampaignId == item.Id).ToList();
                var doctorDtos = ObjectMapper.Map<List<CampaignDoctor>, List<CampaignDoctorDto>>(campaignDoctors.ToList());

                var doctors = doctorDtos.Where(d => d.CampaignId == item.Id).ToList();

                result.Add(new CampaignDto()
                {
                    Id = item.Id,
                    Title = item.Title,
                    SubTitle = item.SubTitle,
                    SelectedDoctor = doctors,
                    IsActive = item.IsActive,

                });
               
            }
            return result;
        }

        public async Task<CampaignDto> UpdateAsync(CampaignInputDto input)
        {
            var updateItem = ObjectMapper.Map<CampaignInputDto, Campaign>(input);

            var item = await _campaignRepository.UpdateAsync(updateItem);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<Campaign, CampaignDto>(item);
        }


    }
}


      
