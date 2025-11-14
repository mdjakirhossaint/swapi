//using Microsoft.Extensions.Logging;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace SoowGoodWeb.Services
{
    public class PlatformPackageService : SoowGoodWebAppService, IPlatformPackageService
    {
        private readonly IRepository<PlatformPackage> _platformPackageRepository;
        private readonly IRepository<DoctorProfile> _doctorProfileRepository;
        private readonly IRepository<PlatformPackageFacility> _platformPackageFacilityRepository;
        private readonly IRepository<DoctorSpecialization> _doctorSpecializationRepository;
        private readonly IRepository<DoctorDegree> _doctorDegreeRepository;
        private readonly IRepository<DocumentsAttachment> _documentsAttachment;
       
        //private readonly ILogger<PlatformPackageService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public PlatformPackageService(IRepository<PlatformPackage> platformPackageRepository, IRepository<PlatformPackageFacility> platformPackageFacilityRepository, IRepository<DoctorProfile> doctorProfileRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<DoctorSpecialization> doctorSpecializationRepository, IRepository<DoctorDegree> doctorDegreeRepository,
             //ILogger<PlatformPackageService> logger,
             IRepository<DocumentsAttachment> documentsAttachment)
        {
            _platformPackageRepository = platformPackageRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWorkManager = unitOfWorkManager;
           _platformPackageFacilityRepository = platformPackageFacilityRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _doctorDegreeRepository = doctorDegreeRepository;
            _documentsAttachment = documentsAttachment;
           
            //_logger = logger;
        }
        public async Task<PlatformPackageDto> CreateAsync(PlatformPackageInputDto input)
        {
             var result = new PlatformPackageDto();

            if (input.PackageProviderId.HasValue)
            {
                var doctor = await _doctorProfileRepository.GetAsync(d => d.Id == input.PackageProviderId.Value);

                
                if (doctor != null)
                {
                    result.DoctorName = doctor.FullName;
                }
                else
                {
                    throw new EntityNotFoundException($"DoctorProfile with id = {input.PackageProviderId.Value} does not exist.");
                }
            }


            var newEntity = ObjectMapper.Map<PlatformPackageInputDto, PlatformPackage>(input);
            var platformPackage = await _platformPackageRepository.InsertAsync(newEntity);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            result = ObjectMapper.Map<PlatformPackage, PlatformPackageDto>(platformPackage);

            return result;
        }


      

        //public async Task<PlatformPackageDto> GetAsync(int id)
        //{
        //    List<PlatformPackageDto>? result = null;
        //    var allPlatformPackageDetails = await _platformPackageRepository.WithDetailsAsync(s => s.PackageProvider);
        //    var item = allPlatformPackageDetails.Where(x => x.Id == id);
        //    var attachedItems = await _documentsAttachment.WithDetailsAsync();
        //    var medicalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
        //    var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medicalSpecializations.ToList());

        //    var medicalDegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
        //    var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicalDegrees.ToList());
        //    if (!item.Any())
        //    {
        //        return result;
        //    }
        //    result = new List<PlatformPackageDto>();
        //    if (item.Any())
        //    {

        //        foreach (var itm in item)
        //        {
        //            var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
        //                                                             && x.EntityId == itm.PackageProviderId
        //                                                             && x.AttachmentType == AttachmentType.ProfilePicture
        //                                                             && x.IsDeleted == false).FirstOrDefault();
        //            var degrees = doctorDegrees.Where(d => d.DoctorProfileId == itm.PackageProviderId).ToList();
        //            string degStr = string.Empty;
        //            foreach (var d in degrees)
        //            {
        //                degStr = degStr + d.DegreeName + ",";
        //            }

        //            if (!string.IsNullOrEmpty(degStr))
        //            {
        //                degStr = degStr.Remove(degStr.Length - 1);
        //            }

        //            var specializations = doctorSpecializations.Where(sp => sp.DoctorProfileId == itm.PackageProviderId).ToList();
        //            string expStr = string.Empty;
        //            foreach (var e in specializations)
        //            {
        //                expStr = expStr + e.SpecializationName + ",";

        //            }

        //            if (!string.IsNullOrEmpty(expStr))
        //            {
        //                expStr = expStr.Remove(expStr.Length - 1);
        //            }
        //            result.Add(new PlatformPackageDto()
        //            {
        //                Id = itm.Id,
        //                PackageName = itm.PackageName,
        //                PackageTitle = itm.PackageTitle,
        //                PackageDescription = itm.PackageDescription,

        //                PackageProviderId = itm.PackageProviderId,
        //                Price = itm.Price,
        //                Reason = itm.Reason,
        //                DoctorName = itm.PackageProviderId > 0 ? itm.PackageProvider.FullName : "",
        //                DoctorTitle = itm.PackageProvider?.DoctorTitle,
        //                DoctorTitleName = itm.PackageProvider?.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(itm.PackageProvider?.DoctorTitle).ToString() : "n/a",
        //                DoctorCode = itm.PackageProviderId > 0 ? itm.PackageProvider.DoctorCode : "",
        //                DoctorSpecialization = specializations,
        //                AreaOfExperties = expStr,
        //                DoctorDegrees = degrees,
        //                Qualifications = degStr,
        //                ProfilePic = profilePics?.Path,
        //            });
        //        }
        //    }

        //    var resList = result.OrderByDescending(d => d.Id).ToList();
        //    return resList;
        //}

        public async Task<PlatformPackageDto> GetAsync(int id)
        {
            var allPlatformPackageDetails = await _platformPackageRepository.WithDetailsAsync(s => s.PackageProvider);
            var item = allPlatformPackageDetails.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return null; // or throw an exception, or return a default value based on your business logic
            }

            var attachedItems = await _documentsAttachment.WithDetailsAsync();

            var medicalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
            var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medicalSpecializations.ToList());

            var medicalDegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
            var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicalDegrees.ToList());

            var packageFacilities = await _platformPackageFacilityRepository.WithDetailsAsync();
            var facilityById = packageFacilities.Where(f => f.PlatformPackageId == id).ToList();
            var facilityDtos = ObjectMapper.Map<List<PlatformPackageFacility>, List<PlatformPackageFacilityDto>>(facilityById);
            string faciStr = string.Join(",", facilityById.Select(d => d.FacilityName));

            var profilePics = attachedItems.FirstOrDefault(x => x.EntityType == EntityType.Doctor
                                                              && x.EntityId == item.PackageProviderId
                                                              && x.AttachmentType == AttachmentType.ProfilePicture
                                                              && x.IsDeleted == false);

            var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.PackageProviderId).ToList();
            string degStr = string.Join(",", degrees.Select(d => d.DegreeName));

            var specializations = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.PackageProviderId).ToList();
            string expStr = string.Join(",", specializations.Select(e => e.SpecializationName));

            var result = new PlatformPackageDto()
            {
                Id = item.Id,
                PackageName = item.PackageName,
                PackageTitle = item.PackageTitle,
                PackageDescription = item.PackageDescription,
                PackageProviderId = item.PackageProviderId,
                Price = item.Price,
                Reason = item.Reason,
                DoctorName = item.PackageProviderId > 0 ? item.PackageProvider.FullName : "",
                DoctorTitle = item.PackageProvider?.DoctorTitle,
                DoctorTitleName = item.PackageProvider?.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.PackageProvider?.DoctorTitle).ToString() : "n/a",
                DoctorCode = item.PackageProviderId > 0 ? item.PackageProvider.DoctorCode : "",
                DoctorSpecialization = specializations,
                AreaOfExperties = expStr,
                DoctorDegrees = degrees,
                Qualifications = degStr,
                FacilityofPackage=faciStr,
                PackageFacilities = facilityDtos,
                ProfilePic = profilePics?.Path,
                Slug=item.PackageName,

            };

            return result;
        }


        public async Task<List<PlatformPackageDto>> GetListAsync()
        {
            List<PlatformPackageDto>? result = null;

            var allPlatformPackageDetails = await _platformPackageRepository.WithDetailsAsync(s=>s.PackageProvider);
            if (!allPlatformPackageDetails.Any())
            {
                return result;
            }
            result = new List<PlatformPackageDto>();
           
            foreach (var item in allPlatformPackageDetails)
            {
                var packageFacilities = await _platformPackageFacilityRepository.WithDetailsAsync();
                var facilityById = packageFacilities.Where(f => f.PlatformPackageId == item.Id).ToList();
                var facilityDtos = ObjectMapper.Map<List<PlatformPackageFacility>, List<PlatformPackageFacilityDto>>(facilityById);
                string faciStr = string.Join(",", facilityById.Select(d => d.FacilityName));

                result.Add(new PlatformPackageDto()
                {
                    Id = item.Id,
                    PackageName = item.PackageName,
                    PackageTitle = item.PackageTitle,
                    PackageDescription = item.PackageDescription,
                    PackageProviderId = item.PackageProviderId,
                    Price = item.Price,
                    Reason = item.Reason,
                    DoctorName = item.PackageProviderId > 0 ? item.PackageProvider.FullName : "",
                    DoctorTitle = item.PackageProvider?.DoctorTitle,
                    DoctorTitleName = item.PackageProvider?.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.PackageProvider?.DoctorTitle).ToString() : "n/a",
                    DoctorCode = item.PackageProviderId > 0 ? item.PackageProvider.DoctorCode : "",
                    FacilityofPackage = faciStr,
                    Slug=item.PackageName,
                    PackageFacilities = facilityDtos.Where(sp => sp.PlatformPackageId == item.Id).ToList(),
                });
            }
            var resList = result.OrderByDescending(d => d.Id).ToList();
            return resList;
        }

        //public async Task<List<string>> GetListBySlugAsync(string slug)
        //{
        //    var serviceProviders = await _platformPackageRepository.WithDetailsAsync(f => f.PackageProvider);
        //    var serviceProviderList = serviceProviders.Where(p => p.PlatformFacility.Slug == slug).ToList();
        //    var distinctProviders = serviceProviderList.Select(s => s.ProviderOrganizationName).Distinct().ToList();

        //    //var providers = (from sp in serviceProviderList join dp in distinctProviders on sp.ProviderOrganizationName equals dp select sp).ToList();

        //    return distinctProviders; //ObjectMapper.Map<List<ServiceProvider>, List<ServiceProviderDto>>(providers).OrderByDescending(a => a.Id).ToList();

        //}

        public Task<List<PlatformPackageDto>> GetPlatformPackageListByAgentMasterIdAsync(int doctorId)
        {
            throw new NotImplementedException();
        }

        public async Task<PlatformPackageDto> UpdateAsync(PlatformPackageInputDto input)
        {
            var updateItem = ObjectMapper.Map<PlatformPackageInputDto, PlatformPackage>(input);

            var item = await _platformPackageRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<PlatformPackage, PlatformPackageDto>(item);
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}
