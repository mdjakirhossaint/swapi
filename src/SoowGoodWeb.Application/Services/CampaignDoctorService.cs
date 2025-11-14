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
    public class CampaignDoctorService : SoowGoodWebAppService, ICampaignDoctorService
    {
        private readonly IRepository<CampaignDoctor> _campaignDoctorRepository;
        private readonly IRepository<DoctorProfile> _doctorProfileRepository;
        private readonly IRepository<Campaign> _agentMasterRepository;
        private readonly IRepository<DoctorSpecialization> _doctorSpecializationRepository;
        private readonly IRepository<DoctorDegree> _doctorDegreeRepository;
        private readonly IRepository<DocumentsAttachment> _documentsAttachment;
        private readonly IRepository<FinancialSetup> _financialSetup;
        //private readonly ILogger<CampaignDoctorService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public CampaignDoctorService(IRepository<CampaignDoctor>campaignDoctorRepository, IRepository<DoctorProfile> doctorProfileRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<Campaign> agentMasterRepository, IRepository<DoctorSpecialization> doctorSpecializationRepository,IRepository<DoctorDegree> doctorDegreeRepository,
             //ILogger<CampaignDoctorService> logger,
             IRepository<FinancialSetup> financialSetup,
             IRepository<DocumentsAttachment> documentsAttachment)
        {
            _campaignDoctorRepository = campaignDoctorRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _agentMasterRepository = agentMasterRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _doctorDegreeRepository = doctorDegreeRepository;
            _documentsAttachment = documentsAttachment;
            _financialSetup= financialSetup;
            //_logger = logger;
        }
        public async Task<CampaignDoctorDto> CreateAsync(CampaignDoctorInputDto input)
        {
            var result = new CampaignDoctorDto();
            var doctor = await _doctorProfileRepository.GetAsync(d => d.Id == input.DoctorProfileId);
            var newEntity = ObjectMapper.Map<CampaignDoctorInputDto, CampaignDoctor>(input);

            var campaignDoctor = await _campaignDoctorRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();
            result = ObjectMapper.Map<CampaignDoctor, CampaignDoctorDto>(campaignDoctor);
            result.DoctorName = doctor?.FullName;
            return result;//ObjectMapper.Map<DoctorDegree, DoctorDegreeDto>(result);
        }

        public async Task<CampaignDoctorDto> GetAsync(int id)
        {
            var item = await _campaignDoctorRepository.GetAsync(x => x.Id == id);

            return ObjectMapper.Map<CampaignDoctor, CampaignDoctorDto>(item);
        }
        public async Task<List<CampaignDoctorDto>> GetListAsync()
        {
            //var doctors = await _campaignDoctorRepository.GetListAsync();
            //return ObjectMapper.Map<List<CampaignDoctor>, List<CampaignDoctorDto>>(doctors);

            List<CampaignDoctorDto>? result = null;

            var allCampaignDoctorDetails = await _campaignDoctorRepository.WithDetailsAsync(d => d.DoctorProfile);
            if (!allCampaignDoctorDetails.Any())
            {
                return result;
            }
            result = new List<CampaignDoctorDto>();
            foreach (var item in allCampaignDoctorDetails)
            {

                result.Add(new CampaignDoctorDto()
                {
                    Id = item.Id,
                    CampaignId = item.CampaignId,
                    DoctorProfileId = item.DoctorProfileId,
                    DoctorName=item.DoctorProfileId>0?item.DoctorProfile.FullName:"",
                });
            }
            var resList = result.OrderByDescending(d => d.Id).ToList();
            return resList;
        }
        public async Task<List<CampaignDoctorDto>> GetCampaignDoctorListByCampaignIdAsync(int campaignId)
        {
           
            //var doctDegrees = doctors.Where(dd => dd.DoctorProfileId == doctorId).ToList();
            //return ObjectMapper.Map<List<CampaignDoctor>, List<CampaignDoctorDto>>(doctDegrees);

            List<CampaignDoctorDto> list = null;
            try
            {
                var items = await _campaignDoctorRepository.WithDetailsAsync(d => d.DoctorProfile, m => m.Campaign);
                items = items.Where(i => i.CampaignId == campaignId);
                //items = items.Where(p => p.DoctorProfile.IsOnline == true && p.DoctorProfile.IsActive == true).ToList();

                if (!items.Any())
                {
                    return list;
                }

                var medicalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medicalSpecializations.ToList());

                var medicalDegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicalDegrees.ToList());

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 10019 || p.PlatformFacilityId == 10020) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt.HasValue ? vatAmnt / 100 : 0;

                if (items.Any())
                {
                    list = new List<CampaignDoctorDto>();
                    foreach (var item in items)
                    {
                        decimal? campaignDoctorIndFeeAmountWithCharges = 0;
                        decimal? allCampaignDoctorFeeAmountWithCharges = 0;
                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                       && x.EntityId == item.DoctorProfileId
                                                                       && x.AttachmentType == AttachmentType.ProfilePicture
                                                                       && x.IsDeleted == false).FirstOrDefault();
                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.DoctorProfileId).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        if (!string.IsNullOrEmpty(degStr))
                        {
                            degStr = degStr.Remove(degStr.Length - 1);
                        }

                        var specializations = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.DoctorProfileId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in specializations)
                        {
                            expStr = expStr + e.SpecializationName + ",";

                        }

                        if (!string.IsNullOrEmpty(expStr))
                        {
                            expStr = expStr.Remove(expStr.Length - 1);
                        }
                        var relevantFees = fees.Where(f => f.CampaignId == campaignId ).ToList();

                        var isCommonDoctorFee = relevantFees.Where(i => i.PlatformFacilityId == 10019 && i.FacilityEntityID ==null).FirstOrDefault();
                        var isIndDoctorFee = relevantFees.Where(i => i.PlatformFacilityId == 10020 && i.FacilityEntityID == item.DoctorProfileId).FirstOrDefault();
                        if (isIndDoctorFee != null)
                        {
                            var campaignDoctorIndFeeIn = isIndDoctorFee.AmountIn;
                            var campaignDoctorExternalFeeIn=isIndDoctorFee.ExternalAmountIn;
                            decimal? campaignDoctorIndFee = isIndDoctorFee.Amount;
                            decimal? campaignDoctorExternalFee = isIndDoctorFee.ExternalAmount;
                            decimal? campaignDoctorIndProviderAmnt = isIndDoctorFee.ProviderAmount;

                            decimal? campaignDoctorIndAmountTotalCharges = campaignDoctorIndFeeIn == "Percentage" ? ((campaignDoctorIndFee / 100) * campaignDoctorIndProviderAmnt) : campaignDoctorIndFee;
                            decimal? masterDocterExternalAmount= campaignDoctorExternalFeeIn == "Percentage" ? ((campaignDoctorIndFee / 100)* campaignDoctorExternalFee):campaignDoctorExternalFee;
                            decimal ? campaignDoctorIndAmountWithChargesWithVat = (campaignDoctorIndAmountTotalCharges * vatCharge) + campaignDoctorIndAmountTotalCharges;
                            campaignDoctorIndFeeAmountWithCharges = campaignDoctorIndAmountWithChargesWithVat + campaignDoctorIndProviderAmnt+ masterDocterExternalAmount;
                            //individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                        }
                        else  
                        {
                            if (isCommonDoctorFee != null)
                            {
                                var campaignDoctorCommonFeeIn = isCommonDoctorFee.AmountIn;
                                var campaignDoctorExternalFeeIn = isCommonDoctorFee.ExternalAmountIn;
                                decimal? campaignDoctorCommonFee = isCommonDoctorFee.Amount;
                                decimal? campaignDoctorExternalFee = isCommonDoctorFee.ExternalAmount;
                                decimal? campaignDoctorCommonProviderAmnt = isCommonDoctorFee.ProviderAmount;

                                decimal? campaignDoctorCommonAmountTotalCharges = campaignDoctorCommonFeeIn == "Percentage" ? ((campaignDoctorCommonFee / 100) * campaignDoctorCommonProviderAmnt) : campaignDoctorCommonFee;
                                decimal? masterDocterExternalAmount = campaignDoctorExternalFeeIn == "Percentage" ? ((campaignDoctorCommonFee / 100) * campaignDoctorExternalFee) : campaignDoctorExternalFee;
                                decimal? campaignDoctorCommonAmountWithChargesWithVat = (campaignDoctorCommonAmountTotalCharges * vatCharge) + campaignDoctorCommonAmountTotalCharges;
                                allCampaignDoctorFeeAmountWithCharges = campaignDoctorCommonAmountWithChargesWithVat + campaignDoctorCommonProviderAmnt + masterDocterExternalAmount;
                            }
                            

                        }

                        list.Add(new CampaignDoctorDto()
                        {
                            Id = item.Id,
                            CampaignId = item.CampaignId,
                            
                            DoctorProfileId = item.DoctorProfileId,
                            DoctorName = item.DoctorProfileId > 0 ? item.DoctorProfile.FullName : "",
                            DoctorCode=item.DoctorProfileId>0?item.DoctorProfile.DoctorCode:"",
                            DoctorTitle = item.DoctorProfile?.DoctorTitle,
                            DoctorTitleName = item.DoctorProfile?.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorProfile?.DoctorTitle).ToString() : "n/a",
                            DoctorSpecialization = specializations,
                            AreaOfExperties = expStr,
                            DoctorDegrees = degrees,
                            Qualifications = degStr,
                            IsActive = item.DoctorProfile?.IsActive,
                            IsOnline = item.DoctorProfile?.IsOnline,
                            
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = (campaignDoctorIndFeeAmountWithCharges > 0)? Math.Round((decimal)campaignDoctorIndFeeAmountWithCharges, 2) : (allCampaignDoctorFeeAmountWithCharges > 0
                                                       ? Math.Round((decimal)allCampaignDoctorFeeAmountWithCharges, 2) : 0)


                    }) ;
                    }
                }

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while getting master doctor list for CampaignId: {CampaignId}", campaignId);
                // Optionally, you can rethrow the exception or handle it accordingly
                throw;
            }
            return list;
        }
        public async Task<List<CampaignDoctorDto>> GetListByDoctorIdAsync(int doctorId)
        {
            List<CampaignDoctorDto> list = null;
            var items = await _campaignDoctorRepository.WithDetailsAsync(d => d.DoctorProfile);
            items = items.Where(i => i.DoctorProfileId == doctorId);
            if (items.Any())
            {
                list = new List<CampaignDoctorDto>();
                foreach (var item in items)
                {
                    list.Add(new CampaignDoctorDto()
                    {
                        Id = item.Id,
                        DoctorName = item.DoctorProfile?.FullName,
                       
                    });
                }
            }

            return list;
        }
        public async Task<CampaignDoctorDto> UpdateAsync(CampaignDoctorInputDto input)
        {
            var updateItem = ObjectMapper.Map<CampaignDoctorInputDto, CampaignDoctor>(input);

            var item = await _campaignDoctorRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<CampaignDoctor, CampaignDoctorDto>(item);
        }

        public async Task DeleteAsync(long id)
        {
            await _campaignDoctorRepository.DeleteAsync(d => d.Id == id);
        }

       

        //public async Task<List<DoctorProfileDto>> GetListAsync()
        //{
        //    List<DoctorProfileDto> list = null;
        //    var items = await _doctorProfileRepository.WithDetailsAsync(p => p.District);
        //    if (items.Any())
        //    {
        //        list = new List<DoctorProfileDto>();
        //        foreach (var item in items)
        //        {
        //            list.Add(new DoctorProfileDto()
        //            {
        //                Id = item.Id,
        //                Name = item.Name,
        //                Description = item.Description,
        //                DistrictId = item.DistrictId,
        //                DistrictName = item.District?.Name,
        //                CivilSubDivisionId = item.CivilSubDivisionId,
        //                EmSubDivisionId = item.EmSubDivisionId,
        //            });
        //        }
        //    }

        //    return list;
        //}
        //public async Task<List<QuarterDto>> GetListByDistrictAsync(int id)
        //{
        //    List<QuarterDto> list = null;
        //    var items = await repository.WithDetailsAsync(p => p.District);
        //    items = items.Where(i => i.DistrictId == id);
        //    if (items.Any())
        //    {
        //        list = new List<QuarterDto>();
        //        foreach (var item in items)
        //        {
        //            list.Add(new QuarterDto()
        //            {
        //                Id = item.Id,
        //                Name = item.Name,
        //                Description = item.Description,
        //                DistrictId = item.DistrictId,
        //                DistrictName = item.District?.Name,
        //                CivilSubDivisionId = item.CivilSubDivisionId,
        //                EmSubDivisionId = item.EmSubDivisionId,
        //            });
        //        }
        //    }

        //    return list;
        //}

        //public async Task<int> GetCountAsync()
        //{
        //    return (await quarterRepository.GetListAsync()).Count;
        //}
        //public async Task<List<QuarterDto>> GetSortedListAsync(FilterModel filterModel)
        //{
        //    var quarters = await quarterRepository.WithDetailsAsync();
        //    quarters = quarters.Skip(filterModel.Offset)
        //                    .Take(filterModel.Limit);
        //    return ObjectMapper.Map<List<Quarter>, List<QuarterDto>>(quarters.ToList());
        //}
        ////public async Task<int> GetCountBySDIdAsync(Guid? civilSDId, Guid? emSDId)
        //public async Task<int> GetCountBySDIdAsync(Guid? sdId)
        //{
        //    var quarters = await quarterRepository.WithDetailsAsync();
        //    //if (civilSDId != null && emSDId != null)
        //    //{
        //    //    quarters = quarters.Where(q => q.CivilSubDivisionId == civilSDId && q.EmSubDivisionId == emSDId);
        //    //}
        //    if (sdId != null)
        //    {
        //        quarters = quarters.Where(q => q.CivilSubDivisionId == sdId || q.EmSubDivisionId == sdId);
        //    }
        //    //else if (emSDId != null)
        //    //{
        //    //    quarters = quarters.Where(q => q.EmSubDivisionId == emSDId);
        //    //}
        //    return quarters.Count();
        //}
        ////public async Task<List<QuarterDto>> GetSortedListBySDIdAsync(Guid? civilSDId, Guid? emSDId, FilterModel filterModel)
        //public async Task<List<QuarterDto>> GetSortedListBySDIdAsync(Guid? sdId, FilterModel filterModel)
        //{
        //    var quarters = await quarterRepository.WithDetailsAsync();
        //    //if (civilSDId != null && emSDId != null)
        //    //{
        //    //    quarters = quarters.Where(q => q.CivilSubDivisionId == civilSDId && q.EmSubDivisionId == emSDId);
        //    //}
        //    //else if (civilSDId != null)
        //    //{
        //    if (sdId != null)
        //        quarters = quarters.Where(q => q.CivilSubDivisionId == sdId || q.EmSubDivisionId == sdId);
        //    //}
        //    //else if (emSDId != null)
        //    //{
        //    //    quarters = quarters.Where(q => q.EmSubDivisionId == emSDId);
        //    //}
        //    quarters = quarters.Skip(filterModel.Offset)
        //                    .Take(filterModel.Limit);
        //    return ObjectMapper.Map<List<Quarter>, List<QuarterDto>>(quarters.ToList());
        //}
        //public async Task<List<QuarterDto>> GetListBySDIdAsync(Guid? sdId)
        //{
        //    var quarters = await quarterRepository.WithDetailsAsync();
        //    if (sdId != null)
        //    {
        //        quarters = quarters.Where(q => q.CivilSubDivisionId == sdId || q.EmSubDivisionId == sdId);
        //    }
        //    return ObjectMapper.Map<List<Quarter>, List<QuarterDto>>(quarters.ToList());
        //}
    }
}
