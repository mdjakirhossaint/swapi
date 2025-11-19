using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp.Account;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using static Volo.Abp.UI.Navigation.DefaultMenuNames.Application;
using SoowGoodWeb.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http;
using SoowGoodWeb.Core.GenericModels;
using SoowGood.Core.Service;
using SoowGoodWeb.Application.Service.Services.TokenService;

namespace SoowGoodWeb.Services
{
    public class DoctorProfileService : SoowGoodWebAppService, IDoctorProfileService
    {
        private readonly IRepository<DoctorProfile> _doctorProfileRepository;
        private readonly IRepository<DoctorDegree> _doctorDegreeRepository;
        private readonly IRepository<DoctorSpecialization> _doctorSpecializationRepository;
        private readonly IRepository<DoctorSchedule> _doctorScheduleRepository;
        private readonly IRepository<DocumentsAttachment> _documentsAttachment;
        private readonly IRepository<FinancialSetup> _financialSetup;
        private readonly IRepository<DoctorFeesSetup> _doctorFeesSetup;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public DoctorProfileService(IRepository<DoctorProfile> doctorProfileRepository
                                    , IUnitOfWorkManager unitOfWorkManager
                                    , IRepository<DoctorDegree> doctorDegreeRepository
                                    , IRepository<DoctorSpecialization> doctorSpecializationRepository
                                    , IRepository<DoctorSchedule> doctorScheduleRepository
                                    , IRepository<DocumentsAttachment> documentsAttachment
                                    , IRepository<FinancialSetup> financialSetup
                                    , IRepository<DoctorFeesSetup> doctorFeesSetup)
        {
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _doctorDegreeRepository = doctorDegreeRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _doctorScheduleRepository = doctorScheduleRepository;
            _documentsAttachment = documentsAttachment;
            _financialSetup = financialSetup;
            _doctorFeesSetup = doctorFeesSetup;
        }
        /// <summary>
        /// Creating Doctor Profiles with basic info
        /// </summary>
        /// <param name="input">
        //FullName 
         //DoctorTitle 
         //DateOfBirth 
         //Gender 
         //MaritalStatus 
         //Address 
         //City 
         //ZipCode 
         //Country 
         //MobileNo 
         //Email 
         //IdentityNumber 
         //BMDCRegNo 
         //BMDCRegExpiryDate 
        /// </param>
        /// <returns></returns>

        public async Task<DoctorProfileDto> CreateAsync(DoctorProfileInputDto input)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var result = new DoctorProfileDto();
            try
            {

                var totalDoctors = await _doctorProfileRepository.GetListAsync();
                var count = totalDoctors.Count();
                input.DateOfBirth = Convert.ToDateTime(input.DateOfBirth).AddDays(1);
                input.BMDCRegExpiryDate = Convert.ToDateTime(input.BMDCRegExpiryDate).AddDays(1);
                var date = DateTime.Now;
                input.DoctorCode = "SGD" + date.ToString("yyyyMMdd") + (count + 1);
                var newEntity = ObjectMapper.Map<DoctorProfileInputDto, DoctorProfile>(input);

                var doctorProfile = await _doctorProfileRepository.InsertAsync(newEntity);

                //await _unitOfWorkManager.Current.SaveChangesAsync();
                result = ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(doctorProfile);
                //return result;
            }
            catch (Exception ex)
            {
                return result;
            }
            return result;
        }
        public async Task<DoctorProfileDto> GetAsync(int id)
        {
            //var item = await _doctorProfileRepository.GetAsync(x => x.Id == id);

            //return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);

            var item = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, sp => sp.Speciality, d => d.DoctorSpecialization);

            var profile = item.FirstOrDefault(item => item.Id == id);

            var result = profile != null ? ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(profile) : null;

            return result;
        }
        public async Task<DoctorProfileDto> GetDoctorByProfileIdAsync(long id)
        {

            DoctorProfileDto result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);

                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.Where(p => p.Id == id && p.IsActive == true).FirstOrDefault();
                if (profiles == null)
                {
                    return result;
                }

                result = new DoctorProfileDto();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                var sfees = financialSetups.Where(p => (p.PlatformFacilityId == 1 || p.PlatformFacilityId == 2 || p.PlatformFacilityId == 4 || p.PlatformFacilityId == 5) && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;


                var doctorFees = await _doctorFeesSetup.WithDetailsAsync(d => d.DoctorSchedule.DoctorProfile);

                try
                {
                    decimal? instantfeeAsPatient = 0;
                    decimal? instantfeeAsAgent = 0;
                    decimal? individualInstantfeeAsPatient = 0;
                    decimal? individualInstantfeeAsAgent = 0;
                    decimal? scheduledPtnChamberfee = 0;
                    decimal? scheduledAgntChamberfee = 0;
                    decimal? scheduledPtnOnlinefee = 0;
                    decimal? scheduledAgntOnlinefee = 0;
                    decimal? realTimePtnAmountWithCharges = 0;
                    decimal? realTimeAgntAmountWithCharges = 0;
                    decimal? realTimeIndPtnAmountWithCharges = 0;
                    decimal? realTimeIndAgntAmountWithCharges = 0;

                    var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                    && x.EntityId == profiles.Id
                                                                    && x.AttachmentType == AttachmentType.ProfilePicture
                                                                    && x.IsDeleted == false).FirstOrDefault();

                    if (profiles.IsOnline == true)
                    {

                        var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == profiles.Id).FirstOrDefault();
                        var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == profiles.Id).FirstOrDefault();
                        if (isIndPntFee != null)
                        {
                            var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                            decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                            decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                            decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                            decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                            realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                            individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                        }
                        else
                        {
                            var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                            decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                            decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                            decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                            decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                            realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                            instantfeeAsPatient = realTimePtnAmountWithCharges;
                        }

                        if (isIndAgntFee != null)
                        {
                            var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                            decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                            var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                            decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                            decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                            decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                            decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                            decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                            decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                            realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                            individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                        }
                        else
                        {
                            var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                            decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                            var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                            decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                            decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                            decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                            decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                            decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                            decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                            realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                            instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                        }
                    }
                    var docChamberfeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Chamber && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                    if (docChamberfeees != null)
                    {
                        decimal? scf = docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == profiles.Id)?.TotalFee > 0 ? docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == profiles.Id)?.TotalFee : 0;

                        decimal? scfPtnChamberAmountWithCharges = 0;
                        decimal? scfAgntChamberAmountWithCharges = 0;

                        var scfPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.AmountIn;
                        decimal? scfPatientcharge = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.Amount;

                        var scfAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmountIn;
                        decimal? scfAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmount;

                        var scfAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.AmountIn;
                        decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.Amount;


                        decimal? scfPtnCharge = scfPatientchargeIn == "Percentage" ? ((scfPatientcharge / 100) * scf) : scfPatientcharge;
                        decimal? scfPtnAmountWithChargesWithVat = (scfPtnCharge * vatCharge) + scfPtnCharge;
                        scfPtnChamberAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                        scheduledPtnChamberfee = scfPtnChamberAmountWithCharges;


                        decimal? scfAgntExCharge = scfAgentchargeExIn == "Percentage" ? ((scfAgentchargeEx / 100) * scf) : scfAgentchargeEx;
                        decimal? scfAgntCharge = scfAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * scf) : sofAgentcharge;

                        decimal? scfTotalSgExCharge = scfAgntExCharge + scfAgntCharge;
                        decimal? scfAgntAmountWithChargesWithVat = (scfTotalSgExCharge * vatCharge) + scfTotalSgExCharge;
                        scfAgntChamberAmountWithCharges = scfAgntAmountWithChargesWithVat + scf;
                        scheduledAgntChamberfee = scfAgntChamberAmountWithCharges;

                    }
                    var docOnlinefeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Online && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                    if (docOnlinefeees != null)
                    {
                        decimal? sof = docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == profiles.Id)?.TotalFee > 0 ? docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == profiles.Id)?.TotalFee : 0;


                        decimal? sofPtnOnlineAmountWithCharges = 0;
                        decimal? sofAgntOnlineAmountWithCharges = 0;

                        var sofPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.AmountIn;
                        decimal? sofPatientcharge = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.Amount;

                        var sofAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmountIn;
                        decimal? sofAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmount;

                        var sofAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.AmountIn;
                        decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.Amount;


                        decimal? sofPtnCharge = sofPatientchargeIn == "Percentage" ? ((sofPatientcharge / 100) * sof) : sofPatientcharge;
                        decimal? sofPtnAmountWithChargesWithVat = (sofPtnCharge * vatCharge) + sofPtnCharge;
                        sofPtnOnlineAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                        scheduledPtnOnlinefee = sofPtnOnlineAmountWithCharges;


                        decimal? sofAgntExCharge = sofAgentchargeExIn == "Percentage" ? ((sofAgentchargeEx / 100) * sof) : sofAgentchargeEx;
                        decimal? sofAgntCharge = sofAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * sof) : sofAgentcharge;

                        decimal? sofTotalSgExCharge = sofAgntExCharge + sofAgntCharge;
                        decimal? sofAgntAmountWithChargesWithVat = (sofTotalSgExCharge * vatCharge) + sofTotalSgExCharge;
                        sofAgntOnlineAmountWithCharges = sofAgntAmountWithChargesWithVat + sof;
                        scheduledAgntOnlinefee = sofAgntOnlineAmountWithCharges;
                    }

                    var degrees = doctorDegrees.Where(d => d.DoctorProfileId == profiles.Id).ToList();
                    string degStr = string.Empty;
                    foreach (var d in degrees)
                    {
                        degStr = degStr + d.DegreeName + ",";
                    }
                    if (degStr.Length != 0)
                    {
                        degStr = degStr.Remove(degStr.Length - 1);

                    }

                    var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == profiles.Id && sp.SpecialityId == profiles.SpecialityId).ToList();
                    string expStr = string.Empty;
                    foreach (var e in experties)
                    {
                        expStr = expStr + e.SpecializationName + ",";
                    }
                    if (expStr.Length != 0)
                    {
                        expStr = expStr.Remove(expStr.Length - 1);

                    }

                    result.Id = profiles.Id;
                    result.Degrees = degrees;
                    result.Qualifications = degStr;
                    result.SpecialityId = profiles.SpecialityId;
                    result.SpecialityName = profiles.SpecialityId > 0 ? profiles.Speciality?.SpecialityName : "n/a";
                    result.DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == profiles.Id && sp.SpecialityId == profiles.SpecialityId).ToList();
                    result.AreaOfExperties = expStr;
                    result.FullName = profiles.FullName;
                    result.DoctorTitle = profiles.DoctorTitle;
                    result.DoctorTitleName = profiles.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(profiles.DoctorTitle).ToString() : "n/a";
                    result.MaritalStatus = profiles.MaritalStatus;
                    result.MaritalStatusName = profiles.MaritalStatus > 0 ? ((MaritalStatus)profiles.MaritalStatus).ToString() : "n/a";
                    result.City = profiles.City;
                    result.ZipCode = profiles.ZipCode;
                    result.Country = profiles.Country;
                    result.IdentityNumber = profiles.IdentityNumber;
                    result.BMDCRegNo = profiles.BMDCRegNo;
                    result.BMDCRegExpiryDate = profiles.BMDCRegExpiryDate;
                    result.Email = profiles.Email;
                    result.MobileNo = profiles.MobileNo;
                    result.DateOfBirth = profiles.DateOfBirth;
                    result.Gender = profiles.Gender;
                    result.GenderName = profiles.Gender > 0 ? ((Gender)profiles.Gender).ToString() : "n/a";
                    result.Address = profiles.Address;
                    result.ProfileRole = "Doctor";
                    result.IsActive = profiles.IsActive;
                    result.UserId = profiles.UserId;
                    result.IsOnline = profiles.IsOnline;
                    result.profileStep = profiles.profileStep;
                    result.createFrom = profiles.createFrom;
                    result.DoctorCode = profiles.DoctorCode;
                    result.ProfilePic = profilePics?.Path;
                    result.Expertise= profiles.Expertise;
                    result.DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2);
                    result.DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2);
                    result.DisplayScheduledPatientChamberFee = scheduledPtnChamberfee > 0 ? Math.Round((decimal)scheduledPtnChamberfee, 2) : 0;
                    result.DisplayScheduledPatientOnlineFee = scheduledPtnOnlinefee > 0 ? Math.Round((decimal)scheduledPtnOnlinefee, 2) : 0;
                    result.DisplayScheduledAgentChamberFee = scheduledAgntChamberfee > 0 ? Math.Round((decimal)scheduledAgntChamberfee, 2) : 0;
                    result.DisplayScheduledAgentOnlineFee = scheduledAgntOnlinefee > 0 ? Math.Round((decimal)scheduledAgntOnlinefee, 2) : 0;
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }
        public async Task<DoctorProfileDto> GetByUserIdAsync(Guid userId)
        {
            var doctorProfiles = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, sp => sp.Speciality, d => d.DoctorSpecialization);
            var item = doctorProfiles.FirstOrDefault(x => x.UserId == userId);
            var result = item != null ? ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item) : null;
            return result;//ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
        }
        public async Task<DoctorProfileDto> GetDoctorDetailsByAdminAsync(int id)
        {
            DoctorProfileDto? result = null;
            var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
            if (!profileWithDetails.Any())
            {
                return result;
            }
            var profile = profileWithDetails.FirstOrDefault(profile => profile.Id == id);
            if (profile == null) { return result; }
            result = new DoctorProfileDto();
            var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
            var degrees = medicaldegrees.Where(i => i.DoctorProfileId == profile.Id).ToList();
            var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(degrees);


            var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
            var specializations = medcalSpecializations.Where(i => i.DoctorProfileId == profile.Id).ToList();
            var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(specializations);

            result.Id = profile.Id;
            result.Degrees = doctorDegrees;
            result.DoctorSpecialization = doctorSpecializations;
            result.FullName = profile.FullName;
            result.DoctorTitle = profile.DoctorTitle;
            result.DoctorTitleName = profile.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(profile.DoctorTitle).ToString() : "n/a";
            result.MaritalStatus = profile.MaritalStatus;
            result.MaritalStatusName = profile.MaritalStatus > 0 ? ((MaritalStatus)profile.MaritalStatus).ToString() : "n/a";
            result.City = profile.City;
            result.ZipCode = profile.ZipCode;
            result.Country = profile.Country;
            result.IdentityNumber = profile.IdentityNumber;
            result.BMDCRegNo = profile.BMDCRegNo;
            result.BMDCRegExpiryDate = profile.BMDCRegExpiryDate;
            result.Email = profile.Email;
            result.MobileNo = profile.MobileNo;
            result.DateOfBirth = profile.DateOfBirth;
            result.Gender = profile.Gender;
            result.GenderName = profile.Gender > 0 ? ((Gender)profile.Gender).ToString() : "n/a";
            result.Address = profile.Address;
            result.ProfileRole = "Doctor";
            result.IsActive = profile.IsActive;
            result.UserId = profile.UserId;
            result.IsOnline = profile.IsOnline;
            result.profileStep = profile.profileStep;
            result.createFrom = profile.createFrom;
            result.DoctorCode = profile.DoctorCode;
            result.SpecialityId = profile.SpecialityId;
            result.SpecialityName = profile.SpecialityId > 0 ? profile.Speciality.SpecialityName : "n/a";

            return result;
        }
        public async Task<List<DoctorProfileDto>> GetListAsync()
        {

            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);

                //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
                if (!profileWithDetails.Any())
                {
                    return result;
                }

                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();

                //profiles = (from doctors in profiles
                //            join schedule in schedules on doctors.Id equals schedule.DoctorProfileId
                //            select doctors).Distinct().ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                //&& (p.ProviderAmount == 0 || p.ProviderAmount == null)
                var sfees = financialSetups.Where(p => (p.PlatformFacilityId == 1 || p.PlatformFacilityId == 2 || p.PlatformFacilityId == 4 || p.PlatformFacilityId == 5) && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;


                var doctorFees = await _doctorFeesSetup.WithDetailsAsync(d => d.DoctorSchedule.DoctorProfile);
                //profiles = profiles.Skip(filterModel.Offset)
                //                   .Take(filterModel.Limit).ToList();

                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? scheduledPtnChamberfee = 0;
                        decimal? scheduledAgntChamberfee = 0;
                        decimal? scheduledPtnOnlinefee = 0;
                        decimal? scheduledAgntOnlinefee = 0;
                        decimal? realTimePtnAmountWithCharges = 0;
                        decimal? realTimeAgntAmountWithCharges = 0;
                        decimal? realTimeIndPtnAmountWithCharges = 0;
                        decimal? realTimeIndAgntAmountWithCharges = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        if (item.IsOnline == true)
                        {

                            var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            if (isIndPntFee != null)
                            {
                                var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                                decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                                decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                                decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                                decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                                realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                                individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                            }
                            else
                            {
                                var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                                decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                                decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                                decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                                decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                                realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                                instantfeeAsPatient = realTimePtnAmountWithCharges;
                            }

                            if (isIndAgntFee != null)
                            {
                                var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                                decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                                var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                                decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                                decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                                decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                                decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                                decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                                decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                                realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                                individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                            }
                            else
                            {
                                var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                                decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                                var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                                decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                                decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                                decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                                decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                                decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                                decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                                realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                                instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                            }
                        }
                        //else
                        //{
                        var docChamberfeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Chamber && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docChamberfeees != null)
                        {
                            decimal? scf = docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee > 0 ? docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee : 0;

                            decimal? scfPtnChamberAmountWithCharges = 0;
                            decimal? scfAgntChamberAmountWithCharges = 0;

                            var scfPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.AmountIn;
                            decimal? scfPatientcharge = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.Amount;

                            var scfAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? scfAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmount;

                            var scfAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.Amount;


                            decimal? scfPtnCharge = scfPatientchargeIn == "Percentage" ? ((scfPatientcharge / 100) * scf) : scfPatientcharge;
                            decimal? scfPtnAmountWithChargesWithVat = (scfPtnCharge * vatCharge) + scfPtnCharge;
                            scfPtnChamberAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                            scheduledPtnChamberfee = scfPtnChamberAmountWithCharges;


                            decimal? scfAgntExCharge = scfAgentchargeExIn == "Percentage" ? ((scfAgentchargeEx / 100) * scf) : scfAgentchargeEx;
                            decimal? scfAgntCharge = scfAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * scf) : sofAgentcharge;

                            decimal? scfTotalSgExCharge = scfAgntExCharge + scfAgntCharge;
                            decimal? scfAgntAmountWithChargesWithVat = (scfTotalSgExCharge * vatCharge) + scfTotalSgExCharge;
                            scfAgntChamberAmountWithCharges = scfAgntAmountWithChargesWithVat + scf;
                            scheduledAgntChamberfee = scfAgntChamberAmountWithCharges;

                        }
                        var docOnlinefeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Online && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docOnlinefeees != null)
                        {
                            decimal? sof = docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee > 0 ? docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee : 0;


                            decimal? sofPtnOnlineAmountWithCharges = 0;
                            decimal? sofAgntOnlineAmountWithCharges = 0;

                            var sofPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.AmountIn;
                            decimal? sofPatientcharge = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.Amount;

                            var sofAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? sofAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmount;

                            var sofAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.Amount;


                            decimal? sofPtnCharge = sofPatientchargeIn == "Percentage" ? ((sofPatientcharge / 100) * sof) : sofPatientcharge;
                            decimal? sofPtnAmountWithChargesWithVat = (sofPtnCharge * vatCharge) + sofPtnCharge;
                            sofPtnOnlineAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                            scheduledPtnOnlinefee = sofPtnOnlineAmountWithCharges;


                            decimal? sofAgntExCharge = sofAgentchargeExIn == "Percentage" ? ((sofAgentchargeEx / 100) * sof) : sofAgentchargeEx;
                            decimal? sofAgntCharge = sofAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * sof) : sofAgentcharge;

                            decimal? sofTotalSgExCharge = sofAgntExCharge + sofAgntCharge;
                            decimal? sofAgntAmountWithChargesWithVat = (sofTotalSgExCharge * vatCharge) + sofTotalSgExCharge;
                            sofAgntOnlineAmountWithCharges = sofAgntAmountWithChargesWithVat + sof;
                            scheduledAgntOnlinefee = sofAgntOnlineAmountWithCharges;
                        }
                        //}

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        degStr = degStr.Remove(degStr.Length - 1);

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }

                        expStr = expStr.Remove(expStr.Length - 1);

                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2),
                            DisplayScheduledPatientChamberFee = scheduledPtnChamberfee > 0 ? Math.Round((decimal)scheduledPtnChamberfee, 2) : 0,
                            DisplayScheduledPatientOnlineFee = scheduledPtnOnlinefee > 0 ? Math.Round((decimal)scheduledPtnOnlinefee, 2) : 0,
                            DisplayScheduledAgentChamberFee = scheduledAgntChamberfee > 0 ? Math.Round((decimal)scheduledAgntChamberfee, 2) : 0,
                            DisplayScheduledAgentOnlineFee = scheduledAgntOnlinefee > 0 ? Math.Round((decimal)scheduledAgntOnlinefee, 2) : 0
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }


            return result;
        }
        public async Task<List<DoctorProfileDto>> GetAllActiveDoctorListAsync()
        {
            List<DoctorProfileDto> result = null;
            var profileWithDetails = await _doctorProfileRepository.GetListAsync(s => s.IsActive == true);

            return ObjectMapper.Map<List<DoctorProfile>, List<DoctorProfileDto>>(profileWithDetails.ToList());
        }

        //public async Task<ApiResponse<List<DoctorProfileDto>>> GetAllActiveDoctorsAsync()
        //{
        //    var apiResponse = new ApiResponse<List<DoctorProfileDto>>();
        //    // Get the HTTP Request from IHttpContextAccessor
        //    var authorizationToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

        //    var (userId, matchKey, status, message) = await _tokenService.GetUserIdFromAuthorizationToken(authorizationToken);

        //    if (userId == Guid.Empty)
        //    {
        //        apiResponse.results = new();
        //        apiResponse.message = message;
        //        apiResponse.status = StatusResponseMessage.failed;
        //        apiResponse.status_code = StatusCodes.Status400BadRequest;
        //        return apiResponse;
        //    }
        //    List<DoctorProfileDto> result = null;
        //    var profileWithDetails = await _doctorProfileRepository.GetListAsync(s => s.IsActive == true);

        //    var mappedObject = ObjectMapper.Map<List<DoctorProfile>, List<DoctorProfileDto>>(profileWithDetails.ToList());
        //    apiResponse.results = mappedObject;
        //    return apiResponse;
        //}

        public async Task<List<DoctorProfileDto>> GetCurrentlyOnlineDoctorListAsync()
        {
            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
                if (!profileWithDetails.Any())
                {
                    return result;
                }
                var profiles = profileWithDetails.Where(p => p.IsOnline == true && p.IsActive == true).ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;

                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? realTimePtnAmountWithCharges = 0;
                        decimal? realTimeAgntAmountWithCharges = 0;
                        decimal? realTimeIndPtnAmountWithCharges = 0;
                        decimal? realTimeIndAgntAmountWithCharges = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        degStr = degStr.Remove(degStr.Length - 1);

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }

                        expStr = expStr.Remove(expStr.Length - 1);

                        var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == item.Id).FirstOrDefault();
                        var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == item.Id).FirstOrDefault();
                        if (isIndPntFee != null)
                        {
                            var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                            decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                            decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                            decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                            decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                            realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                            individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                        }
                        else
                        {
                            var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                            decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                            decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                            decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                            decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                            realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                            instantfeeAsPatient = realTimePtnAmountWithCharges;
                        }

                        if (isIndAgntFee != null)
                        {
                            var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                            decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                            var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                            decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                            decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                            decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                            decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                            decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                            decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                            realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                            individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                        }
                        else
                        {
                            var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                            decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                            var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                            decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                            decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                            decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                            decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                            decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                            decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                            realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                            instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                        }

                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2)
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result.OrderBy(f => f.DisplayInstantFeeAsPatient).ToList();
        }

        /// <summary>
        /// This function used for getting currently online doctor list
        /// if doctor click the online togle from the doctors dashboard
        /// the portal will show the live online doctor list
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task<List<DoctorProfileDto>> GetLiveOnlineDoctorListAsync(FilterModel filterModel)
        {
            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
                if (!profileWithDetails.Any())
                {
                    return result;
                }
                var profiles = profileWithDetails.Where(p => p.IsOnline == true && p.IsActive == true).ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;

                //profiles = profiles.OrderByDescending

                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? realTimePtnAmountWithCharges = 0;
                        decimal? realTimeAgntAmountWithCharges = 0;
                        decimal? realTimeIndPtnAmountWithCharges = 0;
                        decimal? realTimeIndAgntAmountWithCharges = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        degStr = degStr.Remove(degStr.Length - 1);

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }

                        expStr = expStr.Remove(expStr.Length - 1);

                        var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == item.Id).FirstOrDefault();
                        var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == item.Id).FirstOrDefault();
                        if (isIndPntFee != null)
                        {
                            var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                            decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                            decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                            decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                            decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                            realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                            individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                        }
                        else
                        {
                            var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                            decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                            decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                            decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                            decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                            realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                            instantfeeAsPatient = realTimePtnAmountWithCharges;
                        }

                        if (isIndAgntFee != null)
                        {
                            var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                            decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                            var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                            decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                            decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                            decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                            decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                            decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                            decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                            realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                            individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                        }
                        else
                        {
                            var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                            var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                            decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                            var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                            decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                            decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                            decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                            decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                            decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                            decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                            realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                            instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                        }

                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2)
                        });
                    }
                    result = result.OrderBy(f => f.DisplayInstantFeeAsPatient).ToList();
                    result = result.Skip(filterModel.Offset)
                   .Take(filterModel.Limit).ToList();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;//.OrderBy(f => f.DisplayInstantFeeAsPatient).ToList();
        }

        /// <summary>
        /// This function used for generate doctor card list with details data of doctors
        /// this function generate the list with filter parameters (specialization, consultancy type, name)
        /// this fuction maintained the pagination for the doctor list
        /// the doctor list connected to the individual doctor fee management for patient or agent
        /// </summary>
        /// <param name="doctorFilterModel"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        /// 
        public async Task<List<DoctorProfileDto>> GetDoctorListFilterAsync(DataFilterModel? doctorFilterModel, FilterModel filterModel)
        {
            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
                if (!profileWithDetails.Any())
                {
                    return result;
                }

                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();

                //profiles = (from doctors in profiles
                //            join schedule in schedules on doctors.Id equals schedule.DoctorProfileId
                //            select doctors).Distinct().ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                var sfees = financialSetups.Where(p => (p.PlatformFacilityId == 1 || p.PlatformFacilityId == 2 || p.PlatformFacilityId == 4 || p.PlatformFacilityId == 5) && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;


                var doctorFees = await _doctorFeesSetup.WithDetailsAsync(d => d.DoctorSchedule.DoctorProfile);
                //profiles = profiles.Skip(filterModel.Offset)
                //                   .Take(filterModel.Limit).ToList();
                if (!string.IsNullOrEmpty(doctorFilterModel?.name))
                {
                    profiles = profiles.Where(p => p.FullName.ToLower().Contains(doctorFilterModel.name.ToLower().Trim())).ToList();
                }

                if (doctorFilterModel?.specializationId > 0)
                {
                    profiles = (from t1 in profiles
                                join t2 in doctorSpecializations.Where(c => c.SpecializationId == doctorFilterModel.specializationId)
                                on t1.Id equals t2.DoctorProfileId
                                select t1).ToList();
                }

                if (doctorFilterModel?.consultancyType > 0)
                {
                    if (doctorFilterModel?.consultancyType == ConsultancyType.Instant)
                    {
                        profiles = profiles.Where(p => p.IsOnline == true).ToList();
                    }
                    else
                    {
                        schedules = schedules.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType);
                        profiles = (from t1 in profiles
                                    join t2 in schedules //.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType)
                                    on t1.Id equals t2.DoctorProfileId
                                    select t1).Distinct().ToList();
                    }
                }
                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? scheduledPtnChamberfee = 0;
                        decimal? scheduledAgntChamberfee = 0;
                        decimal? scheduledPtnOnlinefee = 0;
                        decimal? scheduledAgntOnlinefee = 0;
                        decimal? realTimePtnAmountWithCharges = 0;
                        decimal? realTimeAgntAmountWithCharges = 0;
                        decimal? realTimeIndPtnAmountWithCharges = 0;
                        decimal? realTimeIndAgntAmountWithCharges = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        if (item.IsOnline == true)
                        {

                            var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            if (isIndPntFee != null)
                            {
                                var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                                decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                                decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                                decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                                decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                                realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                                individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                            }
                            else
                            {
                                var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                                decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                                decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                                decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                                decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                                realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                                instantfeeAsPatient = realTimePtnAmountWithCharges;
                            }

                            if (isIndAgntFee != null)
                            {
                                var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                                decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                                var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                                decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                                decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                                decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                                decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                                decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                                decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                                realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                                individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                            }
                            else
                            {
                                var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                                decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                                var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                                decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                                decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                                decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                                decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                                decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                                decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                                realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                                instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                            }
                        }
                        var docChamberfeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Chamber && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docChamberfeees != null)
                        {
                            decimal? scf = docChamberfeees
        .Where(d => d.DoctorSchedule?.DoctorProfileId == item.Id && d.AppointmentType == AppointmentType.New)
        .Select(d => d.TotalFee)
        .DefaultIfEmpty(0)
        .Min();

                            decimal? scfPtnChamberAmountWithCharges = 0;
                            decimal? scfAgntChamberAmountWithCharges = 0;

                            var scfPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.AmountIn;
                            decimal? scfPatientcharge = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.Amount;

                            var scfAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? scfAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmount;

                            var scfAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.Amount;


                            decimal? scfPtnCharge = scfPatientchargeIn == "Percentage" ? ((scfPatientcharge / 100) * scf) : scfPatientcharge;
                            decimal? scfPtnAmountWithChargesWithVat = (scfPtnCharge * vatCharge) + scfPtnCharge;
                            scfPtnChamberAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                            scheduledPtnChamberfee = scfPtnChamberAmountWithCharges;


                            decimal? scfAgntExCharge = scfAgentchargeExIn == "Percentage" ? ((scfAgentchargeEx / 100) * scf) : scfAgentchargeEx;
                            decimal? scfAgntCharge = scfAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * scf) : sofAgentcharge;

                            decimal? scfTotalSgExCharge = scfAgntExCharge + scfAgntCharge;
                            decimal? scfAgntAmountWithChargesWithVat = (scfTotalSgExCharge * vatCharge) + scfTotalSgExCharge;
                            scfAgntChamberAmountWithCharges = scfAgntAmountWithChargesWithVat + scf;
                            scheduledAgntChamberfee = scfAgntChamberAmountWithCharges;

                        }
                        var docOnlinefeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Online && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docOnlinefeees != null)
                        {
                            decimal? sof = docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee > 0 ? docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee : 0;


                            decimal? sofPtnOnlineAmountWithCharges = 0;
                            decimal? sofAgntOnlineAmountWithCharges = 0;

                            var sofPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.AmountIn;
                            decimal? sofPatientcharge = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.Amount;

                            var sofAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? sofAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmount;

                            var sofAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.Amount;


                            decimal? sofPtnCharge = sofPatientchargeIn == "Percentage" ? ((sofPatientcharge / 100) * sof) : sofPatientcharge;
                            decimal? sofPtnAmountWithChargesWithVat = (sofPtnCharge * vatCharge) + sofPtnCharge;
                            sofPtnOnlineAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                            scheduledPtnOnlinefee = sofPtnOnlineAmountWithCharges;


                            decimal? sofAgntExCharge = sofAgentchargeExIn == "Percentage" ? ((sofAgentchargeEx / 100) * sof) : sofAgentchargeEx;
                            decimal? sofAgntCharge = sofAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * sof) : sofAgentcharge;

                            decimal? sofTotalSgExCharge = sofAgntExCharge + sofAgntCharge;
                            decimal? sofAgntAmountWithChargesWithVat = (sofTotalSgExCharge * vatCharge) + sofTotalSgExCharge;
                            sofAgntOnlineAmountWithCharges = sofAgntAmountWithChargesWithVat + sof;
                            scheduledAgntOnlinefee = sofAgntOnlineAmountWithCharges;
                        }

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        degStr = degStr.Remove(degStr.Length - 1);

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }

                        expStr = expStr.Remove(expStr.Length - 1);

                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2),
                            DisplayScheduledPatientChamberFee = scheduledPtnChamberfee > 0 ? Math.Round((decimal)scheduledPtnChamberfee, 2) : 0,
                            DisplayScheduledPatientOnlineFee = scheduledPtnOnlinefee > 0 ? Math.Round((decimal)scheduledPtnOnlinefee, 2) : 0,
                            DisplayScheduledAgentChamberFee = scheduledAgntChamberfee > 0 ? Math.Round((decimal)scheduledAgntChamberfee, 2) : 0,
                            DisplayScheduledAgentOnlineFee = scheduledAgntOnlinefee > 0 ? Math.Round((decimal)scheduledAgntOnlinefee, 2) : 0
                        });
                    }
                    result = result.Skip(filterModel.Offset)
                                       .Take(filterModel.Limit).ToList();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// **** this function used for mobile app
        /// This function used for getting currently online doctor list
        /// if doctor click the online togle from the doctors dashboard
        /// the portal will show the live online doctor list
        /// </summary>
        /// <param name="doctorFilterModel"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        /// 
        public async Task<List<DoctorProfileDto>> GetDoctorListFilterMobileAppAsync(DataFilterModel? doctorFilterModel, FilterModel filterModel)
        {
            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
                if (!profileWithDetails.Any())
                {
                    return result;
                }

                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();

                //profiles = (from doctors in profiles
                //            join schedule in schedules on doctors.Id equals schedule.DoctorProfileId
                //            select doctors).Distinct().ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.Where(p => (p.PlatformFacilityId == 3 || p.PlatformFacilityId == 6) && p.ProviderAmount >= 0 && p.IsActive == true).ToList();
                var sfees = financialSetups.Where(p => (p.PlatformFacilityId == 1 || p.PlatformFacilityId == 2 || p.PlatformFacilityId == 4 || p.PlatformFacilityId == 5) && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;


                var doctorFees = await _doctorFeesSetup.WithDetailsAsync(d => d.DoctorSchedule.DoctorProfile);

                if (!string.IsNullOrEmpty(doctorFilterModel?.name))
                {
                    profiles = profiles.Where(p => p.FullName.ToLower().Contains(doctorFilterModel.name.ToLower().Trim())).ToList();
                }

                if (doctorFilterModel?.specializationId > 0)
                {
                    profiles = (from t1 in profiles
                                join t2 in doctorSpecializations.Where(c => c.SpecializationId == doctorFilterModel.specializationId)
                                on t1.Id equals t2.DoctorProfileId
                                select t1).ToList();
                }

                if (doctorFilterModel?.consultancyType > 0)
                {
                    if (doctorFilterModel?.consultancyType == ConsultancyType.Instant)
                    {
                        profiles = profiles.Where(p => p.IsOnline == true).ToList();
                    }
                    else
                    {
                        schedules = schedules.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType);
                        profiles = (from t1 in profiles
                                    join t2 in schedules //.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType)
                                    on t1.Id equals t2.DoctorProfileId
                                    select t1).Distinct().ToList();
                    }
                }
                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? scheduledPtnChamberfee = 0;
                        decimal? scheduledAgntChamberfee = 0;
                        decimal? scheduledPtnOnlinefee = 0;
                        decimal? scheduledAgntOnlinefee = 0;
                        decimal? realTimePtnAmountWithCharges = 0;
                        decimal? realTimeAgntAmountWithCharges = 0;
                        decimal? realTimeIndPtnAmountWithCharges = 0;
                        decimal? realTimeIndAgntAmountWithCharges = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        if (item.IsOnline == true)
                        {

                            var isIndPntFee = fees.Where(i => i.PlatformFacilityId == 3 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            var isIndAgntFee = fees.Where(i => i.PlatformFacilityId == 6 && i.FacilityEntityID == item.Id).FirstOrDefault();
                            if (isIndPntFee != null)
                            {
                                var realtimeIndPatientchargeIn = isIndPntFee.AmountIn;
                                decimal? realtimeIndPatientcharge = isIndPntFee.Amount;
                                decimal? realtimeIndPatientProviderAmnt = isIndPntFee.ProviderAmount;

                                decimal? realTimeIndPtnAmountTotalCharges = realtimeIndPatientchargeIn == "Percentage" ? ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) : realtimeIndPatientcharge;
                                decimal? realTimeIndPtnAmountWithChargesWithVat = (realTimeIndPtnAmountTotalCharges * vatCharge) + realTimeIndPtnAmountTotalCharges;
                                realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                                individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                            }
                            else
                            {
                                var allPntFee = fees.Where(a => a.PlatformFacilityId == 3 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePatientchargeIn = allPntFee != null ? allPntFee.AmountIn : null;
                                decimal? realtimePatientcharge = allPntFee != null ? allPntFee.Amount : 0;
                                decimal? realtimePatientProviderAmnt = allPntFee != null ? allPntFee.ProviderAmount : 0;

                                decimal? realTimePtnAmountTotalCharges = realtimePatientchargeIn == "Percentage" ? ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) : realtimePatientcharge;
                                decimal? realTimePtnAmountWithChargesWithVat = (realTimePtnAmountTotalCharges * vatCharge) + realTimePtnAmountTotalCharges;
                                realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                                instantfeeAsPatient = realTimePtnAmountWithCharges;
                            }

                            if (isIndAgntFee != null)
                            {
                                var realtimeIndPlAgntchargeIn = isIndAgntFee.AmountIn;
                                decimal? realtimeIndPlAgntcharge = isIndAgntFee.Amount;
                                var realtimeIndAgentchargeIn = isIndAgntFee.ExternalAmountIn;
                                decimal? realtimeIndAgentcharge = isIndAgntFee.ExternalAmount;
                                decimal? realtimeIndPlAgntProviderAmnt = isIndAgntFee.ProviderAmount;

                                decimal? realTimeIndPlAgntAmountTotalCharges = realtimeIndPlAgntchargeIn == "Percentage" ? ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndPlAgntcharge;
                                decimal? realTimeIndAgntAmountTotalCharges = realtimeIndAgentchargeIn == "Percentage" ? ((realtimeIndAgentcharge / 100) * realtimeIndPlAgntProviderAmnt) : realtimeIndAgentcharge;

                                decimal? realTimeIndPlExtAmountTotalCharges = realTimeIndPlAgntAmountTotalCharges + realTimeIndAgntAmountTotalCharges;
                                decimal? realTimeIndAgntAmountWithChargesWithVat = (realTimeIndPlExtAmountTotalCharges * vatCharge) + realTimeIndPlExtAmountTotalCharges;
                                realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;

                                individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realtimeIndPlAgntProviderAmnt;
                            }
                            else
                            {
                                var allAgntFee = fees.Where(a => a.PlatformFacilityId == 6 && a.FacilityEntityID == null)?.FirstOrDefault();
                                var realtimePlAgntchargeIn = allAgntFee != null ? allAgntFee.AmountIn : null;
                                decimal? realtimePlAgntcharge = allAgntFee != null ? allAgntFee.Amount : 0;
                                var realtimeAgentchargeIn = allAgntFee != null ? allAgntFee.ExternalAmountIn : null;
                                decimal? realtimeAgentcharge = allAgntFee != null ? allAgntFee.ExternalAmount : 0;
                                decimal? realtimePlAgntProviderAmnt = allAgntFee != null ? allAgntFee.ProviderAmount : 0;

                                decimal? realTimePlAgntAmountTotalCharges = realtimePlAgntchargeIn == "Percentage" ? ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) : realtimePlAgntcharge;
                                decimal? realTimeAgntAmountTotalCharges = realtimeAgentchargeIn == "Percentage" ? ((realtimeAgentcharge / 100) * realtimePlAgntProviderAmnt) : realtimeAgentcharge;

                                decimal? totalRealtimeSgExAmnts = realTimePlAgntAmountTotalCharges + realTimeAgntAmountTotalCharges;
                                decimal? realTimeAgntAmountWithChargesWithVat = (totalRealtimeSgExAmnts * vatCharge) + totalRealtimeSgExAmnts;
                                realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;

                                instantfeeAsAgent = realTimeAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                            }
                        }
                        var docChamberfeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Chamber && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docChamberfeees != null)
                        {
                            decimal? scf = docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee > 0 ? docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee : 0;

                            decimal? scfPtnChamberAmountWithCharges = 0;
                            decimal? scfAgntChamberAmountWithCharges = 0;

                            var scfPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.AmountIn;
                            decimal? scfPatientcharge = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.Amount;

                            var scfAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? scfAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.ExternalAmount;

                            var scfAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 4)?.FirstOrDefault()?.Amount;


                            decimal? scfPtnCharge = scfPatientchargeIn == "Percentage" ? ((scfPatientcharge / 100) * scf) : scfPatientcharge;
                            decimal? scfPtnAmountWithChargesWithVat = (scfPtnCharge * vatCharge) + scfPtnCharge;
                            scfPtnChamberAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                            scheduledPtnChamberfee = scfPtnChamberAmountWithCharges;


                            decimal? scfAgntExCharge = scfAgentchargeExIn == "Percentage" ? ((scfAgentchargeEx / 100) * scf) : scfAgentchargeEx;
                            decimal? scfAgntCharge = scfAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * scf) : sofAgentcharge;

                            decimal? scfTotalSgExCharge = scfAgntExCharge + scfAgntCharge;
                            decimal? scfAgntAmountWithChargesWithVat = (scfTotalSgExCharge * vatCharge) + scfTotalSgExCharge;
                            scfAgntChamberAmountWithCharges = scfAgntAmountWithChargesWithVat + scf;
                            scheduledAgntChamberfee = scfAgntChamberAmountWithCharges;

                        }
                        var docOnlinefeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Online && f.TotalFee >= 0).OrderBy(a => a.TotalFee).ToList();
                        if (docOnlinefeees != null)
                        {
                            decimal? sof = docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee > 0 ? docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee : 0;


                            decimal? sofPtnOnlineAmountWithCharges = 0;
                            decimal? sofAgntOnlineAmountWithCharges = 0;

                            var sofPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.AmountIn;
                            decimal? sofPatientcharge = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.Amount;

                            var sofAgentchargeExIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? sofAgentchargeEx = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.ExternalAmount;

                            var sofAgentchargeIn = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.AmountIn;
                            decimal? sofAgentcharge = sfees.Where(a => a.PlatformFacilityId == 5)?.FirstOrDefault()?.Amount;


                            decimal? sofPtnCharge = sofPatientchargeIn == "Percentage" ? ((sofPatientcharge / 100) * sof) : sofPatientcharge;
                            decimal? sofPtnAmountWithChargesWithVat = (sofPtnCharge * vatCharge) + sofPtnCharge;
                            sofPtnOnlineAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                            scheduledPtnOnlinefee = sofPtnOnlineAmountWithCharges;


                            decimal? sofAgntExCharge = sofAgentchargeExIn == "Percentage" ? ((sofAgentchargeEx / 100) * sof) : sofAgentchargeEx;
                            decimal? sofAgntCharge = sofAgentchargeIn == "Percentage" ? ((sofAgentcharge / 100) * sof) : sofAgentcharge;

                            decimal? sofTotalSgExCharge = sofAgntExCharge + sofAgntCharge;
                            decimal? sofAgntAmountWithChargesWithVat = (sofTotalSgExCharge * vatCharge) + sofTotalSgExCharge;
                            sofAgntOnlineAmountWithCharges = sofAgntAmountWithChargesWithVat + sof;
                            scheduledAgntOnlinefee = sofAgntOnlineAmountWithCharges;
                        }

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        if (degStr.Length != 0)
                        {
                            degStr = degStr.Remove(degStr.Length - 1);

                        }
                        

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }
                        if (expStr.Length != 0)
                        {
                            expStr = expStr.Remove(expStr.Length - 1);

                        }


                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? Utilities.Utility.GetDisplayName(item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2),
                            DisplayScheduledPatientChamberFee = scheduledPtnChamberfee > 0 ? Math.Round((decimal)scheduledPtnChamberfee, 2) : 0,
                            DisplayScheduledPatientOnlineFee = scheduledPtnOnlinefee > 0 ? Math.Round((decimal)scheduledPtnOnlinefee, 2) : 0,
                            DisplayScheduledAgentChamberFee = scheduledAgntChamberfee > 0 ? Math.Round((decimal)scheduledAgntChamberfee, 2) : 0,
                            DisplayScheduledAgentOnlineFee = scheduledAgntOnlinefee > 0 ? Math.Round((decimal)scheduledAgntOnlinefee, 2) : 0
                        });
                    }
                    result = result.Skip(filterModel.Offset)
                   .Take(filterModel.Limit).ToList();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }
        public async Task<int> GetDoctorsCountByFiltersAsync(DataFilterModel? doctorFilterModel)
        {
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
                if (!profileWithDetails.Any())
                {
                    return 0;
                }
                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();

                //profiles = (from doctors in profiles
                //            join schedule in schedules on doctors.Id equals schedule.DoctorProfileId
                //            select doctors).Distinct().ToList();

                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());

                profiles = (from doctors in profiles
                            join degree in doctorDegrees on doctors.Id equals degree.DoctorProfileId
                            select doctors).Distinct().ToList();

                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                profiles = (from doctors in profiles
                            join experties in doctorSpecializations on doctors.Id equals experties.DoctorProfileId
                            select doctors).Distinct().ToList();


                if (!string.IsNullOrEmpty(doctorFilterModel?.name))
                {
                    profiles = profiles.Where(p => p.FullName.ToLower().Contains(doctorFilterModel.name.ToLower().Trim())).ToList();
                }

                if (doctorFilterModel?.specializationId > 0)
                {
                    profiles = (from t1 in profiles
                                join t2 in doctorSpecializations.Where(c => c.SpecializationId == doctorFilterModel.specializationId)
                                on t1.Id equals t2.DoctorProfileId
                                select t1).ToList();
                }

                if (doctorFilterModel?.consultancyType > 0)
                {
                    if (doctorFilterModel?.consultancyType == ConsultancyType.Instant)
                    {
                        profiles = profiles.Where(p => p.IsOnline == true).ToList();
                    }
                    else
                    {
                        schedules = schedules.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType);
                        profiles = (from t1 in profiles
                                    join t2 in schedules
                                    on t1.Id equals t2.DoctorProfileId
                                    select t1).Distinct().ToList();
                    }
                }

                return profiles.Count;
            }
            catch (Exception e)
            {
                return 0;
            }

        }
        public async Task<DoctorProfileDto> GetByUserNameAsync(string userName)
        {
            var dProfiles = await _doctorProfileRepository.WithDetailsAsync(s => s.Speciality);
            var item = dProfiles.Where(x => x.MobileNo == userName).FirstOrDefault();

            return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
        }


        public async Task<DoctorProfileDto> GetByUserEmailAsync(string emailAddress)
        {
            var dProfiles = await _doctorProfileRepository.WithDetailsAsync(s => s.Speciality);
            var item = dProfiles.Where(x => x.MobileNo == emailAddress).FirstOrDefault();

            return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
        }



        public async Task<List<DoctorProfileDto>> GetListDoctorListByAdminAsync()
        {
            List<DoctorProfileDto>? result = null;
            try 
            {
                
                var allProfile = await _doctorProfileRepository.GetListAsync();
                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());
                if (!allProfile.Any())
                {
                    return result;
                }

                result = new List<DoctorProfileDto>();
                foreach (var item in allProfile)
                {
                    result.Add(new DoctorProfileDto()
                    {
                        Id = item.Id,
                        FullName = item.FullName,
                        Email = item.Email,
                        MobileNo = item.MobileNo,
                        DateOfBirth = item.DateOfBirth,
                        Gender = item.Gender,
                        GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                        DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                        Address = item.Address,
                        ProfileRole = "Doctor",
                        IsActive = item.IsActive,

                    });
                }
                
            }
            catch(Exception ex) 
            {
            }
            return result.OrderByDescending(d => d.Id).ToList();
        }
        public async Task<List<DoctorProfileDto>> GetDoctorListFilterByAdminAsync(DataFilterModel? doctorFilterModel, FilterModel filterModel)
        {
            List<DoctorProfileDto> result = null;
            try
            {
                var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);

                //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
                if (!profileWithDetails.Any())
                {
                    return result;
                }

                var schedules = await _doctorScheduleRepository.WithDetailsAsync(d => d.DoctorProfile);
                var profiles = profileWithDetails.ToList();

                //profiles = (from doctors in profiles
                //            join schedule in schedules on doctors.Id equals schedule.DoctorProfileId
                //            select doctors).Distinct().ToList();

                result = new List<DoctorProfileDto>();
                var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);

                var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


                var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
                var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

                var attachedItems = await _documentsAttachment.WithDetailsAsync();

                var financialSetups = await _financialSetup.WithDetailsAsync();
                var fees = financialSetups.OrderBy(a => a.ProviderAmount).Where(p => p.ProviderAmount > 0 && p.IsActive == true).ToList();
                var sfees = financialSetups.OrderBy(a => a.ProviderAmount).Where(p => (p.ProviderAmount == 0 || p.ProviderAmount == null) && p.IsActive == true).ToList();
                decimal? vatAmnt = fees.Where(a => a.Vat > 0)?.FirstOrDefault()?.Vat;
                decimal? vatCharge = vatAmnt / 100;


                var doctorFees = await _doctorFeesSetup.WithDetailsAsync(d => d.DoctorSchedule.DoctorProfile);
                //profiles = profiles.Skip(filterModel.Offset)
                //                   .Take(filterModel.Limit).ToList();
                if (!string.IsNullOrEmpty(doctorFilterModel?.name))
                {
                    profiles = profiles.Where(p => (p.FullName != null && p.FullName.ToLower().Contains(doctorFilterModel.name.ToLower().Trim())) || (p.MobileNo != null && p.MobileNo.ToLower().Contains(doctorFilterModel.name.ToLower().Trim()))).ToList();
                }

                if (doctorFilterModel?.isActive != null)
                {
                    if (doctorFilterModel?.isActive == true)
                    {
                        profiles = profiles.Where(p => p.IsActive == true).ToList();
                    }
                    else
                    {
                        profiles = profiles.Where(p => p.IsActive == false).ToList();
                    }
                }


                if (doctorFilterModel?.specializationId > 0)
                {
                    profiles = (from t1 in profiles
                                join t2 in doctorSpecializations.Where(c => c.SpecializationId == doctorFilterModel.specializationId)
                                on t1.Id equals t2.DoctorProfileId
                                select t1).ToList();
                }

                if (doctorFilterModel?.consultancyType > 0)
                {
                    if (doctorFilterModel?.consultancyType == ConsultancyType.Instant)
                    {
                        profiles = profiles.Where(p => p.IsOnline == true).ToList();
                    }
                    else
                    {
                        schedules = schedules.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType);
                        profiles = (from t1 in profiles
                                    join t2 in schedules //.Where(c => c.ConsultancyType == doctorFilterModel.consultancyType)
                                    on t1.Id equals t2.DoctorProfileId
                                    select t1).Distinct().ToList();
                    }
                }
                try
                {
                    foreach (var item in profiles)
                    {
                        decimal? instantfeeAsPatient = 0;
                        decimal? instantfeeAsAgent = 0;
                        decimal? individualInstantfeeAsPatient = 0;
                        decimal? individualInstantfeeAsAgent = 0;
                        decimal? scheduledChamberfee = 0;
                        decimal? scheduledOnlinefee = 0;

                        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
                                                                        && x.EntityId == item.Id
                                                                        && x.AttachmentType == AttachmentType.ProfilePicture
                                                                        && x.IsDeleted == false).FirstOrDefault();

                        if (item.IsOnline == true)
                        {
                            decimal? realTimePtnAmountWithCharges = 0;
                            decimal? realTimeAgntAmountWithCharges = 0;

                            decimal? realTimePlAgntAmountWithCharges = 0; ///

                            var realtimePatientchargeIn = fees.Where(a => a.PlatformFacilityId == 3 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.AmountIn;
                            decimal? realtimePatientcharge = fees.Where(a => a.PlatformFacilityId == 3 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.Amount;
                            decimal? realtimePatientProviderAmnt = fees.Where(p => p.PlatformFacilityId == 3 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;
                            if (realtimePatientchargeIn == "Percentage")
                            {
                                decimal? realTimePtnAmountWithChargesWithVat = ((realtimePatientcharge / 100) * realtimePatientProviderAmnt) * vatCharge;
                                realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                            }
                            else if (realtimePatientchargeIn == "Flat")
                            {
                                decimal? realTimePtnAmountWithChargesWithVat = (realtimePatientcharge) * vatCharge;
                                realTimePtnAmountWithCharges = realTimePtnAmountWithChargesWithVat + realtimePatientProviderAmnt;
                            }

                            var realtimePlAgntchargeIn = fees.Where(a => a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.AmountIn;
                            decimal? realtimePlAgntcharge = fees.Where(a => a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.Amount;
                            decimal? realtimePlAgntProviderAmnt = fees.Where(p => p.PlatformFacilityId == 6 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;
                            if (realtimePlAgntchargeIn == "Percentage")
                            {
                                decimal? realTimePlAgntAmountWithChargesWithVat = ((realtimePlAgntcharge / 100) * realtimePlAgntProviderAmnt) * vatCharge;
                                realTimePlAgntAmountWithCharges = realTimePlAgntAmountWithChargesWithVat;
                            }
                            else if (realtimePlAgntchargeIn == "Flat")
                            {
                                decimal? realTimePlAgntAmountWithChargesWithVat = (realtimePlAgntcharge) * vatCharge;
                                realTimePlAgntAmountWithCharges = realTimePlAgntAmountWithChargesWithVat;
                            }

                            var realtimeAgentchargeIn = fees.Where(a => a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? realtimeAgentcharge = fees.Where(a => a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.ExternalAmount;
                            decimal? realtimeAgentProviderAmnt = fees.Where(p => p.PlatformFacilityId == 6 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;

                            if (realtimeAgentchargeIn == "Percentage")
                            {
                                decimal? realTimeAgntAmountWithChargesWithVat = ((realtimeAgentcharge / 100) * realtimeAgentProviderAmnt) * vatCharge;
                                realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;
                            }
                            else if (realtimeAgentchargeIn == "Flat")
                            {
                                decimal? realTimeAgntAmountWithChargesWithVat = (realtimeAgentcharge) * vatCharge;
                                realTimeAgntAmountWithCharges = realTimeAgntAmountWithChargesWithVat;
                            }

                            instantfeeAsPatient = realTimePtnAmountWithCharges;
                            instantfeeAsAgent = realTimeAgntAmountWithCharges + realTimePlAgntAmountWithCharges + realtimePlAgntProviderAmnt;

                            decimal? realTimeIndPtnAmountWithCharges = 0;
                            decimal? realTimeIndAgntAmountWithCharges = 0;
                            decimal? realTimeIndPlAgntAmountWithCharges = 0;

                            var realtimeIndPatientchargeIn = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 3 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.AmountIn;
                            decimal? realtimeIndPatientcharge = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 3 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.Amount;
                            decimal? realtimeIndPatientProviderAmnt = fees.Where(p => p.FacilityEntityID == item.Id && p.PlatformFacilityId == 3 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;

                            if (realtimeIndPatientchargeIn == "Percentage")
                            {
                                decimal? realTimeIndPtnAmountWithChargesWithVat = ((realtimeIndPatientcharge / 100) * realtimeIndPatientProviderAmnt) * vatCharge;
                                realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;
                            }
                            else if (realtimeIndPatientchargeIn == "Flat")
                            {
                                decimal? realTimeIndPtnAmountWithChargesWithVat = (realtimeIndPatientcharge) * vatCharge;
                                realTimeIndPtnAmountWithCharges = realTimeIndPtnAmountWithChargesWithVat + realtimeIndPatientProviderAmnt;

                            }

                            var realtimeIndPlAgntchargeIn = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.AmountIn;
                            decimal? realtimeIndPlAgntcharge = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.Amount;
                            decimal? realtimeIndPlAgntProviderAmnt = fees.Where(p => p.FacilityEntityID == item.Id && p.PlatformFacilityId == 6 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;

                            if (realtimeIndPlAgntchargeIn == "Percentage")
                            {
                                decimal? realTimeIndPlAgntAmountWithChargesWithVat = ((realtimeIndPlAgntcharge / 100) * realtimeIndPlAgntProviderAmnt) * vatCharge;
                                realTimeIndPlAgntAmountWithCharges = realTimeIndPlAgntAmountWithChargesWithVat;
                            }
                            else if (realtimeIndPlAgntchargeIn == "Flat")
                            {
                                decimal? realTimeIndPlAgntAmountWithChargesWithVat = (realtimeIndPlAgntcharge) * vatCharge;
                                realTimeIndPlAgntAmountWithCharges = realTimeIndPlAgntAmountWithChargesWithVat;
                            }

                            var realtimeIndAgentchargeIn = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.ExternalAmountIn;
                            decimal? realtimeIndAgentcharge = fees.Where(a => a.FacilityEntityID == item.Id && a.PlatformFacilityId == 6 && a.ProviderAmount != null && a.IsActive == true)?.FirstOrDefault()?.ExternalAmount;
                            decimal? realtimeIndAgentProviderAmnt = fees.Where(p => p.FacilityEntityID == item.Id && p.PlatformFacilityId == 6 && p.ProviderAmount != null && p.IsActive == true).FirstOrDefault()?.ProviderAmount;

                            if (realtimeIndAgentchargeIn == "Percentage")
                            {

                                decimal? realTimeIndAgntAmountWithChargesWithVat = ((realtimeIndAgentcharge / 100) * realtimeIndAgentProviderAmnt) * vatCharge;
                                realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;
                            }
                            else if (realtimeIndAgentchargeIn == "Flat")
                            {
                                decimal? realTimeIndAgntAmountWithChargesWithVat = realtimeIndAgentcharge * vatCharge;
                                realTimeIndAgntAmountWithCharges = realTimeIndAgntAmountWithChargesWithVat;
                            }

                            individualInstantfeeAsPatient = realTimeIndPtnAmountWithCharges;
                            individualInstantfeeAsAgent = realTimeIndAgntAmountWithCharges + realTimeIndPlAgntAmountWithCharges + realtimeIndAgentProviderAmnt;
                        }
                        //else
                        //{
                        var docChamberfeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Chamber && f.TotalFee != null).OrderBy(a => a.TotalFee).ToList();
                        if (docChamberfeees != null)
                        {
                            decimal? scf = docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee;
                            //decimal? scheduledChamberfee = docChamberfeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee;

                            decimal? scfPtnAmountWithCharges = 0;
                            decimal? scfAgntAmountWithCharges = 0;

                            var scfPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.AmountIn;
                            decimal? scfPatientcharge = sfees.Where(a => a.PlatformFacilityId == 1)?.FirstOrDefault()?.Amount;

                            if (scfPatientchargeIn == "Percentage")
                            {
                                decimal? scfPtnAmountWithChargesWithVat = ((scfPatientcharge / 100) * scf) * vatCharge;
                                scfPtnAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                            }
                            else if (scfPatientchargeIn == "Flat")
                            {
                                decimal? scfPtnAmountWithChargesWithVat = (scfPatientcharge) * vatCharge;
                                scfPtnAmountWithCharges = scfPtnAmountWithChargesWithVat + scf;
                            }
                            scheduledChamberfee = scfPtnAmountWithCharges;

                        }
                        var docOnlinefeees = doctorFees.Where(f => f.DoctorSchedule.ConsultancyType == ConsultancyType.Online && f.TotalFee != null).OrderBy(a => a.TotalFee).ToList();
                        if (docOnlinefeees != null)
                        {
                            decimal? sof = docOnlinefeees?.FirstOrDefault(d => d.DoctorSchedule?.DoctorProfileId == item.Id)?.TotalFee;

                            decimal? sofPtnAmountWithCharges = 0;
                            decimal? sofAgntAmountWithCharges = 0;

                            var sofPatientchargeIn = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.AmountIn;
                            decimal? sofPatientcharge = sfees.Where(a => a.PlatformFacilityId == 2)?.FirstOrDefault()?.Amount;

                            if (sofPatientchargeIn == "Percentage")
                            {
                                decimal? sofPtnAmountWithChargesWithVat = ((sofPatientcharge / 100) * sof) * vatCharge;
                                sofPtnAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                            }
                            else if (sofPatientchargeIn == "Flat")
                            {
                                decimal? sofPtnAmountWithChargesWithVat = (sofPatientcharge) * vatCharge;
                                sofPtnAmountWithCharges = sofPtnAmountWithChargesWithVat + sof;
                            }
                            scheduledOnlinefee = sofPtnAmountWithCharges;
                        }
                        //}

                        var degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList();
                        string degStr = string.Empty;
                        foreach (var d in degrees)
                        {
                            degStr = degStr + d.DegreeName + ",";
                        }

                        degStr = degStr.Remove(degStr.Length - 1);

                        var experties = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList();
                        string expStr = string.Empty;
                        foreach (var e in experties)
                        {
                            expStr = expStr + e.SpecializationName + ",";
                        }

                        expStr = expStr.Remove(expStr.Length - 1);

                        result.Add(new DoctorProfileDto()
                        {
                            Id = item.Id,
                            Degrees = degrees,
                            Qualifications = degStr,
                            SpecialityId = item.SpecialityId,
                            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
                            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
                            AreaOfExperties = expStr,
                            FullName = item.FullName,
                            DoctorTitle = item.DoctorTitle,
                            DoctorTitleName = item.DoctorTitle > 0 ? ((DoctorTitle)item.DoctorTitle).ToString() : "n/a",
                            MaritalStatus = item.MaritalStatus,
                            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
                            City = item.City,
                            ZipCode = item.ZipCode,
                            Country = item.Country,
                            IdentityNumber = item.IdentityNumber,
                            BMDCRegNo = item.BMDCRegNo,
                            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
                            Email = item.Email,
                            MobileNo = item.MobileNo,
                            DateOfBirth = item.DateOfBirth,
                            Gender = item.Gender,
                            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
                            Address = item.Address,
                            ProfileRole = "Doctor",
                            IsActive = item.IsActive,
                            UserId = item.UserId,
                            IsOnline = item.IsOnline,
                            profileStep = item.profileStep,
                            createFrom = item.createFrom,
                            DoctorCode = item.DoctorCode,
                            ProfilePic = profilePics?.Path,
                            //DisplayInstantFeeAsPatient = individualInstantfeeAsPatient > 0 ? Math.Round((decimal)individualInstantfeeAsPatient, 2) : Math.Round((decimal)instantfeeAsPatient, 2),
                            //DisplayInstantFeeAsAgent = individualInstantfeeAsAgent > 0 ? Math.Round((decimal)individualInstantfeeAsAgent, 2) : Math.Round((decimal)instantfeeAsAgent, 2),
                            //DisplayScheduledChamberFee = scheduledChamberfee > 0 ? Math.Round((decimal)scheduledChamberfee, 2) : 0,
                            //DisplayScheduledOnlineFee = scheduledOnlinefee > 0 ? Math.Round((decimal)scheduledOnlinefee, 2) : 0
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                return null;
            }


            return result;
        }
        public async Task<DoctorProfileDto> UpdateActiveStatusByAdmin(int Id, bool activeStatus)
        {
            var user = await _doctorProfileRepository.GetAsync(x => x.Id == Id);
            if (user != null)
            {
                if (user.IsActive == false)
                {
                    user.IsActive = activeStatus;
                }
            }
            var item = await _doctorProfileRepository.UpdateAsync(user);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);

        }

        public async Task<DoctorProfileDto> UpdateExpertise(int Id, string expertise)
        {
            var user = await _doctorProfileRepository.GetAsync(x => x.Id == Id);
            if (user != null)
            {
               
                    user.Expertise = expertise;
            }
            var item = await _doctorProfileRepository.UpdateAsync(user);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);

        }

        /// <summary>
        /// this function will work 
        /// if doctor click the online togle from the doctors dashboard
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="onlineStatus"></param>
        /// <returns></returns>
        public async Task<DoctorProfileDto> UpdateDoctorsOnlineStatus(long Id, bool onlineStatus)
        {
            var user = await _doctorProfileRepository.GetAsync(x => x.Id == Id);

            if (user != null)
            {
                user.IsOnline = onlineStatus;
                var updatedUser = await _doctorProfileRepository.UpdateAsync(user);
                await _unitOfWorkManager.Current.SaveChangesAsync();
                return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(updatedUser);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// this fuction used for doctor profile update with the steps forwording
        /// degrees will added with this fuctions
        /// specializaitons wil added with this fuctins
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DoctorProfileDto> UpdateAsync(DoctorProfileInputDto input)
        {
            var result = new DoctorProfileDto();
            try
            {
                var updateItem = ObjectMapper.Map<DoctorProfileInputDto, DoctorProfile>(input);
                var item = await _doctorProfileRepository.UpdateAsync(updateItem);
                await _unitOfWorkManager.Current.SaveChangesAsync();
                result = ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
                if (result != null)
                {
                    if (input.Degrees?.Count > 0)
                    {
                        foreach (var d in input.Degrees)
                        {
                            var alldegrees = await _doctorDegreeRepository.WithDetailsAsync();
                            var existingDegree = alldegrees.FirstOrDefault(e => e.DegreeId == d.DegreeId && e.DoctorProfileId == d.DoctorProfileId);
                            if (existingDegree == null)
                            {
                                var degree = new DoctorDegreeInputDto
                                {
                                    DoctorProfileId = result.Id,
                                    DegreeId = d.DegreeId,
                                    Duration = d.Duration,
                                    PassingYear = d.PassingYear,
                                    InstituteName = d.InstituteName,
                                    InstituteCity = d.InstituteCity,
                                    InstituteCountry = d.InstituteCountry
                                };
                                var newDegree = ObjectMapper.Map<DoctorDegreeInputDto, DoctorDegree>(degree);
                                var doctorDegree = await _doctorDegreeRepository.InsertAsync(newDegree);
                                ObjectMapper.Map<DoctorDegree, DoctorDegreeDto>(doctorDegree);
                            }
                            else
                            {
                                existingDegree.DegreeId = d.DegreeId;
                                existingDegree.Duration = d.Duration;
                                existingDegree.PassingYear = d.PassingYear;
                                existingDegree.InstituteName = d.InstituteName;
                                existingDegree.InstituteCity = d.InstituteCity;
                                existingDegree.InstituteCountry = d.InstituteCountry;
                                var doctorDegree = await _doctorDegreeRepository.UpdateAsync(existingDegree);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                                ObjectMapper.Map<DoctorDegree, DoctorDegreeDto>(doctorDegree);
                            }
                        }
                    }
                    if (input.DoctorSpecialization?.Count > 0)
                    {

                        foreach (var s in input.DoctorSpecialization)
                        {
                            var allExperties = await _doctorSpecializationRepository.WithDetailsAsync();
                            var existingSpecializations = allExperties.FirstOrDefault(e => e.SpecializationId == s.SpecializationId && e.DoctorProfileId == s.DoctorProfileId);
                            if (existingSpecializations == null)
                            {
                                var specialization = new DoctorSpecializationInputDto
                                {
                                    DoctorProfileId = result.Id,
                                    SpecialityId = s.SpecialityId,
                                    SpecializationId = s.SpecializationId,
                                    DocumentName = s.DocumentName,
                                };
                                var newSpcializations = ObjectMapper.Map<DoctorSpecializationInputDto, DoctorSpecialization>(specialization);
                                var doctorSpecialization = await _doctorSpecializationRepository.InsertAsync(newSpcializations);
                                ObjectMapper.Map<DoctorSpecialization, DoctorSpecializationDto>(doctorSpecialization);
                            }
                            else
                            {
                                existingSpecializations.SpecializationId = s.SpecializationId;
                                existingSpecializations.DocumentName = s.DocumentName;
                                var doctorSpecialization = await _doctorSpecializationRepository.UpdateAsync(existingSpecializations);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                                ObjectMapper.Map<DoctorSpecialization, DoctorSpecializationDto>(doctorSpecialization);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return result;//ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
        }
        public async Task<DoctorProfileDto> UpdateProfileStepAsync(long profileId, int step)
        {
            try
            {
                var profile = await _doctorProfileRepository.GetAsync(a => a.Id == profileId);//.FindAsync(input.Id);
                profile.profileStep = step;

                var item = await _doctorProfileRepository.UpdateAsync(profile);
                //await _unitOfWorkManager.Current.SaveChangesAsync();
                return ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        /// <summary>
        /// Doctor profile edit fuctions
        /// Doctor profile updated from admin/doctor profiles settings etc
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DoctorProfileDto> UpdateDoctorProfileAsync(DoctorProfileInputDto input)
        {
            var result = new DoctorProfileDto();
            try
            {
                var itemDoctor = await _doctorProfileRepository.GetAsync(d => d.Id == input.Id);
                if (itemDoctor != null)
                {
                    itemDoctor.FullName = !string.IsNullOrEmpty(input.FullName) ? input.FullName : itemDoctor.FullName;
                    itemDoctor.DoctorTitle = input.DoctorTitle; //input.DoctorTitle > 0 ? input.DoctorTitle : itemDoctor.DoctorTitle;
                    itemDoctor.DateOfBirth = input.DateOfBirth;
                    itemDoctor.Gender = input.Gender; 
                    itemDoctor.Address =  input.Address;
                    itemDoctor.City =  input.City ;
                    itemDoctor.Country =  input.Country;
                    itemDoctor.ZipCode = input.ZipCode;
                    itemDoctor.Email =  input.Email;
                    itemDoctor.IdentityNumber =  input.IdentityNumber ;
                    itemDoctor.BMDCRegNo = input.BMDCRegNo;
                    itemDoctor.BMDCRegExpiryDate = input.BMDCRegExpiryDate;
                    itemDoctor.SpecialityId = input.SpecialityId > 0 ? input.SpecialityId : itemDoctor.SpecialityId;
                    itemDoctor.IsActive = input.IsActive;
                    itemDoctor.Expertise = input.Expertise;


                    var item = await _doctorProfileRepository.UpdateAsync(itemDoctor);
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    result = ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
                }
            }
            catch (Exception ex)
            {
            }
            return result;//ObjectMapper.Map<DoctorProfile, DoctorProfileDto>(item);
        }


    }
}

//public async Task<List<DoctorProfileDto>> GetCurrentlyOnlineDoctorListAsync1()
//{
//    List<DoctorProfileDto> result = null;
//    var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
//    var profiles = profileWithDetails.Where(o => o.IsOnline == true && o.IsActive == true).ToList();
//    var schedules = await _doctorScheduleRepository.WithDetailsAsync();
//    //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
//    if (!profiles.Any())
//    {
//        return result;
//    }
//    result = new List<DoctorProfileDto>();
//    var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
//    var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


//    var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
//    var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());


//    var attachedItems = await _documentsAttachment.WithDetailsAsync();

//    foreach (var item in profiles)
//    {
//        var profilePics = attachedItems.Where(x => x.EntityType == EntityType.Doctor
//                                                        && x.EntityId == item.Id
//                                                        && x.AttachmentType == AttachmentType.ProfilePicture
//                                                        && x.IsDeleted == false).FirstOrDefault();

//        result.Add(new DoctorProfileDto()
//        {
//            Id = item.Id,
//            Degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList(),
//            SpecialityId = item.SpecialityId,
//            SpecialityName = item.SpecialityId > 0 ? item.Speciality?.SpecialityName : "n/a",
//            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id && sp.SpecialityId == item.SpecialityId).ToList(),
//            FullName = item.FullName,
//            DoctorTitle = item.DoctorTitle,
//            DoctorTitleName = item.DoctorTitle > 0 ? ((DoctorTitle)item.DoctorTitle).ToString() : "n/a",
//            MaritalStatus = item.MaritalStatus,
//            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
//            City = item.City,
//            ZipCode = item.ZipCode,
//            Country = item.Country,
//            IdentityNumber = item.IdentityNumber,
//            BMDCRegNo = item.BMDCRegNo,
//            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
//            Email = item.Email,
//            MobileNo = item.MobileNo,
//            DateOfBirth = item.DateOfBirth,
//            Gender = item.Gender,
//            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
//            Address = item.Address,
//            ProfileRole = "Doctor",
//            IsActive = item.IsActive,
//            UserId = item.UserId,
//            IsOnline = item.IsOnline,
//            profileStep = item.profileStep,
//            createFrom = item.createFrom,
//            ProfilePic = profilePics?.Path,
//            DoctorCode = item.DoctorCode,
//        });
//    }

//    return result;
//}
//public async Task<List<DoctorProfileDto>> GetDoctorListSearchByNameAsync(string? name, FilterModel? filterModel) //, int? skipValue, int? currentLimit)
//{
//    List<DoctorProfileDto> result = null;
//    var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
//    var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();
//    var schedules = await _doctorScheduleRepository.WithDetailsAsync();
//    //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
//    if (!profileWithDetails.Any())
//    {
//        return result;
//    }
//    result = new List<DoctorProfileDto>();
//    var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
//    var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


//    var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
//    var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

//    if (!string.IsNullOrEmpty(name))
//    {
//        profiles = profiles.Where(p => p.FullName == name).ToList();
//    }

//    profiles = profiles.Skip(filterModel.Offset)
//                       .Take(filterModel.Limit).ToList();

//    foreach (var item in profiles)
//    {
//        result.Add(new DoctorProfileDto()
//        {
//            Id = item.Id,
//            Degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList(),
//            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id).ToList(),
//            FullName = item.FullName,
//            DoctorTitle = item.DoctorTitle,
//            DoctorTitleName = item.DoctorTitle > 0 ? ((DoctorTitle)item.DoctorTitle).ToString() : "n/a",
//            MaritalStatus = item.MaritalStatus,
//            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
//            City = item.City,
//            ZipCode = item.ZipCode,
//            Country = item.Country,
//            IdentityNumber = item.IdentityNumber,
//            BMDCRegNo = item.BMDCRegNo,
//            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
//            Email = item.Email,
//            MobileNo = item.MobileNo,
//            DateOfBirth = item.DateOfBirth,
//            Gender = item.Gender,
//            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
//            Address = item.Address,
//            ProfileRole = "Doctor",
//            IsActive = item.IsActive,
//            UserId = item.UserId,
//            IsOnline = item.IsOnline,
//            profileStep = item.profileStep,
//            createFrom = item.createFrom,
//            DoctorCode = item.DoctorCode,
//            SpecialityId = item.SpecialityId,
//            SpecialityName = item.SpecialityId > 0 ? item.Speciality.SpecialityName : "n/a",
//        });
//    }

//    return result;
//}

//public async Task<int> GetDoctorsCountByNameAsync(string? name) //, int? skipValue, int? currentLimit)
//{
//    var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
//    var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();
//    var schedules = await _doctorScheduleRepository.WithDetailsAsync();
//    //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
//    if (!profileWithDetails.Any())
//    {
//        return 0;
//    }
//    var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
//    var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


//    var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
//    var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

//    if (!string.IsNullOrEmpty(name))
//    {
//        profiles = profiles.Where(p => p.FullName == name).ToList();
//    }

//    return profiles.Count;
//}

//public async Task<List<DoctorProfileDto>> GetDoctorListWithSearchFilterAsync(string? name, ConsultancyType? consultancy, long? speciality, long? specialization, int? skipValue, int? currentLimit)
//{
//    List<DoctorProfileDto> result = null;
//    var profileWithDetails = await _doctorProfileRepository.WithDetailsAsync(s => s.Degrees, p => p.Speciality, d => d.DoctorSpecialization);
//    var profiles = profileWithDetails.Where(p => p.IsActive == true).ToList();
//    var schedules = await _doctorScheduleRepository.WithDetailsAsync();
//    //var scheduleCons = schedules.Where(s=>(s.ConsultancyType == consultType)
//    if (!profileWithDetails.Any())
//    {
//        return result;
//    }
//    result = new List<DoctorProfileDto>();
//    var medicaldegrees = await _doctorDegreeRepository.WithDetailsAsync(d => d.Degree);
//    var doctorDegrees = ObjectMapper.Map<List<DoctorDegree>, List<DoctorDegreeDto>>(medicaldegrees.ToList());


//    var medcalSpecializations = await _doctorSpecializationRepository.WithDetailsAsync(s => s.Specialization, sp => sp.Speciality);
//    var doctorSpecializations = ObjectMapper.Map<List<DoctorSpecialization>, List<DoctorSpecializationDto>>(medcalSpecializations.ToList());

//    if (!string.IsNullOrEmpty(name))
//    {
//        profiles = profiles.Where(p => p.FullName == name).ToList();
//    }

//    if (speciality > 0)
//    {
//        profiles = profiles.Where(p => p.SpecialityId == speciality).ToList();
//        doctorSpecializations = doctorSpecializations.Where(sp => sp.SpecialityId == speciality).ToList();
//    }

//    if (specialization > 0)
//    {
//        doctorSpecializations = doctorSpecializations.Where(sp => sp.Id == specialization).ToList();
//        profiles = (from t1 in profiles
//                    join t2 in doctorSpecializations.Where(c => c.Id == specialization)
//                    on t1.Id equals t2.DoctorProfileId
//                    select t1).ToList();
//    }

//    if (consultancy > 0)
//    {
//        //schedules = schedules.Where(c=>c.ConsultancyType==consultType).ToList();
//        profiles = (from t1 in profiles
//                    join t2 in schedules.Where(c => c.ConsultancyType == consultancy)
//                    on t1.Id equals t2.DoctorProfileId
//                    select t1).ToList();
//    }

//    profiles = profiles.Skip((int)(skipValue > 0 ? skipValue : 0))
//                       .Take((int)(currentLimit > 0 ? currentLimit : 0)).ToList();

//    foreach (var item in profiles)
//    {
//        result.Add(new DoctorProfileDto()
//        {
//            Id = item.Id,
//            Degrees = doctorDegrees.Where(d => d.DoctorProfileId == item.Id).ToList(),
//            DoctorSpecialization = doctorSpecializations.Where(sp => sp.DoctorProfileId == item.Id).ToList(),
//            FullName = item.FullName,
//            DoctorTitle = item.DoctorTitle,
//            DoctorTitleName = item.DoctorTitle > 0 ? ((DoctorTitle)item.DoctorTitle).ToString() : "n/a",
//            MaritalStatus = item.MaritalStatus,
//            MaritalStatusName = item.MaritalStatus > 0 ? ((MaritalStatus)item.MaritalStatus).ToString() : "n/a",
//            City = item.City,
//            ZipCode = item.ZipCode,
//            Country = item.Country,
//            IdentityNumber = item.IdentityNumber,
//            BMDCRegNo = item.BMDCRegNo,
//            BMDCRegExpiryDate = item.BMDCRegExpiryDate,
//            Email = item.Email,
//            MobileNo = item.MobileNo,
//            DateOfBirth = item.DateOfBirth,
//            Gender = item.Gender,
//            GenderName = item.Gender > 0 ? ((Gender)item.Gender).ToString() : "n/a",
//            Address = item.Address,
//            ProfileRole = "Doctor",
//            IsActive = item.IsActive,
//            UserId = item.UserId,
//            IsOnline = item.IsOnline,
//            profileStep = item.profileStep,
//            createFrom = item.createFrom,
//            DoctorCode = item.DoctorCode,
//            SpecialityId = item.SpecialityId,
//            SpecialityName = item.SpecialityId > 0 ? item.Speciality.SpecialityName : "n/a",
//        });
//    }

//    return result;
//}
//public async Task<List<DoctorProfileDto>> GetAllDoctorsSearchListAsync(string? name, int? consultType, long? speciality, long? specialization)
//{
//    var profiles = await _doctorProfileRepository.WithDetailsAsync(d => d.Degrees, s => s.DoctorSpecialization);
//    var specializations = await _doctorSpecializationRepository.GetListAsync(sp => sp.Id == specialization);
//    //var doctors = new List<DoctorProfileDto>();
//    if (profiles.Any())
//    {
//        //var doctors = from d in profiles join sp in specializations on d.Id equals sp.DoctorProfileId
//        //profiles.Where(d => d.FullName == name && d.SpecialityId == speciality).ToList();
//        //doctors = doctors.Where(sp=>sp.DoctorSpecialization)
//    }
//    return ObjectMapper.Map<List<DoctorProfile>, List<DoctorProfileDto>>(profiles.ToList());
//}