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
    public class MasterDoctorService : SoowGoodWebAppService, IMasterDoctorService
    {
        private readonly IRepository<MasterDoctor> _masterDoctorRepository;
        private readonly IRepository<DoctorProfile> _doctorProfileRepository;
        private readonly IRepository<AgentMaster> _agentMasterRepository;
        private readonly IRepository<DoctorSpecialization> _doctorSpecializationRepository;
        private readonly IRepository<DoctorDegree> _doctorDegreeRepository;
        private readonly IRepository<DocumentsAttachment> _documentsAttachment;
        private readonly IRepository<FinancialSetup> _financialSetup;
        //private readonly ILogger<MasterDoctorService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public MasterDoctorService(IRepository<MasterDoctor>masterDoctorRepository, IRepository<DoctorProfile> doctorProfileRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<AgentMaster> agentMasterRepository, IRepository<DoctorSpecialization> doctorSpecializationRepository,IRepository<DoctorDegree> doctorDegreeRepository,
             //ILogger<MasterDoctorService> logger,
             IRepository<FinancialSetup> financialSetup,
             IRepository<DocumentsAttachment> documentsAttachment)
        {
            _masterDoctorRepository = masterDoctorRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _agentMasterRepository = agentMasterRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _doctorDegreeRepository = doctorDegreeRepository;
            _documentsAttachment = documentsAttachment;
            _financialSetup= financialSetup;
            //_logger = logger;
        }
        public async Task<MasterDoctorDto> CreateAsync(MasterDoctorInputDto input)
        {
            var result = new MasterDoctorDto();
            var doctor = await _doctorProfileRepository.GetAsync(d => d.Id == input.DoctorProfileId);
            var newEntity = ObjectMapper.Map<MasterDoctorInputDto, MasterDoctor>(input);

            var masterDoctor = await _masterDoctorRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();
            result = ObjectMapper.Map<MasterDoctor, MasterDoctorDto>(masterDoctor);
            result.DoctorName = doctor?.FullName;
            return result;//ObjectMapper.Map<DoctorDegree, DoctorDegreeDto>(result);
        }

        public async Task<MasterDoctorDto> GetAsync(int id)
        {
            var item = await _masterDoctorRepository.GetAsync(x => x.Id == id);

            return ObjectMapper.Map<MasterDoctor, MasterDoctorDto>(item);
        }
        public async Task<List<MasterDoctorDto>> GetListAsync()
        {
            //var doctors = await _masterDoctorRepository.GetListAsync();
            //return ObjectMapper.Map<List<MasterDoctor>, List<MasterDoctorDto>>(doctors);

            List<MasterDoctorDto>? result = null;

            var allMasterDoctorDetails = await _masterDoctorRepository.WithDetailsAsync(d => d.DoctorProfile);
            if (!allMasterDoctorDetails.Any())
            {
                return result;
            }
            result = new List<MasterDoctorDto>();
            foreach (var item in allMasterDoctorDetails)
            {

                result.Add(new MasterDoctorDto()
                {
                    Id = item.Id,
                    AgentMasterId = item.AgentMasterId,
                    DoctorProfileId = item.DoctorProfileId,
                    DoctorName=item.DoctorProfileId>0?item.DoctorProfile.FullName:"",
                });
            }
            var resList = result.OrderByDescending(d => d.Id).ToList();
            return resList;
        }
        public async Task<List<MasterDoctorDto>> GetMasterDoctorListByAgentMasterIdAsync(int masterId)
        {
           
            //var doctDegrees = doctors.Where(dd => dd.DoctorProfileId == doctorId).ToList();
            //return ObjectMapper.Map<List<MasterDoctor>, List<MasterDoctorDto>>(doctDegrees);

            List<MasterDoctorDto> list = null;
            try
            {
                var items = await _masterDoctorRepository.WithDetailsAsync(d => d.DoctorProfile, m => m.AgentMaster);
                items = items.Where(i => i.AgentMasterId == masterId);
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
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 20 || p.PlatformFacilityId == 21) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt.HasValue ? vatAmnt / 100 : 0;

                if (items.Any())
                {
                    list = new List<MasterDoctorDto>();
                    foreach (var item in items)
                    {
                        decimal? masterDoctorIndFeeAmountWithCharges = 0;
                        decimal? allMasterDoctorFeeAmountWithCharges = 0;
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
                        var relevantFees = fees.Where(f => f.AgentMasterId == masterId).ToList();

                        var isCommonDoctorFee = relevantFees.Where(i => i.PlatformFacilityId == 20 && i.FacilityEntityID ==null && i.AgentMasterId==masterId ).FirstOrDefault();
                        var isIndDoctorFee = relevantFees.Where(i => i.PlatformFacilityId == 21 && i.FacilityEntityID == item.DoctorProfileId).FirstOrDefault();
                        if (isIndDoctorFee != null)
                        {
                            var masterDoctorIndFeeIn = isIndDoctorFee.AmountIn;
                            var masterDoctorExternalFeeIn=isIndDoctorFee.ExternalAmountIn;
                            decimal? masterDoctorIndFee = isIndDoctorFee.Amount;
                            decimal? masterDoctorExternalFee = isIndDoctorFee.ExternalAmount;
                            decimal? masterDoctorIndProviderAmnt = isIndDoctorFee.ProviderAmount;

                            decimal? masterDoctorIndAmountTotalCharges = masterDoctorIndFeeIn == "Percentage" ? ((masterDoctorIndFee / 100) * masterDoctorIndProviderAmnt) : masterDoctorIndFee;
                            decimal? masterDocterExternalAmount= masterDoctorExternalFeeIn == "Percentage" ? ((masterDoctorIndFee / 100)* masterDoctorExternalFee):masterDoctorExternalFee;
                            decimal ? masterDoctorIndAmountWithChargesWithVat = (masterDoctorIndAmountTotalCharges * vatCharge) + masterDoctorIndAmountTotalCharges;
                            masterDoctorIndFeeAmountWithCharges = masterDoctorIndAmountWithChargesWithVat + masterDoctorIndProviderAmnt+ masterDocterExternalAmount;
                            //individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                        }
                        else  
                        {
                            if (isCommonDoctorFee != null)
                            {
                                var masterDoctorCommonFeeIn = isCommonDoctorFee.AmountIn;
                                var masterDoctorExternalFeeIn = isCommonDoctorFee.ExternalAmountIn;
                                decimal? masterDoctorCommonFee = isCommonDoctorFee.Amount;
                                decimal? masterDoctorExternalFee = isCommonDoctorFee.ExternalAmount;
                                decimal? masterDoctorCommonProviderAmnt = isCommonDoctorFee.ProviderAmount;

                                decimal? masterDoctorCommonAmountTotalCharges = masterDoctorCommonFeeIn == "Percentage" ? ((masterDoctorCommonFee / 100) * masterDoctorCommonProviderAmnt) : masterDoctorCommonFee;
                                decimal? masterDocterExternalAmount = masterDoctorExternalFeeIn == "Percentage" ? ((masterDoctorCommonFee / 100) * masterDoctorExternalFee) : masterDoctorExternalFee;
                                decimal? masterDoctorCommonAmountWithChargesWithVat = (masterDoctorCommonAmountTotalCharges * vatCharge) + masterDoctorCommonAmountTotalCharges;
                                allMasterDoctorFeeAmountWithCharges = masterDoctorCommonAmountWithChargesWithVat + masterDoctorCommonProviderAmnt + masterDocterExternalAmount;
                            }
                            

                        }

                        list.Add(new MasterDoctorDto()
                        {
                            Id = item.Id,
                            AgentMasterId = item.AgentMasterId,
                            DoctorProfileId = item.DoctorProfileId,
                            DoctorName = item.DoctorProfileId > 0 && item.DoctorProfile != null ? item.DoctorProfile.FullName : string.Empty,
                            DoctorCode = item.DoctorProfileId > 0 && item.DoctorProfile != null ? item.DoctorProfile.DoctorCode : string.Empty,
                            DoctorTitle = item.DoctorProfile?.DoctorTitle,
                            DoctorTitleName = item.DoctorProfile?.DoctorTitle > 0
                         ? Utilities.Utility.GetDisplayName(item.DoctorProfile.DoctorTitle).ToString()
                         : "N/A",
                            DoctorSpecialization = specializations,
                            AreaOfExperties = expStr,
                            DoctorDegrees = degrees,
                            Qualifications = degStr,
                            IsActive = item.DoctorProfile?.IsActive ?? false,
                            IsOnline = item.DoctorProfile?.IsOnline ?? false,
                            ProfilePic = profilePics?.Path ?? string.Empty,
                            DisplayInstantFeeAsAgent = masterDoctorIndFeeAmountWithCharges > 0
                                 ? Math.Round((decimal)masterDoctorIndFeeAmountWithCharges, 2)
                                 : (allMasterDoctorFeeAmountWithCharges > 0
                                     ? Math.Round((decimal)allMasterDoctorFeeAmountWithCharges, 2)
                                     : 0)
                        });

                    }
                }

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while getting master doctor list for AgentMasterId: {MasterId}", masterId);
                // Optionally, you can rethrow the exception or handle it accordingly
                throw;
            }
            return list;
        }
        public async Task<List<MasterDoctorDto>> GetListByDoctorIdAsync(int doctorId)
        {
            List<MasterDoctorDto> list = null;
            var items = await _masterDoctorRepository.WithDetailsAsync(d => d.DoctorProfile);
            items = items.Where(i => i.DoctorProfileId == doctorId);
            if (items.Any())
            {
                list = new List<MasterDoctorDto>();
                foreach (var item in items)
                {
                    list.Add(new MasterDoctorDto()
                    {
                        Id = item.Id,
                        DoctorName = item.DoctorProfile?.FullName,
                       
                    });
                }
            }

            return list;
        }
        public async Task<MasterDoctorDto> UpdateAsync(MasterDoctorInputDto input)
        {
            var updateItem = ObjectMapper.Map<MasterDoctorInputDto, MasterDoctor>(input);

            var item = await _masterDoctorRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<MasterDoctor, MasterDoctorDto>(item);
        }

        public async Task DeleteAsync(long id)
        {
            await _masterDoctorRepository.DeleteAsync(d => d.Id == id);
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
