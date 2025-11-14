//using Microsoft.Extensions.Logging;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace SoowGoodWeb.Services
{
    public class PlatformPackageFacilityService : SoowGoodWebAppService, IPlatformPackageFacilityService
    {
        private readonly IRepository<PlatformPackageFacility> _platformPackageFacilityRepository;
        private readonly IRepository<PlatformPackage> _platformPackageRepository;
        //private readonly ILogger<PlatformPackageFacilityService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public PlatformPackageFacilityService(IRepository<PlatformPackageFacility>platformPackageFacilityRepository, IRepository<PlatformPackage> platformPackageRepository, IUnitOfWorkManager unitOfWorkManager
             //ILogger<PlatformPackageFacilityService> logger
            )
        {
            _platformPackageFacilityRepository = platformPackageFacilityRepository;
            _platformPackageRepository = platformPackageRepository;
            _unitOfWorkManager = unitOfWorkManager;
            //_logger = logger;
        }
        public async Task<PlatformPackageFacilityDto> CreateAsync(PlatformPackageFacilityInputDto input)
        {
            var result = new PlatformPackageFacilityDto();
            var doctor = await _platformPackageRepository.GetAsync(d => d.Id == input.PlatformPackageId);
            var newEntity = ObjectMapper.Map<PlatformPackageFacilityInputDto, PlatformPackageFacility>(input);

            var masterDoctor = await _platformPackageFacilityRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();
            result = ObjectMapper.Map<PlatformPackageFacility, PlatformPackageFacilityDto>(masterDoctor);
          
            return result;//ObjectMapper.Map<DoctorDegree, DoctorDegreeDto>(result);
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<PlatformPackageFacilityDto> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PlatformPackageFacilityDto>> GetListAsync()
        {
            List<PlatformPackageFacilityDto>? result = null;

            var allPlatformPackageFacilityDetails = await _platformPackageFacilityRepository.WithDetailsAsync(d => d.PlatformPackage);
            if (!allPlatformPackageFacilityDetails.Any())
            {
                return result;
            }
            result = new List<PlatformPackageFacilityDto>();
            foreach (var item in allPlatformPackageFacilityDetails)
            {

                result.Add(new PlatformPackageFacilityDto()
                {
                    Id = item.Id,
                    PlatformPackageId = item.PlatformPackageId,
                    FacilityName = item.FacilityName,
                });
            }
            var resList = result.OrderByDescending(d => d.Id).ToList();
            return resList;
        }

        

        public async Task<List<PlatformPackageFacilityDto>> GetPlatformPackageFacilityListByPlatformPackageIdAsync(int platformPackageId)
        {
            List<PlatformPackageFacilityDto> list = null;
            var items = await _platformPackageFacilityRepository.WithDetailsAsync(d => d.PlatformPackage);
            items = items.Where(i => i.PlatformPackageId == platformPackageId);
            if (items.Any())
            {
                list = new List<PlatformPackageFacilityDto>();
                foreach (var item in items)
                {
                    list.Add(new PlatformPackageFacilityDto()
                    {
                        Id = item.Id,
                        PlatformPackageId =item.PlatformPackageId,
                        FacilityName = item.FacilityName,

                    });
                }
            }

            return list;
        }

        public async Task<PlatformPackageFacilityDto> UpdateAsync(PlatformPackageFacilityInputDto input)
        {
            var updateItem = ObjectMapper.Map<PlatformPackageFacilityInputDto, PlatformPackageFacility>(input);

            var item = await _platformPackageFacilityRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<PlatformPackageFacility, PlatformPackageFacilityDto>(item);
        }
    }
}
