using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoowGoodWeb.Enums;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.SslCommerz;
using AgoraIO.Media;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using SoowGoodWeb.Utilities;
using static System.Net.WebRequestMethods;
using Volo.Abp.Application.Dtos;
using Microsoft.Extensions.Configuration;
using SoowGoodWeb.Application.Service.Services.SMSService;
using Polly;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using OfficeOpenXml;
using System.IO;

namespace SoowGoodWeb.Services
{
    public class AppointmentService : SoowGoodWebAppService, IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<DoctorChamber> _doctorChamberRepository;
        private readonly IRepository<AgentProfile> _agentRepository;
        private readonly IRepository<DoctorScheduleDaySession> _doctorScheduleSessionRepository;
        private readonly IRepository<PatientProfile> _patientProfileRepository;
        private readonly IRepository<AgentProfile> _agentProfileRepository;
        private readonly IRepository<PaymentHistory> _paymentHistoryRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<DoctorProfile> _doctorDetails;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly SslCommerzGatewayManager _sslCommerzGatewayManager;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;
        private readonly GreenWebSmsService _greenWebSmsService;
        private readonly uint _expireTimeInSeconds = 3600;

        public AppointmentDto GlobalAppointmentResponse { get; set; }
        public AppointmentService(IRepository<Appointment> appointmentRepository,
            IRepository<DoctorChamber> doctorChamberRepository,
            IRepository<AgentProfile> agentRepository,
            IRepository<PaymentHistory> paymentHistoryRepository,
            IRepository<DoctorScheduleDaySession> doctorScheduleSessionRepository,
            IRepository<PatientProfile> patientProfileRepository,
            IRepository<DoctorProfile> doctorDetails,
            IRepository<AgentProfile> agentProfileRepository,
            //SslCommerzGatewayManager sslCommerzGatewayManager,
            IUnitOfWorkManager unitOfWorkManager,
            IHubContext<BroadcastHub, IHubClient> hubContext,
            IRepository<Notification> notificationRepository,
            ISmsService smsService, IConfiguration configuration,
            GreenWebSmsService greenWebSmsService)
        {
            _appointmentRepository = appointmentRepository;
            //_doctorScheduleRepository = doctorScheduleRepository;
            _doctorChamberRepository = doctorChamberRepository;
            _doctorScheduleSessionRepository = doctorScheduleSessionRepository;
            _patientProfileRepository = patientProfileRepository;
            _doctorDetails = doctorDetails;
            _agentProfileRepository = agentProfileRepository;
            //_sslCommerzGatewayManager = sslCommerzGatewayManager;
            _agentRepository = agentRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _hubContext = hubContext;
            _paymentHistoryRepository = paymentHistoryRepository;
            _notificationRepository = notificationRepository;
            _smsService = smsService;
            _configuration = configuration;
            _greenWebSmsService = greenWebSmsService;
        }
        /// <summary>
        /// Appointment Create from this function
        /// 
        /// </summary>
        /// <param name="input">
        /// Patient id
        /// doctor id
        /// doctor schedule id if(chamber or scheduled online)
        /// chamber id  if(chamber or scheduled online)
        /// fee setup id if(chamber or scheduled online)
        /// schedule session id if(chamber or scheduled online)
        /// consulatane type
        /// appointment date
        /// appointment time
        /// appoinement type
        /// doctor fee
        /// agent fee if appointment create from agent
        /// platform fee
        /// vat fee
        /// </param>
        /// <returns></returns>
        /// 

        //public async Task<AppointmentDto> CreateAsync(AppointmentInputDto input)
        //{
        //    var response = new AppointmentDto();
        //    var notificatinInput = new NotificationInputDto();
        //    var notificatin = new NotificationDto();
        //    try
        //    {
        //        string consultancyType;
        //        long lastSerial;
        //        var chamberName = "";
        //        input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date).AddDays(1) : null;
        //        //input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date) : null;
        //        if (input.DoctorChamberId > 0)
        //        {
        //            var appChamber = await _doctorChamberRepository.FirstOrDefaultAsync(c => c.Id == input.DoctorChamberId);
        //            chamberName = appChamber.ChamberName;

        //        }

        //        var list = new List<string>();
        //        if (input is { DoctorScheduleId: > 0, DoctorScheduleDaySessionId: > 0 })
        //        {
        //            var mainSession = await _doctorScheduleSessionRepository.GetAsync(s => s.Id == input.DoctorScheduleDaySessionId && s.DoctorScheduleId == input.DoctorScheduleId);
        //            var stTime = Convert.ToDateTime(mainSession.StartTime);
        //            var enTime = Convert.ToDateTime(mainSession.EndTime);
        //            var totalHr = (enTime - stTime).TotalHours;

        //            var hrMnt = totalHr * 60;
        //            var slotPerPatient = hrMnt / mainSession.NoOfPatients;
        //            string[]? slots = null;

        //            for (var appointment = stTime; appointment < enTime; appointment = appointment.AddMinutes((double)slotPerPatient!))
        //            {
        //                list.Add(appointment.ToString("HH:mm"));
        //                slots = list.ToArray();
        //            }

        //            lastSerial = await GetAppCountByScheduleIdSessionIdAsync(input.DoctorScheduleId, input.DoctorScheduleDaySessionId, input.AppointmentDate);



        //            for (var i = lastSerial; i < mainSession.NoOfPatients;)
        //            {
        //                input.AppointmentTime = slots != null ? slots[i] : "";
        //                break;
        //            }


        //            consultancyType = (input.ConsultancyType > 0 ? (ConsultancyType)input.ConsultancyType : 0).ToString();
        //            input.AppointmentSerial = (lastSerial + 1).ToString();
        //            //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
        //            input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL00" + input.AppointmentSerial;
        //        }
        //        else
        //        {
        //            input.ConsultancyType = ConsultancyType.Instant;
        //            input.AppointmentDate = DateTime.Today;
        //            input.AppointmentTime = DateTime.Now.ToString("HH:mm");
        //            input.AppointmentType = AppointmentType.New;
        //            lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
        //            consultancyType = ConsultancyType.Instant.ToString();
        //            input.AppointmentSerial = (lastSerial + 1).ToString();
        //            //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
        //            input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
        //        }


        //        //if (input.PatientProfileId > 0)
        //        //{
        //        //    input.ConsultancyType = ConsultancyType.Instant;
        //        //    input.AppointmentDate = DateTime.Today;
        //        //    input.AppointmentTime = DateTime.Now.ToString("HH:mm");
        //        //    input.AppointmentType = AppointmentType.New;
        //        //    lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
        //        //    consultancyType = ConsultancyType.Instant.ToString();
        //        //    input.AppointmentSerial = (lastSerial + 1).ToString();
        //        //    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
        //        //    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
        //        //}


        //        var newEntity = ObjectMapper.Map<AppointmentInputDto, Appointment>(input);

        //        var doctorChamber = await _appointmentRepository.InsertAsync(newEntity);

        //        response = ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);

        //        response.AppointmentTypeName = response.AppointmentType.ToString();
        //        response.ConsultancyTypeName = response.ConsultancyType.ToString();
        //        response.ChamberPaymentTypeName = response.ChamberPaymentType.ToString();
        //        response.DoctorChamberName = !string.IsNullOrEmpty(chamberName) ? chamberName.ToString() : "SoowGood Online";


        //        if (input.PatientProfileId is not null && input.PatientProfileId > 0)
        //        {

        //        }
        //        if (input.DoctorProfileId is not null && input.DoctorProfileId > 0)
        //        {

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return response;
        //    }

        //    return response;//ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);
        //}


        public async Task<AppointmentDto> CreateAsync(AppointmentInputDto input)
        {
            GlobalAppointmentResponse = new AppointmentDto();
            var response = new AppointmentDto();
            var notificatinInput = new NotificationInputDto();
            var notificatin = new NotificationDto();
            var clientKey = "SoowGood_App";
            try
            {
                string consultancyType;
                long lastSerial;
                var chamberName = "";
                input.CreationTime = DateTime.Now;
                input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date).AddDays(1) : null;
                //input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date) : null;
                if (input.DoctorChamberId > 0)
                {
                    var appChamber = await _doctorChamberRepository.FirstOrDefaultAsync(c => c.Id == input.DoctorChamberId);
                    chamberName = appChamber.ChamberName;

                }

                var list = new List<string>();
                if (input is { DoctorScheduleId: > 0, DoctorScheduleDaySessionId: > 0 })
                {
                    var mainSession = await _doctorScheduleSessionRepository.GetAsync(s => s.Id == input.DoctorScheduleDaySessionId && s.DoctorScheduleId == input.DoctorScheduleId);
                    var stTime = Convert.ToDateTime(mainSession.StartTime);
                    var enTime = Convert.ToDateTime(mainSession.EndTime);
                    var totalHr = (enTime - stTime).TotalHours;

                    var hrMnt = totalHr * 60;
                    var slotPerPatient = hrMnt / mainSession.NoOfPatients;
                    string[]? slots = null;

                    for (var appointment = stTime; appointment < enTime; appointment = appointment.AddMinutes((double)slotPerPatient!))
                    {
                        list.Add(appointment.ToString("HH:mm"));
                        slots = list.ToArray();
                    }

                    // If appointment is for today, filter slots to only those after current time
                    if (input.AppointmentDate?.Date == DateTime.Today)
                    {
                        var currentTime = DateTime.Now.TimeOfDay;
                        slots = list.Where(s => TimeSpan.Parse(s) > currentTime).ToArray();
                    }

                    lastSerial = await GetAppCountByScheduleIdSessionIdAsync(input.DoctorScheduleId, input.DoctorScheduleDaySessionId, input.AppointmentDate);

                    string displayTime = input.CreationTime.ToString("HH:mm");

                    // Assign the first available slot
                    if (slots != null && slots.Length > 0 && lastSerial < slots.Length)
                    {

                        input.AppointmentTime = slots[lastSerial];
                        if (TimeSpan.Parse(displayTime) > TimeSpan.Parse(input.AppointmentTime) && input.AppointmentDate?.Date == DateTime.Today)
                        {
                            input.AppointmentTime = displayTime;

                        }


                    }



                    consultancyType = (input.ConsultancyType > 0 ? (ConsultancyType)input.ConsultancyType : 0).ToString();
                    input.AppointmentSerial = (lastSerial + 1).ToString();
                    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL00" + input.AppointmentSerial;

                    //if (input.ConsultancyType == ConsultancyType.Chamber)
                    //{
                    //    // Send for Admin Message 
                    //    var patientInfo = await _patientProfileRepository.FirstOrDefaultAsync(x => x.Id == input.PatientProfileId);

                    //    var message = $" New scheduled created for patient {input.PatientName} by doctor {input.DoctorName} at {Convert.ToDateTime(input.AppointmentTime).ToString("hh:mm")} {Convert.ToDateTime(input.AppointmentDate).ToString("dd/MM/yyyy")}";
                    //    if (patientInfo.CreatorRole != null)
                    //    {
                    //        await _greenWebSmsService.SendSMS(clientKey, patientInfo.PatientMobileNo, message);

                    //    }
                    //    else
                    //    {
                    //        await _greenWebSmsService.SendSMS(clientKey, patientInfo.MobileNo, message);

                    //    }

                    //    var doctorInfo = await _doctorDetails.FirstOrDefaultAsync(x => x.Id == input.DoctorProfileId);

                    //    var message2 = $" New scheduled created for patient {input.PatientName} by doctor {input.DoctorName} at {Convert.ToDateTime(input.AppointmentTime).ToString("hh:mm")} {Convert.ToDateTime(input.AppointmentDate).ToString("dd/MM/yyyy")}";
                    //    await _greenWebSmsService.SendSMS(clientKey, doctorInfo.MobileNo, message2);


                    //}

                    

                }
                else
                {
                    input.ConsultancyType = ConsultancyType.Instant;
                    input.AppointmentDate = DateTime.Today;
                    input.AppointmentTime = DateTime.Now.ToString("HH:mm");
                    input.AppointmentType = AppointmentType.New;
                    lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
                    consultancyType = ConsultancyType.Instant.ToString();
                    input.AppointmentSerial = (lastSerial + 1).ToString();
                    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
                }


                //if (input.PatientProfileId > 0)
                //{
                //    input.ConsultancyType = ConsultancyType.Instant;
                //    input.AppointmentDate = DateTime.Today;
                //    input.AppointmentTime = DateTime.Now.ToString("HH:mm");
                //    input.AppointmentType = AppointmentType.New;
                //    lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
                //    consultancyType = ConsultancyType.Instant.ToString();
                //    input.AppointmentSerial = (lastSerial + 1).ToString();
                //    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                //    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
                //}


                var newEntity = ObjectMapper.Map<AppointmentInputDto, Appointment>(input);

                var doctorChamber = await _appointmentRepository.InsertAsync(newEntity);
              

                response = ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);

                response.AppointmentTypeName = response.AppointmentType.ToString();
                response.ConsultancyTypeName = response.ConsultancyType.ToString();
                response.ChamberPaymentTypeName = response.ChamberPaymentType.ToString();
                response.DoctorChamberName = !string.IsNullOrEmpty(chamberName) ? chamberName.ToString() : "SoowGood Online";


                if (input.PatientProfileId is not null && input.PatientProfileId > 0)
                {

                }
                if (input.DoctorProfileId is not null && input.DoctorProfileId > 0)
                {

                }

            }
            catch (Exception ex)
            {
                return response;
            }
            double fee = Convert.ToDouble(input.TotalAppointmentFee);
            if (input.ChamberPaymentType == ChamberPaymentType.payAtChamber ||(input.ConsultancyType==ConsultancyType.Instant&& fee <= 0))
            {

               string appointmentTime = !string.IsNullOrWhiteSpace(input.AppointmentTime)
    ? Convert.ToDateTime(input.AppointmentTime).ToString("hh:mm")
    : "N/A";


                var sms = $"Dr. {input.DoctorName}, You have a new appointment scheduled at {appointmentTime} on {input.AppointmentDate}. Please be prepared 5 minutes before the appointment.";
                var smsForPatient = $"Dear {input.PatientName}, You have a new appointment scheduled at {appointmentTime} on {input.AppointmentDate}. Please be prepared 5 minutes before the appointment.";



                if (input.PatientProfileId > 0)
                {
                    var patientInfo = await _patientProfileRepository.FirstOrDefaultAsync(x => x.Id == input.PatientProfileId);

                    if (!string.IsNullOrEmpty(patientInfo.MobileNo))
                    {
                        await _greenWebSmsService.SendSMS(clientKey, patientInfo.MobileNo, smsForPatient);
                    }
                    else
                    {
                        await _greenWebSmsService.SendSMS(clientKey, patientInfo.PatientMobileNo, smsForPatient);
                    }
                }
                if (input.DoctorProfileId > 0)
                {
                    var doctorInfo = await _doctorDetails.FirstOrDefaultAsync(x => x.Id == input.DoctorProfileId);

                    await _greenWebSmsService.SendSMS(clientKey, doctorInfo.MobileNo, sms);
                }

                // Send for Admin Message 

                var adminMobileNumbers = _configuration.GetSection("AdminMobileNumbers").GetSection("AdminMobileNumbers").Value;
                if (!string.IsNullOrEmpty(adminMobileNumbers))
                {
                    var SeparatedadminMobileNumbers = adminMobileNumbers.Split(",").ToList();
                    foreach (var number in SeparatedadminMobileNumbers)
                    {
                        var message = $" New scheduled created for patient {input.PatientName} by doctor {input.DoctorName} at {appointmentTime} {Convert.ToDateTime(input.AppointmentDate).ToString("dd/MM/yyyy")}";
                        await _greenWebSmsService.SendSMS(clientKey, number, message);
                    }
                }

            }
            return response;//ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);
        }

        public AppointmentDto GetAppointmentInfo()
        {
            return GlobalAppointmentResponse;
        }


        public async Task<AppointmentDto> CreateAppForMobileAsync(AppointmentInputDto input)
        {
            var response = new AppointmentDto();
            var notificatinInput = new NotificationInputDto();
            var notificatin = new NotificationDto();
            try
            {
                string consultancyType;
                long lastSerial;
                var chamberName = "";
                //input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date).AddDays(1) : null;
                if (input.DoctorChamberId > 0)
                {
                    var appChamber = await _doctorChamberRepository.FirstOrDefaultAsync(c => c.Id == input.DoctorChamberId);
                    chamberName = appChamber.ChamberName;

                }

                var list = new List<string>();
                if (input is { DoctorScheduleId: > 0, DoctorScheduleDaySessionId: > 0 })
                {
                    var mainSession = await _doctorScheduleSessionRepository.GetAsync(s => s.Id == input.DoctorScheduleDaySessionId && s.DoctorScheduleId == input.DoctorScheduleId);
                    var stTime = Convert.ToDateTime(mainSession.StartTime);
                    var enTime = Convert.ToDateTime(mainSession.EndTime);
                    var totalHr = (enTime - stTime).TotalHours;

                    var hrMnt = totalHr * 60;
                    var slotPerPatient = hrMnt / mainSession.NoOfPatients;
                    string[]? slots = null;

                    for (var appointment = stTime; appointment < enTime; appointment = appointment.AddMinutes((double)slotPerPatient!))
                    {
                        list.Add(appointment.ToString("HH:mm"));
                        slots = list.ToArray();
                    }

                    lastSerial = await GetAppCountByScheduleIdSessionIdAsync(input.DoctorScheduleId, input.DoctorScheduleDaySessionId, input.AppointmentDate);



                    for (var i = lastSerial; i < mainSession.NoOfPatients;)
                    {
                        input.AppointmentTime = slots != null ? slots[i] : "";
                        break;
                    }


                    consultancyType = (input.ConsultancyType > 0 ? (ConsultancyType)input.ConsultancyType : 0).ToString();
                    input.AppointmentSerial = (lastSerial + 1).ToString();
                    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL00" + input.AppointmentSerial;
                }
                else
                {
                    input.ConsultancyType = ConsultancyType.Instant;
                    input.AppointmentDate = DateTime.Today;
                    input.AppointmentTime = DateTime.Now.ToString("HH:mm");
                    input.AppointmentType = AppointmentType.New;
                    lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
                    consultancyType = ConsultancyType.Instant.ToString();
                    input.AppointmentSerial = (lastSerial + 1).ToString();
                    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
                }


                //if (input.PatientProfileId > 0)
                //{
                //    input.ConsultancyType = ConsultancyType.Instant;
                //    input.AppointmentDate = DateTime.Today;
                //    input.AppointmentTime = DateTime.Now.ToString("HH:mm");
                //    input.AppointmentType = AppointmentType.New;
                //    lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
                //    consultancyType = ConsultancyType.Instant.ToString();
                //    input.AppointmentSerial = (lastSerial + 1).ToString();
                //    //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
                //    input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
                //}


                var newEntity = ObjectMapper.Map<AppointmentInputDto, Appointment>(input);

                var doctorChamber = await _appointmentRepository.InsertAsync(newEntity);

                response = ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);

                response.AppointmentTypeName = response.AppointmentType.ToString();
                response.ConsultancyTypeName = response.ConsultancyType.ToString();
                response.ChamberPaymentTypeName = response.ChamberPaymentType.ToString();
                response.DoctorChamberName = !string.IsNullOrEmpty(chamberName) ? chamberName.ToString() : "SoowGood Online";


                if (input.PatientProfileId is not null && input.PatientProfileId > 0)
                {

                }
                if (input.DoctorProfileId is not null && input.DoctorProfileId > 0)
                {

                }

            }
            catch (Exception ex)
            {
                return response;
            }

            return response;//ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);
        }

        //public async Task<AppointmentDto> CreateAppForMobileAsync(AppointmentInputDto input)
        //{
        //    var response = new AppointmentDto();
        //    var notificatinInput = new NotificationInputDto();
        //    var notificatin = new NotificationDto();
        //    try
        //    {
        //        string consultancyType;
        //        long lastSerial;
        //        var chamberName = "";
        //        input.AppointmentDate = input.AppointmentDate != null ? Convert.ToDateTime(input.AppointmentDate.Value.Date) : null;
        //        if (input.DoctorChamberId > 0)
        //        {
        //            var appChamber = await _doctorChamberRepository.FirstOrDefaultAsync(c => c.Id == input.DoctorChamberId);
        //            chamberName = appChamber.ChamberName;
            
        //        }

        //        var list = new List<string>();
        //        if (input is { DoctorScheduleId: > 0, DoctorScheduleDaySessionId: > 0 })
        //        {
        //            var mainSession = await _doctorScheduleSessionRepository.GetAsync(s => s.Id == input.DoctorScheduleDaySessionId && s.DoctorScheduleId == input.DoctorScheduleId);
        //            var stTime = Convert.ToDateTime(mainSession.StartTime);
        //            var enTime = Convert.ToDateTime(mainSession.EndTime);
        //            var totalHr = (enTime - stTime).TotalHours;

        //            var hrMnt = totalHr * 60;
        //            var slotPerPatient = hrMnt / mainSession.NoOfPatients;
        //            string[]? slots = null;

        //            for (var appointment = stTime; appointment < enTime; appointment = appointment.AddMinutes((double)slotPerPatient!))
        //            {
        //                list.Add(appointment.ToString("HH:mm"));
        //                slots = list.ToArray();
        //            }

        //            lastSerial = await GetAppCountByScheduleIdSessionIdAsync(input.DoctorScheduleId, input.DoctorScheduleDaySessionId);


        //            for (var i = lastSerial; i < mainSession.NoOfPatients;)
        //            {
        //                input.AppointmentTime = slots != null ? slots[i] : "";
        //                break;
        //            }
        //            consultancyType = (input.ConsultancyType > 0 ? (ConsultancyType)input.ConsultancyType : 0).ToString();
        //            input.AppointmentSerial = (lastSerial + 1).ToString();
        //            //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
        //            input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL00" + input.AppointmentSerial;
        //        }
        //        else
        //        {
        //            input.ConsultancyType = ConsultancyType.Instant;
        //            input.AppointmentDate = DateTime.Today;
        //            input.AppointmentTime = DateTime.Now.ToString("HH:mm");
        //            input.AppointmentType = AppointmentType.New;
        //            lastSerial = await GetAppCountByRealTimeConsultancyAsync(input.AppointmentDate);
        //            consultancyType = ConsultancyType.Instant.ToString();
        //            input.AppointmentSerial = (lastSerial + 1).ToString();
        //            //input.AppointmentCode = input.DoctorCode + input.AppointmentDate?.ToString("yyyyMMdd") + consultancyType + "SL-" + input.AppointmentSerial;
        //            input.AppointmentCode = "SGAP" + input.AppointmentDate?.ToString("yyMMdd") + consultancyType.ToUpper() + Guid.NewGuid() + "SL-" + input.AppointmentSerial;
        //        }

        //        var newEntity = ObjectMapper.Map<AppointmentInputDto, Appointment>(input);

        //        var doctorChamber = await _appointmentRepository.InsertAsync(newEntity);

        //        response = ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);

        //        response.AppointmentTypeName = response.AppointmentType.ToString();
        //        response.ConsultancyTypeName = response.ConsultancyType.ToString();
        //        response.ChamberPaymentTypeName = response.ChamberPaymentType.ToString();
        //        response.DoctorChamberName = !string.IsNullOrEmpty(chamberName) ? chamberName.ToString() : "SoowGood Online";

        //    }
        //    catch (Exception ex)
        //    {
        //        return response;
        //    }

        //    return response;//ObjectMapper.Map<Appointment, AppointmentDto>(doctorChamber);
        //}

        public async Task<AppointmentDto> UpdateAsync(AppointmentInputDto input)
        {
            var updateItem = ObjectMapper.Map<AppointmentInputDto, Appointment>(input);

            var item = await _appointmentRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<Appointment, AppointmentDto>(item);
        }

        public async Task<AppointmentDto?> GetAsync(int id)
        {
            var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
            var patientDetails = new PatientProfile();

            var schedule = item.FirstOrDefault(x => x.Id == id);
            if (schedule != null)
            {
                patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == schedule.PatientProfileId);

            }


            var result = schedule != null ? ObjectMapper.Map<Appointment, AppointmentDto>(schedule) : null;

            if (result != null)
            {
                result.PatientMobileNo = patientDetails.CreatorRole!=null? patientDetails.PatientMobileNo:patientDetails.MobileNo;
                result.BloodGroup = patientDetails.BloodGroup;
                result.PatientAge = patientDetails.Age;
            }

            return result;
        }

        public async Task<List<AppointmentDto>> GetListAsync()
        {
            var appointments = await _appointmentRepository.GetListAsync();
            return ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
        }

        public async Task<List<AppointmentDto>> GetAppointmentListByDoctorIdAsync(long doctorId)
        {
            var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
            var appointments = item.Where(d => d.DoctorProfileId == doctorId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();
            return ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
        }
        /// <summary>
        /// 
        /// this function used for getting appointment list for specific doct
        /// this list can be filter by the data filter parameter
        /// </summary>
        /// <param name="doctorId"></param>
        /// <param name="dataFilter"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public async Task<IActionResult?> GetAppointmentListForDoctorWithSearchFilterAsync(long doctorId, DataFilterModel? dataFilter, FilterModel filterModel)
        {
            List<AppointmentDto> result = null;
            var provider = CultureInfo.InvariantCulture;

            try
            {
                var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
                var tdate1 = DateTime.Now;
                if (dataFilter?.toDate is null or "Invalid Date")
                {
                    dataFilter.toDate = dataFilter.fromDate;
                    tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;

                }
                else
                {
                    tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
                }
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, s => s.DoctorSchedule.DoctorChamber);
                var appointments = item
    .Where(d => d.DoctorProfileId == doctorId &&
               (d.AppointmentStatus == AppointmentStatus.Confirmed ||
                d.AppointmentStatus == AppointmentStatus.Completed)).ToList();
                appointments= appointments.OrderByDescending(p => p.Id).ToList();

                // && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();

                if (!string.IsNullOrEmpty(dataFilter?.name))
                {
                    appointments = appointments.Where(p => p.PatientName != null && p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim())).ToList();
                }
                if (dataFilter?.scheduleId > 0)
                {
                    appointments = appointments.Where(a => a.DoctorScheduleId == dataFilter.scheduleId).ToList();
                }

                if (dataFilter?.consultancyType != null)
                {
                    appointments = appointments.Where(p => (ConsultancyType)p.ConsultancyType == dataFilter.consultancyType).ToList();
                }
                if (dataFilter?.appointmentStatus != null)
                {
                    appointments = appointments.Where(p => (AppointmentStatus)p.AppointmentStatus == dataFilter.appointmentStatus).ToList();
                }
                if (dataFilter?.appointmentType != null)
                {
                    appointments = appointments.Where(p => (AppointmentType)p.AppointmentType == dataFilter.appointmentType).ToList();

                }

                if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
                            && p?.AppointmentDate.Value.Date <= tdate1).ToList();
                }
                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "today")
                {
                    appointments = appointments
                        .Where(p => p?.AppointmentDate.HasValue == true && p.AppointmentDate.Value.Date == DateTime.Today)
                        .OrderBy(p =>
    p.AppointmentStatus == AppointmentStatus.Confirmed ? 0 : 1)
                        .ToList();
                }



                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "upcoming")
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date > DateTime.Today)
    .OrderBy(p => p.AppointmentDate)
    .ThenBy(p => p.AppointmentSerial) // optional secondary sort
    .ToList();
                }

                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "completed")
                {
                    appointments = appointments.Where(p => p.AppointmentStatus == AppointmentStatus.Completed).ToList();
                }
                // After all filters (including 'day', etc.)
               



                result = new List<AppointmentDto>();
                var doctorDetails = await _doctorDetails.WithDetailsAsync(s => s.Speciality);
                foreach (var itemApt in appointments)
                {
                    var patientDetails = new PatientProfile();
                    var doctorInfo = doctorDetails.Where(d => d.Id == itemApt.DoctorProfileId).FirstOrDefault();
                    try
                    {
                        patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == itemApt.PatientProfileId);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var drTitle = Utilities.Utility.GetDisplayName(doctorInfo.DoctorTitle);
                    result.Add(new AppointmentDto()
                    {
                        Id = itemApt.Id,
                        AppointmentCode = itemApt.AppointmentCode,
                        AppointmentSerial = itemApt.AppointmentSerial,
                        DoctorCode = itemApt.DoctorCode,
                        DoctorScheduleId = itemApt.DoctorScheduleId,
                        DoctorScheduleName = itemApt.DoctorScheduleId > 0 ? itemApt.DoctorSchedule.ScheduleName : "N/A",
                        DoctorProfileId = itemApt.DoctorProfileId,
                        DoctorName = drTitle + " " + itemApt.DoctorName,
                        PatientProfileId = itemApt.PatientProfileId,
                        PatientCode = patientDetails.PatientCode,
                        PatientName = itemApt.PatientName,
                        PatientLocation = patientDetails.City,
                        BloodGroup = patientDetails.BloodGroup,
                        PatientAge = patientDetails.Age,
                        PatientMobileNo = patientDetails.CreatorRole != null ? patientDetails.PatientMobileNo : patientDetails.MobileNo,
                        ConsultancyType = itemApt.ConsultancyType,
                        ConsultancyTypeName = itemApt.ConsultancyType > 0 ? ((ConsultancyType)itemApt.ConsultancyType).ToString() : "N/A",
                        DoctorChamberId = itemApt.DoctorChamberId,
                        DoctorChamberName = itemApt.DoctorChamberId > 0 ? itemApt?.DoctorSchedule?.DoctorChamber?.ChamberName : "N/A",
                        DoctorChamberAddress = itemApt.DoctorChamberId > 0 ? itemApt?.DoctorSchedule?.DoctorChamber?.Address : "N/A",
                        DoctorScheduleDaySessionId = itemApt.DoctorScheduleDaySessionId,
                        ScheduleDayofWeek = itemApt.ScheduleDayofWeek,
                        AppointmentType = itemApt.AppointmentType,
                        AppointmentTypeName = itemApt.AppointmentType > 0 ? ((AppointmentType)itemApt.AppointmentType).ToString() : "N/A",
                        AppointmentDate = itemApt.AppointmentDate,
                        AppointmentTime = itemApt.AppointmentTime,
                        AppointmentStatus = itemApt.AppointmentStatus,
                        AppointmentStatusName = ((AppointmentStatus)itemApt.AppointmentStatus).ToString(),
                        AppointmentPaymentStatusName = itemApt.AppointmentStatus > 0 ? ((AppointmentStatus)itemApt.AppointmentStatus).ToString() : "N/A",
                        TotalAppointmentFee = itemApt.DoctorFee,
                        GenderName = patientDetails.Gender.ToString(),

                    });
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            var Count = result.Count();

            if (filterModel.ExportToExcel)
            {
                return ExportAppointmentsToExcel(result);

            }

            int skip = (filterModel.PageNo > 0 ? filterModel.PageNo - 1 : 0) * filterModel.PageSize;

            var pagedAppointments = result
     .Skip(skip)
     .Take(filterModel.PageSize > 0 ? filterModel.PageSize : 10)
     .ToList();
            return new OkObjectResult(new
            {
                result = pagedAppointments,
                count = Count
            });



        }

        public async Task<int> GetAppointmentCountForDoctorWithSearchFilterAsync(long doctorId, DataFilterModel? dataFilter)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
                var tdate1 = DateTime.Now;
                if (dataFilter?.toDate is null or "Invalid Date")
                {
                    if (dataFilter != null)
                    {
                        dataFilter.toDate = dataFilter.fromDate;
                        tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
                    }
                }
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
                var appointments = item.Where(d => d.DoctorProfileId == doctorId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();// && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();

                if (!string.IsNullOrEmpty(dataFilter?.name))
                {
                    appointments = appointments.Where(p => p.PatientName != null && p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim())).ToList();
                }
                if (dataFilter?.consultancyType != null)
                {
                    appointments = appointments.Where(p => p.ConsultancyType == dataFilter.consultancyType).ToList();
                }
                if (dataFilter?.appointmentType != null)
                {
                    appointments = appointments.Where(p => p.AppointmentType == dataFilter.appointmentType).ToList();

                }
                if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
                            && p?.AppointmentDate.Value.Date <= tdate1).ToList();
                }
                return appointments.Count();
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<List<AppointmentDto>> GetAppointmentListByPatientIdAsync(long patientId, string role)
        {
            var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
            var appointments = item.Where(d => d.AppointmentCreatorId == patientId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed) && d.AppointmentCreatorRole == role).ToList();
            return ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
        }
        /// <summary>
        /// /// this function used for getting appointment list for specific patient or agent
        /// this list can be filter by the data filter parameter
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="role"></param>
        /// <param name="dataFilter"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetAppointmentListForPatientWithSearchFilterAsync(long patientId, string role, DataFilterModel? dataFilter, FilterModel filterModel)
        {
            List<AppointmentDto> result = null;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
                var tdate1 = DateTime.Now;
                if (dataFilter?.toDate is null or "Invalid Date")
                {
                    dataFilter.toDate = dataFilter.fromDate;
                    tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
                }
                else
                {
                    tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
                }
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, s => s.DoctorSchedule.DoctorChamber);
                //Before Appintment
                // && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();
                var appointments = new List<Appointment>();


                if (role == "agent")
                {
                    appointments = item.Where(d => d.AppointmentCreatorId == patientId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed) && d.AppointmentCreatorRole == role).OrderByDescending(o => o.Id).ToList();

                }
                if (role == "patient")
                {
                    appointments = item.Where(d => (d.PatientProfileId == patientId || d.AppointmentCreatorId == patientId) && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).OrderByDescending(o => o.Id).ToList();

                }


                if (!string.IsNullOrEmpty(dataFilter?.name))
                {
                    appointments = appointments.Where(p => p.DoctorName != null && p.DoctorName.ToLower().Contains(dataFilter.name.ToLower().Trim())).ToList();
                }
                if (dataFilter?.consultancyType != null)
                {
                    appointments = appointments.Where(p => (ConsultancyType)p.ConsultancyType == dataFilter.consultancyType).ToList();
                }
                if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
                            && p?.AppointmentDate.Value.Date <= tdate1).ToList();
                }
                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "today")
                {
                    appointments = appointments
                        .Where(p => p?.AppointmentDate.HasValue == true && p.AppointmentDate.Value.Date == DateTime.Today)
                        .OrderBy(p =>
    p.AppointmentStatus == AppointmentStatus.Confirmed ? 0 : 1)
                        .ToList();
                }
                if (dataFilter?.appointmentStatus != null)
                {
                    appointments = appointments.Where(p => (AppointmentStatus)p.AppointmentStatus == dataFilter.appointmentStatus).ToList();
                }
                if (dataFilter?.appointmentType != null)
                {
                    appointments = appointments.Where(p => (AppointmentType)p.AppointmentType == dataFilter.appointmentType).ToList();

                }


                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "upcoming")
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date > DateTime.Today).Where(p => p?.AppointmentDate.Value.Date > DateTime.Today)
    .OrderBy(p => p.AppointmentDate)
    .ThenBy(p => p.AppointmentSerial) // optional secondary sort
    .ToList();
                }
                if (!string.IsNullOrEmpty(dataFilter?.day) && dataFilter.day == "completed")
                {
                    appointments = appointments.Where(p => p.AppointmentStatus == AppointmentStatus.Completed).ToList();
                }
                
                //appointments = appointments.Skip(filterModel.Offset)
                //                   .Take(filterModel.Limit).ToList();
                result = new List<AppointmentDto>();
                try
                {
                    foreach (var itemApt in appointments)
                    {
                        var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == itemApt.PatientProfileId);
                        var doctorInfo = await _doctorDetails.GetAsync(d => d.Id == itemApt.DoctorProfileId);

                        var drTitle = Utilities.Utility.GetDisplayName(doctorInfo.DoctorTitle);
                        result.Add(new AppointmentDto()
                        {
                            Id = itemApt.Id,
                            AppointmentCode = itemApt.AppointmentCode,
                            AppointmentSerial = itemApt.AppointmentSerial,
                            DoctorCode = itemApt.DoctorCode,
                            DoctorScheduleId = itemApt.DoctorScheduleId,
                            DoctorScheduleName = itemApt.DoctorScheduleId > 0 ? itemApt.DoctorSchedule.ScheduleName : "N/A",
                            DoctorProfileId = itemApt.DoctorProfileId,
                            DoctorName = drTitle + " " + itemApt.DoctorName,
                            PatientProfileId = itemApt.PatientProfileId,
                            PatientCode = patientDetails.PatientCode,
                            PatientName = itemApt.PatientName,
                            PatientLocation = patientDetails.City,
                            ConsultancyType = itemApt.ConsultancyType,
                            ConsultancyTypeName = itemApt.ConsultancyType > 0 ? ((ConsultancyType)itemApt.ConsultancyType).ToString() : "N/A",
                            DoctorChamberId = itemApt.DoctorChamberId,
                            DoctorChamberName = itemApt.DoctorChamberId > 0 ? itemApt?.DoctorSchedule?.DoctorChamber?.ChamberName : "N/A",
                            DoctorChamberAddress = itemApt.DoctorChamberId > 0 ? itemApt?.DoctorSchedule?.DoctorChamber?.Address : "N/A",
                            DoctorScheduleDaySessionId = itemApt.DoctorScheduleDaySessionId,
                            ScheduleDayofWeek = itemApt.ScheduleDayofWeek,
                            AppointmentType = itemApt.AppointmentType,
                            AppointmentStatusName = ((AppointmentStatus)itemApt.AppointmentStatus).ToString(),

                            AppointmentTypeName = itemApt.AppointmentType > 0 ? ((AppointmentType)itemApt.AppointmentType).ToString() : "N/A",
                            AppointmentDate = itemApt.AppointmentDate,
                            AppointmentTime = itemApt.AppointmentTime,
                            AppointmentStatus = itemApt.AppointmentStatus,
                            AppointmentPaymentStatusName = itemApt.AppointmentStatus > 0 ? ((AppointmentStatus)itemApt.AppointmentStatus).ToString() : "N/A",
                            TotalAppointmentFee = itemApt.TotalAppointmentFee,
                            PatientMobileNo = patientDetails.CreatorRole != null ? patientDetails.PatientMobileNo : patientDetails.MobileNo,
                            BloodGroup = patientDetails.BloodGroup,
                            PatientAge = patientDetails.Age,
                            GenderName = patientDetails.Gender.ToString(),

                        });
                    }
                }
                catch (Exception ex)
                {
                }

                //return ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
            }
            catch (Exception ex)
            {
                return null;
            }
            var Count = result.Count();
            int skip = (filterModel.PageNo > 0 ? filterModel.PageNo - 1 : 0) * filterModel.PageSize;
            if (filterModel.ExportToExcel)
            {
                return ExportAppointmentsToExcel(result);

            }
            var pagedAppointments = result
     .Skip(skip)
     .Take(filterModel.PageSize > 0 ? filterModel.PageSize : 10)
     .ToList();
            return new OkObjectResult(new
            {
                result = pagedAppointments,
                count = Count
            });
        }

        public async Task<int> GetAppointmentCountForPatientWithSearchFilterAsync(long patientId, string role, DataFilterModel? dataFilter)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
                var tdate1 = DateTime.Now;
                if (dataFilter?.toDate is null or "Invalid Date")
                {
                    if (dataFilter != null)
                    {
                        dataFilter.toDate = dataFilter.fromDate;
                        tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
                    }
                }
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
                var appointments = item.Where(d => d.AppointmentCreatorId == patientId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed) && d.AppointmentCreatorRole == role).ToList();// && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();

                if (!string.IsNullOrEmpty(dataFilter?.name))
                {
                    appointments = appointments.Where(p => p.PatientName != null && p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim())).ToList();
                }
                if (dataFilter?.consultancyType > 0)
                {
                    appointments = appointments.Where(p => p.ConsultancyType == dataFilter.consultancyType).ToList();
                }
                if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
                {
                    appointments = appointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
                            && p?.AppointmentDate.Value.Date <= tdate1).ToList();
                }

                return appointments.Count;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        /// <summary>
        /// /// this function used for getting appointment list for admin
        /// </summary>
        /// <returns></returns>
        //public async Task<List<AppointmentDto>?> GetListAppointmentListByAdminAsync()
        //{
        //    List<AppointmentDto>? result = null;
        //    DoctorScheduleDaySession? weekDayName = null;
        //    var allAppoinments = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);
        //    //var allAppoinment = allAppoinments.OrderByDescending(d => d.AppointmentDate).ToList();
        //    var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
        //    //var  = await _appointmentRepository.GetListAsync();
        //    if (!allAppoinments.Any())
        //    {
        //        return result;
        //    }

        //    result = new List<AppointmentDto>();
        //    try
        //    {

        //        foreach (var item in allAppoinments)
        //        {
        //            var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == item.PatientProfileId);

        //            //if(item.AppointmentCreatorRole=="agent")
        //            var agent = item.AppointmentCreatorRole == "agent" ? agentDetails.Where(a => a.Id == item.AppointmentCreatorId).FirstOrDefault() : null;
        //            var sDsession = await _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);
        //            if (item.DoctorScheduleDaySessionId > 0)
        //            {
        //                weekDayName = sDsession.FirstOrDefault(p => p.Id == item.DoctorScheduleDaySessionId);
        //            }
        //            result.Add(new AppointmentDto()
        //            {
        //                Id = item.Id,
        //                PatientName = item.PatientName,
        //                AppointmentDate = Convert.ToDateTime(item.AppointmentDate).Date,
        //                AppointmentTime = item.AppointmentTime,
        //                AppointmentSerial = item.AppointmentSerial,
        //                AppointmentType = item.AppointmentType,
        //                AppointmentTypeName = item.AppointmentType > 0 ? ((AppointmentType)item.AppointmentType).ToString() : "n/a",
        //                DoctorName = item.DoctorName,
        //                DoctorScheduleId = item.DoctorScheduleId,
        //                DoctorScheduleName = item.DoctorScheduleId > 0 ? item.DoctorSchedule?.ScheduleName : "n/a",
        //                AppointmentCode = item.AppointmentCode,
        //                AppointmentStatus = item.AppointmentStatus,
        //                DoctorCode = item.DoctorCode,
        //                PatientCode = item.PatientCode,
        //                PatientMobileNo = patientDetails.PatientMobileNo,
        //                MobileNo = patientDetails.MobileNo,
        //                PatientEmail = patientDetails.PatientEmail,
        //                AppointmentStatusName = item.AppointmentStatus > 0 ? ((AppointmentStatus)item.AppointmentStatus).ToString() : "n/a",
        //                AppointmentPaymentStatus = item.AppointmentPaymentStatus,
        //                AppointmentPaymentStatusName = item.AppointmentPaymentStatus > 0 ? ((AppointmentPaymentStatus)item.AppointmentPaymentStatus).ToString() : "n/a",
        //                ConsultancyType = item.ConsultancyType,
        //                ConsultancyTypeName = item.ConsultancyType > 0 ? ((ConsultancyType)item.ConsultancyType).ToString() : "n/a",
        //                DoctorChamberId = item.DoctorChamberId,
        //                DoctorChamberName = item.DoctorChamberId > 0 ? item.DoctorSchedule?.DoctorChamber?.ChamberName : "n/a",
        //                DoctorFee = item.DoctorFee,
        //                PatientLocation = patientDetails?.City?.ToString(),
        //                DoctorScheduleDaySessionId = item.DoctorScheduleDaySessionId,
        //                ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
        //                CancelledByRole = item.CancelledByRole,
        //                PaymentTransactionId = item.PaymentTransactionId,
        //                AppointmentCreatorRole = item.AppointmentCreatorRole,
        //                BoothName = item.AppointmentCreatorRole == "agent" ? agent?.Address : "N/A",
        //                AgentName = item.AppointmentCreatorRole == "agent" ? agent?.FullName : "N/A",
        //                AgentMasterName = item.AppointmentCreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
        //                AgentSupervisorName = item.AppointmentCreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",
        //                TotalAppointmentFee = item.TotalAppointmentFee,
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // ignored
        //    }

        //    result = result.OrderByDescending(a => a.AppointmentDate).ToList();
        //    //var finalList = result.OrderByDescending(item => item.AppointmentDate).ToList();

        //    var finalResult = (from element in result
        //                       group element by element.AppointmentDate
        //              into groups
        //                       select groups.OrderByDescending(p => Convert.ToInt32(p.AppointmentSerial))).SelectMany(g => g).ToList();

        //    return finalResult;
        //}

        public async Task<PagedResultDto<AppointmentDto>> GetListAppointmentListByAdminAsync(int pageNumber, int pageSize)
        {
            // Ensure pageNumber and pageSize are valid
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // Fetch all necessary data concurrently
            var appointmentsTask = _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);
            var agentsTask = _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var doctorSessionsTask = _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);

            await Task.WhenAll(appointmentsTask, agentsTask, doctorSessionsTask);

            var allAppointments = await appointmentsTask;

            // Sort the appointments by AppointmentDate in descending order
            var sortedAppointments = allAppointments.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.AppointmentSerial);

            // Paginate the appointments
            var totalCount = sortedAppointments.Count(); // Total number of appointments for the entire query
            var pagedAppointments = sortedAppointments.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(); // Apply pagination

            var agentDetails = await agentsTask;
            var doctorSessions = await doctorSessionsTask;

            var patientIds = pagedAppointments.Select(a => a.PatientProfileId).Distinct().ToList();
            var patientDetailsDict = (await _patientProfileRepository.GetListAsync(p => patientIds.Contains(p.Id)))
                                     .ToDictionary(p => p.Id, p => p);

            var result = new List<AppointmentDto>();

            foreach (var appointment in pagedAppointments)
            {
                // Safe lookup for patient details using TryGetValue
                PatientProfile? patientDetails = null;

                if (appointment.PatientProfileId.HasValue)
                {
                    patientDetailsDict.TryGetValue(appointment.PatientProfileId.Value, out patientDetails);
                }

                var agent = appointment.AppointmentCreatorRole == "agent"
                            ? agentDetails.FirstOrDefault(a => a.Id == appointment.AppointmentCreatorId)
                            : null;

                var weekDayName = appointment.DoctorScheduleDaySessionId > 0
                                  ? doctorSessions.FirstOrDefault(p => p.Id == appointment.DoctorScheduleDaySessionId)
                                  : null;

                result.Add(new AppointmentDto
                {
                    Id = appointment.Id,
                    PatientName = appointment.PatientName,
                    AppointmentDate = appointment.AppointmentDate?.Date ?? DateTime.MinValue,
                    AppointmentTime = appointment.AppointmentTime,
                    AppointmentSerial = appointment.AppointmentSerial,
                    AppointmentType = appointment.AppointmentType,
                    AppointmentTypeName = appointment.AppointmentType > 0
                                          ? ((AppointmentType)appointment.AppointmentType).ToString()
                                          : "n/a",
                    DoctorName = appointment.DoctorName,
                    DoctorScheduleId = appointment.DoctorScheduleId,
                    DoctorScheduleName = appointment.DoctorScheduleId > 0
                                         ? appointment.DoctorSchedule?.ScheduleName
                                         : "n/a",
                    AppointmentCode = appointment.AppointmentCode,
                    AppointmentStatus = appointment.AppointmentStatus,
                    DoctorCode = appointment.DoctorCode,
                    PatientCode = appointment.PatientCode,
                    PatientMobileNo = patientDetails?.PatientMobileNo,
                    MobileNo = patientDetails?.MobileNo,
                    PatientAge = patientDetails?.Age,
                    PatientEmail = patientDetails?.PatientEmail,
                    AppointmentStatusName = appointment.AppointmentStatus > 0
                                            ? ((AppointmentStatus)appointment.AppointmentStatus).ToString()
                                            : "n/a",
                    AppointmentPaymentStatus = appointment.AppointmentPaymentStatus,
                    AppointmentPaymentStatusName = appointment.AppointmentPaymentStatus > 0
                                                   ? ((AppointmentPaymentStatus)appointment.AppointmentPaymentStatus).ToString()
                                                   : "n/a",
                    ConsultancyType = appointment.ConsultancyType,
                    ConsultancyTypeName = appointment.ConsultancyType > 0
                                          ? ((ConsultancyType)appointment.ConsultancyType).ToString()
                                          : "n/a",
                    DoctorChamberId = appointment.DoctorChamberId,
                    DoctorChamberName = appointment.DoctorChamberId > 0
                                        ? appointment.DoctorSchedule?.DoctorChamber?.ChamberName
                                        : "n/a",
                    DoctorFee = appointment.DoctorFee,
                    PatientLocation = patientDetails?.City?.ToString(),
                    DoctorScheduleDaySessionId = appointment.DoctorScheduleDaySessionId,
                    ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
                    CancelledByRole = appointment.CancelledByRole,
                    PaymentTransactionId = appointment.PaymentTransactionId,
                    AppointmentCreatorRole = appointment.AppointmentCreatorRole,
                    BoothName = agent?.Address ?? "N/A",
                    AgentName = agent?.FullName ?? "N/A",
                    AgentMasterName = agent?.AgentMaster?.AgentMasterOrgName ?? "N/A",
                    AgentSupervisorName = agent?.AgentSupervisor?.AgentSupervisorOrgName ?? "N/A",
                    TotalAppointmentFee = appointment.TotalAppointmentFee,
                });
            }

            // Return the paged result with total count information
            return new PagedResultDto<AppointmentDto>
            {
                TotalCount = totalCount,
                Items = result
            };
        }


        /// <summary>
        /// /// this function used for getting appointment list for specific agent master
        /// </summary>
        /// <param name="agentMasterId"></param>
        /// <returns></returns>
        public async Task<List<AppointmentDto>?> GetListAppointmentListByAgentMasterAsync(long agentMasterId)
        {
            List<AppointmentDto>? result = null;
            DoctorScheduleDaySession? weekDayName = null;
            var allAppoinment = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);
            var appointments = allAppoinment.Where(c => c.AppointmentCreatorRole == "agent").ToList();
            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var agentsByMasters = agentDetails.Where(a => a.AgentMasterId == agentMasterId).ToList();

            var itemAppointments = (from app in appointments join agents in agentsByMasters on app.AppointmentCreatorId equals agents.Id select app).ToList();

            //allAppoinment.Where(ap=>ap.AppointmentCreatorId == agentMasterId);
            //var  = await _appointmentRepository.GetListAsync();
            if (!itemAppointments.Any())
            {
                return result;
            }

            result = new List<AppointmentDto>();
            try
            {

                foreach (var item in itemAppointments)
                {
                    var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == item.PatientProfileId);

                    //if(item.AppointmentCreatorRole=="agent")
                    var agent = item.AppointmentCreatorRole == "agent" ? agentsByMasters.Where(a => a.Id == item.AppointmentCreatorId).FirstOrDefault() : null;
                    var sDsession = await _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);
                    if (item.DoctorScheduleDaySessionId > 0)
                    {
                        weekDayName = sDsession.FirstOrDefault(p => p.Id == item.DoctorScheduleDaySessionId);
                    }
                    result.Add(new AppointmentDto()
                    {
                        Id = item.Id,
                        PatientName = item.PatientName,
                        AppointmentDate = Convert.ToDateTime(item.AppointmentDate).Date,
                        AppointmentTime = item.AppointmentTime,
                        AppointmentSerial = item.AppointmentSerial,
                        AppointmentType = item.AppointmentType,
                        AppointmentTypeName = item.AppointmentType > 0 ? ((AppointmentType)item.AppointmentType).ToString() : "n/a",
                        DoctorName = item.DoctorName,
                        DoctorScheduleId = item.DoctorScheduleId,
                        DoctorScheduleName = item.DoctorScheduleId > 0 ? item.DoctorSchedule?.ScheduleName : "n/a",
                        AppointmentCode = item.AppointmentCode,
                        AppointmentStatus = item.AppointmentStatus,
                        DoctorCode = item.DoctorCode,
                        PatientCode = item.PatientCode,
                        PatientMobileNo = patientDetails.PatientMobileNo,
                        PatientEmail = patientDetails.PatientEmail,
                        AppointmentStatusName = item.AppointmentStatus > 0 ? ((AppointmentStatus)item.AppointmentStatus).ToString() : "n/a",
                        AppointmentPaymentStatus = item.AppointmentPaymentStatus,
                        AppointmentPaymentStatusName = item.AppointmentPaymentStatus > 0 ? ((AppointmentPaymentStatus)item.AppointmentPaymentStatus).ToString() : "n/a",
                        ConsultancyType = item.ConsultancyType,
                        ConsultancyTypeName = item.ConsultancyType > 0 ? ((ConsultancyType)item.ConsultancyType).ToString() : "n/a",
                        DoctorChamberId = item.DoctorChamberId,
                        DoctorChamberName = item.DoctorChamberId > 0 ? item.DoctorSchedule?.DoctorChamber?.ChamberName : "n/a",
                        DoctorFee = item.DoctorFee,
                        PatientLocation = patientDetails?.City?.ToString(),
                        DoctorScheduleDaySessionId = item.DoctorScheduleDaySessionId,
                        ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
                        CancelledByRole = item.CancelledByRole,
                        PaymentTransactionId = item.PaymentTransactionId,
                        AppointmentCreatorRole = item.AppointmentCreatorRole,
                        BoothName = item.AppointmentCreatorRole == "agent" ? agent?.Address : "N/A",
                        AgentMasterName = item.AppointmentCreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                        AgentSupervisorName = item.AppointmentCreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",
                    });
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            result = result.OrderByDescending(a => a.AppointmentDate).ToList();
            var list = result.OrderBy(item => item.AppointmentSerial)
                .GroupBy(item => item.AppointmentDate)
                .OrderBy(g => g.Key).Select(g => new { g }).ToList();


            return result;
        }
        /// <summary>
        /// /// this function used for getting appointment list for specific agent supervisor
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        public async Task<List<AppointmentDto>?> GetListAppointmentListByAgentSupervisorAsync(long supervisorId)
        {
            List<AppointmentDto>? result = null;
            DoctorScheduleDaySession? weekDayName = null;
            var allAppoinment = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);
            var appointments = allAppoinment.Where(c => c.AppointmentCreatorRole == "agent").ToList();
            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var agentsBySupervisors = agentDetails.Where(a => a.AgentMasterId == supervisorId).ToList();

            var itemAppointments = (from app in appointments join agents in agentsBySupervisors on app.AppointmentCreatorId equals agents.Id select app).ToList();

            //allAppoinment.Where(ap=>ap.AppointmentCreatorId == agentMasterId);
            //var  = await _appointmentRepository.GetListAsync();
            if (!itemAppointments.Any())
            {
                return result;
            }

            result = new List<AppointmentDto>();
            try
            {

                foreach (var item in itemAppointments)
                {
                    var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == item.PatientProfileId);

                    //if(item.AppointmentCreatorRole=="agent")
                    var agent = item.AppointmentCreatorRole == "agent" ? agentsBySupervisors.Where(a => a.Id == item.AppointmentCreatorId).FirstOrDefault() : null;
                    var sDsession = await _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);
                    if (item.DoctorScheduleDaySessionId > 0)
                    {
                        weekDayName = sDsession.FirstOrDefault(p => p.Id == item.DoctorScheduleDaySessionId);
                    }
                    result.Add(new AppointmentDto()
                    {
                        Id = item.Id,
                        PatientName = item.PatientName,
                        AppointmentDate = Convert.ToDateTime(item.AppointmentDate).Date,
                        AppointmentTime = item.AppointmentTime,
                        AppointmentSerial = item.AppointmentSerial,
                        AppointmentType = item.AppointmentType,
                        AppointmentTypeName = item.AppointmentType > 0 ? ((AppointmentType)item.AppointmentType).ToString() : "n/a",
                        DoctorName = item.DoctorName,
                        DoctorScheduleId = item.DoctorScheduleId,
                        DoctorScheduleName = item.DoctorScheduleId > 0 ? item.DoctorSchedule?.ScheduleName : "n/a",
                        AppointmentCode = item.AppointmentCode,
                        AppointmentStatus = item.AppointmentStatus,
                        DoctorCode = item.DoctorCode,
                        PatientCode = item.PatientCode,
                        PatientMobileNo = patientDetails.PatientMobileNo,
                        PatientEmail = patientDetails.PatientEmail,
                        AppointmentStatusName = item.AppointmentStatus > 0 ? ((AppointmentStatus)item.AppointmentStatus).ToString() : "n/a",
                        AppointmentPaymentStatus = item.AppointmentPaymentStatus,
                        AppointmentPaymentStatusName = item.AppointmentPaymentStatus > 0 ? ((AppointmentPaymentStatus)item.AppointmentPaymentStatus).ToString() : "n/a",
                        ConsultancyType = item.ConsultancyType,
                        ConsultancyTypeName = item.ConsultancyType > 0 ? ((ConsultancyType)item.ConsultancyType).ToString() : "n/a",
                        DoctorChamberId = item.DoctorChamberId,
                        DoctorChamberName = item.DoctorChamberId > 0 ? item.DoctorSchedule?.DoctorChamber?.ChamberName : "n/a",
                        DoctorFee = item.DoctorFee,
                        PatientLocation = patientDetails?.City?.ToString(),
                        DoctorScheduleDaySessionId = item.DoctorScheduleDaySessionId,
                        ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
                        CancelledByRole = item.CancelledByRole,
                        PaymentTransactionId = item.PaymentTransactionId,
                        AppointmentCreatorRole = item.AppointmentCreatorRole,
                        BoothName = item.AppointmentCreatorRole == "agent" ? agent?.Address : "N/A",
                        AgentMasterName = item.AppointmentCreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                        AgentSupervisorName = item.AppointmentCreatorRole == "agent" ? agent?.AgentSupervisor?.AgentSupervisorOrgName : "N/A",
                    });
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            result = result.OrderByDescending(a => a.AppointmentDate).ToList();
            var list = result.OrderBy(item => item.AppointmentSerial)
                .GroupBy(item => item.AppointmentDate)
                .OrderBy(g => g.Key).Select(g => new { g }).ToList();


            return result;
        }

        public async Task<int> GetAppCountByScheduleIdSessionIdAsync(long? scheduleId, long? sessionId, DateTime? schuleDate = null)
        {
            var appointments = await _appointmentRepository.GetListAsync(a =>
        a.DoctorScheduleId == scheduleId &&
        a.DoctorScheduleDaySessionId == sessionId &&
        a.CancelledByEntityId == null &&
        a.CancelledByRole == null &&
         (a.AppointmentStatus == AppointmentStatus.Confirmed || a.AppointmentStatus == AppointmentStatus.Completed || a.AppointmentStatus == AppointmentStatus.Cancelled) &&
        a.AppointmentDate == schuleDate);

            var maxSerial = appointments
           .Where(a => !string.IsNullOrEmpty(a.AppointmentSerial))
          .Select(a => int.TryParse(a.AppointmentSerial, out int serial) ? serial : 0)
          .DefaultIfEmpty(0)
           .Max();

            return maxSerial;
        }

        public async Task<int> GetLeftBookingCount(long sessionId, long scheduleId)
        {
            int resultNp = 0;
            var numberOfPatintforScheduleSession = await _doctorScheduleSessionRepository.GetAsync(s => s.Id == sessionId && s.DoctorScheduleId == scheduleId);
            int noOfPatients = (int)numberOfPatintforScheduleSession.NoOfPatients;

            int appCounts = await GetAppCountByScheduleIdSessionIdAsync(scheduleId, sessionId);
            if (noOfPatients == appCounts)
            {
                resultNp = 0;
            }
            else if (noOfPatients > appCounts)
            {
                resultNp = (noOfPatients - appCounts);
            }
            else
            {
                resultNp = noOfPatients;
            }

            return resultNp;//noOfPatients == appCounts? 0: (int)resultNp;
        }
        /// <summary>
        /// this function used for get patient list for a specific doctor
        /// </summary>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public async Task<List<AppointmentDto>> GetPatientListByDoctorIdAsync(long doctorId)
        {
            var restultPatientList = new List<AppointmentDto>();
            try
            {
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
                //var appointments = await item.Where(d=> d.DoctorProfileId == doctorId && d.AppointmentStatus == AppointmentStatus.Confirmed).ToList();
                var appointments = item.Where(d => d.DoctorProfileId == doctorId);// && d.AppointmentStatus == AppointmentStatus.Confirmed).ToList();
                var patientIds = (from app in appointments
                                  select app.PatientProfileId).Distinct();

                foreach (var appointment in patientIds)
                {
                    var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment);
                    restultPatientList.Add(new AppointmentDto()
                    {
                        DoctorProfileId = doctorId,
                        PatientProfileId = patient.Id,
                        PatientCode = patient.PatientCode,
                        PatientName = patient.PatientName,
                        PatientMobileNo = patient.PatientMobileNo,
                        PatientEmail = patient.PatientEmail,
                        PatientLocation = patient.City,
                        BloodGroup = patient.BloodGroup,
                        GenderName = patient.Gender.ToString(),
                        PatientAge = patient.Age,


                    });
                }
                return restultPatientList;//ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
            }
            catch (Exception ex)
            {
                return restultPatientList;
            }

        }




        public async Task<int> GetAppCountByRealTimeConsultancyAsync(DateTime? aptDate)
        {
            var appointments = await _appointmentRepository.GetListAsync(a => a.AppointmentDate == aptDate && a.ConsultancyType == ConsultancyType.Instant);
            var appCount = appointments.Count();
            return appCount;
        }

        public async Task<List<AppointmentDto>> GetSearchedPatientListByDoctorIdAsync(long doctorId, string name)
        {
            var restultPatientList = new List<AppointmentDto>();
            try
            {
                var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
                //var appointments = await item.Where(d=> d.DoctorProfileId == doctorId && d.AppointmentStatus == AppointmentStatus.Confirmed).ToList();
                var appointments = item.Where(d => d.DoctorProfileId == doctorId);// && d.AppointmentStatus == AppointmentStatus.Confirmed).ToList();
                var patientIds = (from app in appointments
                                  select app.PatientProfileId).Distinct();
                foreach (var appointment in patientIds)
                {
                    var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment);
                    restultPatientList.Add(new AppointmentDto()
                    {
                        DoctorProfileId = doctorId,
                        PatientProfileId = patient.Id,
                        PatientCode = patient.PatientCode,
                        PatientName = patient.PatientName,
                        PatientMobileNo = patient.PatientMobileNo,
                        PatientEmail = patient.PatientEmail,
                        PatientLocation = patient.City
                    });
                }

                if (!string.IsNullOrEmpty(name))
                {
                    restultPatientList = restultPatientList.Where(p => p.PatientName.Contains(name)).ToList();
                }

                return restultPatientList;//ObjectMapper.Map<List<Appointment>, List<AppointmentDto>>(appointments);
            }
            catch (Exception ex)
            {
                return restultPatientList;
            }

        }
        /// <summary>
        /// this function used for getting appointment list for admin with filtering
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="dataFilter"></param>
        /// <returns></returns>
        //public async Task<List<AppointmentDto>?> GetListAppointmentListByAdminWithFilterAsync(long? userId, string? role, DataFilterModel? dataFilter, FilterModel filterModel)
        //{
        //    List<AppointmentDto>? result = null;
        //    List<Appointment>? appointments = null;
        //    List<Appointment>? itemAppointments = null;
        //    DoctorScheduleDaySession? weekDayName = null;

        //    var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
        //    var tdate1 = DateTime.Now;
        //    if (dataFilter?.toDate is null or "Invalid Date")
        //    {
        //        dataFilter.toDate = dataFilter.fromDate;
        //        tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;

        //    }
        //    else
        //    {
        //        tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
        //    }
        //    var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
        //    var allAppoinment = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);
        //    if (!string.IsNullOrEmpty(role) && role != "sgadmin")
        //    {
        //        appointments = allAppoinment.Where(c => c.AppointmentCreatorRole == "agent").ToList();

        //        var agentsByMasterSupervisors = agentDetails.Where(a => role == "masteragent" ? a.AgentMasterId == userId : a.AgentSupervisorId == userId).ToList();

        //        itemAppointments = (from app in appointments join agents in agentsByMasterSupervisors on app.AppointmentCreatorId equals agents.Id select app).ToList();
        //    }
        //    else
        //    {
        //        itemAppointments = allAppoinment.Where(a => a.AppointmentStatus > 0).ToList();//allAppoinment.Where(d => (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed || d.AppointmentStatus == AppointmentStatus.Pending || d.AppointmentStatus == AppointmentStatus.Cancelled || d.AppointmentStatus == AppointmentStatus.InProgress || d.AppointmentStatus == AppointmentStatus.Failed)).ToList();
        //    }
        //    if (!itemAppointments.Any())
        //    {
        //        return result;
        //    }
        //    //if (!string.IsNullOrEmpty(dataFilter?.name))
        //    //{
        //    //    itemAppointments = itemAppointments.Where(p => ((!string.IsNullOrEmpty(p.PatientName)) && (!string.IsNullOrEmpty(p.DoctorName))) && (p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim()) || p.DoctorName.ToLower().Contains(dataFilter.name.ToLower().Trim()))).ToList();
        //    //}
        //    //if (dataFilter?.consultancyType > 0)
        //    //{
        //    //    itemAppointments = itemAppointments.Where(p => p.ConsultancyType == dataFilter.consultancyType).ToList();
        //    //}
        //    //if (dataFilter?.appointmentStatus > 0)
        //    //{
        //    //    itemAppointments = itemAppointments.Where(p => p.AppointmentStatus == dataFilter.appointmentStatus).ToList();
        //    //}
        //    //if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
        //    //{
        //    //    itemAppointments = itemAppointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
        //    //            && p?.AppointmentDate.Value.Date <= tdate1).ToList();
        //    //}

        //    result = new List<AppointmentDto>();
        //    try
        //    {

        //        foreach (var item in itemAppointments)
        //        {
        //            var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == item.PatientProfileId);

        //            //if(item.AppointmentCreatorRole=="agent")
        //            var agent = item.AppointmentCreatorRole == "agent" ? agentDetails.Where(a => a.Id == item.AppointmentCreatorId).FirstOrDefault() : null;
        //            var sDsession = await _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);
        //            if (item.DoctorScheduleDaySessionId > 0)
        //            {
        //                weekDayName = sDsession.FirstOrDefault(p => p.Id == item.DoctorScheduleDaySessionId);
        //            }
        //            result.Add(new AppointmentDto()
        //            {
        //                Id = item.Id,
        //                PatientName = item.PatientName,
        //                AppointmentDate = Convert.ToDateTime(item.AppointmentDate).Date,
        //                AppointmentTime = item.AppointmentTime,
        //                AppointmentSerial = item.AppointmentSerial,
        //                AppointmentType = item.AppointmentType,
        //                AppointmentTypeName = item.AppointmentType > 0 ? ((AppointmentType)item.AppointmentType).ToString() : "n/a",
        //                DoctorName = item.DoctorName,
        //                DoctorScheduleId = item.DoctorScheduleId,
        //                DoctorScheduleName = item.DoctorScheduleId > 0 ? item.DoctorSchedule?.ScheduleName : "n/a",
        //                AppointmentCode = item.AppointmentCode,
        //                AppointmentStatus = item.AppointmentStatus,
        //                DoctorCode = item.DoctorCode,
        //                PatientCode = item.PatientCode,
        //                PatientMobileNo = patientDetails.PatientMobileNo,
        //                PatientEmail = patientDetails.PatientEmail,
        //                AppointmentStatusName = item.AppointmentStatus > 0 ? ((AppointmentStatus)item.AppointmentStatus).ToString() : "n/a",
        //                AppointmentPaymentStatus = item.AppointmentPaymentStatus,
        //                AppointmentPaymentStatusName = item.AppointmentPaymentStatus > 0 ? ((AppointmentPaymentStatus)item.AppointmentPaymentStatus).ToString() : "n/a",
        //                ConsultancyType = item.ConsultancyType,
        //                ConsultancyTypeName = item.ConsultancyType > 0 ? ((ConsultancyType)item.ConsultancyType).ToString() : "n/a",
        //                DoctorChamberId = item.DoctorChamberId,
        //                DoctorChamberName = item.DoctorChamberId > 0 ? item.DoctorSchedule?.DoctorChamber?.ChamberName : "n/a",
        //                DoctorFee = item.DoctorFee,
        //                PatientLocation = patientDetails?.City?.ToString(),
        //                DoctorScheduleDaySessionId = item.DoctorScheduleDaySessionId,
        //                ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
        //                CancelledByRole = item.CancelledByRole,
        //                PaymentTransactionId = item.PaymentTransactionId,
        //                AppointmentCreatorRole = item.AppointmentCreatorRole,
        //                BoothName = item.AppointmentCreatorRole == "agent" ? agent?.Address : "N/A",
        //                AgentName = item.AppointmentCreatorRole == "agent" ? agent?.FullName : "N/A",
        //                AgentMasterName = item.AppointmentCreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
        //                AgentSupervisorName = item.AppointmentCreatorRole == "agent" ? agent?.AgentSupervisor?.SupervisorName : "N/A",
        //                TotalAppointmentFee = item.TotalAppointmentFee,
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // ignored
        //    }



        //    if (!string.IsNullOrEmpty(dataFilter?.name))
        //    {
        //        result = result.Where(p => ((!string.IsNullOrEmpty(p.PatientName)) && (!string.IsNullOrEmpty(p.DoctorName)) && (!string.IsNullOrEmpty(p.AppointmentCreatorRole)) && (!string.IsNullOrEmpty(p.BoothName))) && (p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim()) || p.DoctorName.ToLower().Contains(dataFilter.name.ToLower().Trim()) ||
        //                                      p.AppointmentCreatorRole.ToLower().Contains(dataFilter.name.ToLower().Trim()) ||
        //                                      p.BoothName.ToLower().Contains(dataFilter.name.ToLower().Trim()))).ToList();
        //    }
        //    if (dataFilter?.consultancyType > 0)
        //    {
        //        result = result.Where(p => p.ConsultancyType == dataFilter.consultancyType).ToList();
        //    }
        //    if (dataFilter?.appointmentStatus > 0)
        //    {
        //        result = result.Where(p => p.AppointmentStatus == dataFilter.appointmentStatus).ToList();
        //    }
        //    if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter.toDate))
        //    {
        //        result = result.Where(p => p?.AppointmentDate >= fDate1
        //                && p?.AppointmentDate <= tdate1).ToList();
        //    }


        //    result = result.OrderByDescending(a => a.AppointmentDate).ToList();
        //    //var finalList = result.OrderByDescending(item => item.AppointmentDate).ToList();

        //    var finalResult = (from element in result
        //                       group element by element.AppointmentDate
        //              into groups
        //                       select groups.OrderByDescending(p => Convert.ToInt32(p.AppointmentSerial))).SelectMany(g => g).ToList();

        //    return finalResult;
        //}

        public async Task<PagedResultDto<AppointmentDto>> GetListAppointmentListByAdminWithFilterAsync(long? userId, string? role, DataFilterModel? dataFilter, int pageNumber, int pageSize)
        {
            // Validate pagination parameters
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            List<AppointmentDto>? result = null;
            List<Appointment>? appointments = null;
            List<Appointment>? itemAppointments = null;
            DoctorScheduleDaySession? weekDayName = null;

            var fDate1 = Convert.ToDateTime(dataFilter.fromDate).Date;
            var tdate1 = DateTime.Now;

            if (dataFilter?.toDate is null or "Invalid Date")
            {
                dataFilter.toDate = dataFilter.fromDate;
                tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
            }
            else
            {
                tdate1 = Convert.ToDateTime(dataFilter.toDate).Date;
            }

            var agentDetails = await _agentProfileRepository.WithDetailsAsync(a => a.AgentMaster, s => s.AgentSupervisor);
            var allAppointments = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule, c => c.DoctorSchedule.DoctorChamber);

            if (!string.IsNullOrEmpty(role) && role != "sgadmin")
            {
                appointments = allAppointments.Where(c => c.AppointmentCreatorRole == "agent").ToList();
                var agentsByMasterSupervisors = agentDetails.Where(a => role == "masteragent" ? a.AgentMasterId == userId : a.AgentSupervisorId == userId).ToList();
                itemAppointments = (from app in appointments
                                    join agents in agentsByMasterSupervisors on app.AppointmentCreatorId equals agents.Id
                                    select app).ToList();
            }
            else
            {
                itemAppointments = allAppointments.Where(a => a.AppointmentStatus > 0).ToList();
            }

            if (!itemAppointments.Any())
            {
                return new PagedResultDto<AppointmentDto>
                {
                    TotalCount = 0,
                    Items = result
                };
            }

            // Apply filters
            if (!string.IsNullOrEmpty(dataFilter?.name))
            {
                itemAppointments = itemAppointments.Where(p =>
                                  (!string.IsNullOrEmpty(p.PatientName) && !string.IsNullOrEmpty(p.DoctorName)) && !string.IsNullOrEmpty(p.AppointmentCreatorRole) &&
                                  (p.PatientName.ToLower().Contains(dataFilter.name.ToLower().Trim()) ||
                                   p.AppointmentCreatorRole.ToLower().Contains(dataFilter.name.ToLower().Trim()) ||
                                   p.DoctorName.ToLower().Contains(dataFilter.name.ToLower().Trim()) ||
                                   (p.AppointmentCreatorRole == "agent" && agentDetails.Any(a =>
                                    a.Id == p.AppointmentCreatorId &&
                                    !string.IsNullOrEmpty(a.Address) && a.Address.ToLower().Contains(dataFilter.name.ToLower().Trim())))
                                   )).ToList();
            }

            if (dataFilter?.consultancyType > 0)
            {
                itemAppointments = itemAppointments.Where(p => p.ConsultancyType == dataFilter.consultancyType).ToList();
            }

            if (dataFilter?.appointmentStatus > 0)
            {
                itemAppointments = itemAppointments.Where(p => p.AppointmentStatus == dataFilter.appointmentStatus).ToList();
            }

            if (!string.IsNullOrEmpty(dataFilter?.fromDate) && !string.IsNullOrEmpty(dataFilter?.toDate))
            {
                itemAppointments = itemAppointments.Where(p => p?.AppointmentDate.Value.Date >= fDate1
                        && p?.AppointmentDate.Value.Date <= tdate1).ToList();
            }

            // Sort data
            var sortedAppointments = itemAppointments
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentSerial)
                .ToList();

            // Pagination logic
            var pagedAppointments = sortedAppointments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var totalCount = sortedAppointments.Count();

            result = new List<AppointmentDto>();

            try
            {
                foreach (var item in pagedAppointments)
                {
                    var patientDetails = await _patientProfileRepository.GetAsync(p => p.Id == item.PatientProfileId);
                    var agent = item.AppointmentCreatorRole == "agent"
                        ? agentDetails.FirstOrDefault(a => a.Id == item.AppointmentCreatorId)
                        : null;
                    var sDsession = await _doctorScheduleSessionRepository.GetListAsync(s => s.IsDeleted == false);
                    if (item.DoctorScheduleDaySessionId > 0)
                    {
                        weekDayName = sDsession.FirstOrDefault(p => p.Id == item.DoctorScheduleDaySessionId);
                    }

                    result.Add(new AppointmentDto()
                    {
                        Id = item.Id,
                        PatientName = item.PatientName,
                        AppointmentDate = Convert.ToDateTime(item.AppointmentDate).Date,
                        AppointmentTime = item.AppointmentTime,
                        AppointmentSerial = item.AppointmentSerial,
                        AppointmentType = item.AppointmentType,
                        AppointmentTypeName = item.AppointmentType > 0 ? ((AppointmentType)item.AppointmentType).ToString() : "n/a",
                        DoctorName = item.DoctorName,
                        DoctorScheduleId = item.DoctorScheduleId,
                        DoctorScheduleName = item.DoctorScheduleId > 0 ? item.DoctorSchedule?.ScheduleName : "n/a",
                        AppointmentCode = item.AppointmentCode,
                        AppointmentStatus = item.AppointmentStatus,
                        DoctorCode = item.DoctorCode,
                        PatientCode = item.PatientCode,
                        PatientMobileNo = patientDetails.CreatorRole!=null? patientDetails.PatientMobileNo : patientDetails.MobileNo,
                        PatientAge =patientDetails.Age,
                        PatientEmail = patientDetails.PatientEmail,
                        AppointmentStatusName = item.AppointmentStatus > 0 ? ((AppointmentStatus)item.AppointmentStatus).ToString() : "n/a",
                        AppointmentPaymentStatus = item.AppointmentPaymentStatus,
                        AppointmentPaymentStatusName = item.AppointmentPaymentStatus > 0 ? ((AppointmentPaymentStatus)item.AppointmentPaymentStatus).ToString() : "n/a",
                        ConsultancyType = item.ConsultancyType,
                        ConsultancyTypeName = item.ConsultancyType > 0 ? ((ConsultancyType)item.ConsultancyType).ToString() : "n/a",
                        DoctorChamberId = item.DoctorChamberId,
                        DoctorChamberName = item.DoctorChamberId > 0 ? item.DoctorSchedule?.DoctorChamber?.ChamberName : "n/a",
                        DoctorFee = item.DoctorFee,
                        PatientLocation = patientDetails?.City?.ToString(),
                        DoctorScheduleDaySessionId = item.DoctorScheduleDaySessionId,
                        ScheduleDayofWeek = weekDayName?.ScheduleDayofWeek?.ToString(),
                        CancelledByRole = item.CancelledByRole,
                        PaymentTransactionId = item.PaymentTransactionId,
                        AppointmentCreatorRole = item.AppointmentCreatorRole,
                        BoothName = item.AppointmentCreatorRole == "agent" ? agent?.Address : "N/A",
                        AgentName = item.AppointmentCreatorRole == "agent" ? agent?.FullName : "N/A",
                        AgentMasterName = item.AppointmentCreatorRole == "agent" ? agent?.AgentMaster?.AgentMasterOrgName : "N/A",
                        AgentSupervisorName = item.AppointmentCreatorRole == "agent" ? agent?.AgentSupervisor?.SupervisorName : "N/A",
                        TotalAppointmentFee = item.TotalAppointmentFee,
                    });
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            return new PagedResultDto<AppointmentDto>
            {
                TotalCount = totalCount,
                Items = result.OrderByDescending(x => x.Id).ToList()
            };
        }


        public async Task<List<SessionWeekDayTimeSlotPatientCountDto>> GetListOfSessionsWithWeekDayTimeSlotPatientCountAsync(long secheduleId, DateTime date)
        {
            var result = new List<SessionWeekDayTimeSlotPatientCountDto>();
            var weekDay = date.DayOfWeek.ToString();
            var sdSessions = await _doctorScheduleSessionRepository.WithDetailsAsync(s => s.DoctorSchedule);
            var sessions = sdSessions.Where(ds => ds.DoctorScheduleId == secheduleId && ds.ScheduleDayofWeek == weekDay).ToList();

            if (sessions.Any())
            {
                foreach (var session in sessions)
                {
                    if (TimeSpan.TryParse(session.EndTime, out var endTime) &&
               DateTime.Now.TimeOfDay > endTime &&
               date.Date == DateTime.Today)
                    {
                        continue;
                    }

                    var appointments = await _appointmentRepository.GetListAsync(a => a.AppointmentDate.Value.Date == date.Date && a.DoctorScheduleDaySessionId == session.Id
                    &&
                    (a.AppointmentStatus == AppointmentStatus.Confirmed || a.AppointmentStatus == AppointmentStatus.Completed)
                    );
                    result.Add(new SessionWeekDayTimeSlotPatientCountDto
                    {
                        ScheduleId = session.DoctorScheduleId,
                        SessionId = session.Id,
                        WeekDay = session.ScheduleDayofWeek,
                        StartTime = session.StartTime,
                        EndTime = session.EndTime,
                        PatientCount = session.NoOfPatients - appointments.Count,
                    });

                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// Appoinement cancell function called for doctor side or patient side
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="cancelByid"></param>
        /// <param name="cancelByRole"></param>
        /// <returns></returns>
        public async Task<ResponseDto> CancellAppointmentAsync(long appId, long cancelByid, string cancelByRole)
        {
            var response = new ResponseDto();
            try
            {
                var itemAppointment = await _appointmentRepository.GetAsync(a => a.Id == appId);//.FindAsync(input.Id);
                itemAppointment.AppointmentStatus = AppointmentStatus.Cancelled;
                itemAppointment.CancelledByEntityId = cancelByid;
                itemAppointment.CancelledByRole = cancelByRole;



                var item = await _appointmentRepository.UpdateAsync(itemAppointment);
                //await _unitOfWorkManager.Current.SaveChangesAsync();
                var result = ObjectMapper.Map<Appointment, AppointmentDto>(item);
                if (result != null)
                {
                    response.Id = result.Id;
                    response.Value = "";
                    response.Success = true;
                    response.Message = "Consultation complete";
                }
                return response;//ObjectMapper.Map<Appointment, AppointmentDto>(item);
            }
            catch (Exception ex)
            {
                return null;
            }
            return response;
        }
        public async Task<ResponseDto> UpdateCallConsultationAppointmentAsync(string appCode)
        {
            var notificatinInput = new NotificationInputDto();
            var notificatin = new NotificationDto();
            var response = new ResponseDto();
            try
            {
                var itemAppointment = await _appointmentRepository.GetAsync(a => a.AppointmentCode == appCode);//.FindAsync(input.Id);
                itemAppointment.AppointmentStatus = AppointmentStatus.Completed;
                itemAppointment.IsCousltationComplete = true;

                //notificatinInput.MessageForCreator = null;
                notificatinInput.Message = "Your prescription is ready to view for the appointment " + itemAppointment.AppointmentCode + ". Please open it from the report page or your appointment card";

                notificatinInput.TransactionType = "Add";
                notificatinInput.CreatorEntityId = itemAppointment.DoctorProfileId;
                notificatinInput.CreatorName = itemAppointment.DoctorName;
                notificatinInput.CreatorRole = "Doctor";
                notificatinInput.CreateForName = itemAppointment.DoctorName;
                notificatinInput.NotifyToEntityId = itemAppointment.PatientProfileId;
                notificatinInput.NotifyToName = itemAppointment.PatientName;
                notificatinInput.NotifyToRole = "Patient";
                notificatinInput.NoticeFromEntity = "Appointment";
                notificatinInput.NoticeFromEntityId = itemAppointment.Id;

                var item = await _appointmentRepository.UpdateAsync(itemAppointment);
                //await _unitOfWorkManager.Current.SaveChangesAsync();
                var result = ObjectMapper.Map<Appointment, AppointmentDto>(item);
                if (result != null)
                {
                    response.Id = result.Id;
                    response.Value = "";
                    response.Success = true;
                    response.Message = "Consultation completed.";
                }

                var newNotificaitonEntity = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinInput);
                var notifictionInsert = await _notificationRepository.InsertAsync(newNotificaitonEntity);

                await _hubContext.Clients.All.BroadcastMessage();//notifictionInsert.Id

                return response;//ObjectMapper.Map<Appointment, AppointmentDto>(item);
            }
            catch (Exception ex)
            {
                return null;
            }
            return response;
        }

        public async Task UpdateAppointmentPaymentStatusAsync(string appCode, string trnId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAsync(a => a.AppointmentCode == appCode);
                if (appointment != null && appointment.AppointmentStatus != AppointmentStatus.Confirmed) //&& app.AppointmentStatus != AppointmentStatus.Confirmed)
                {
                    appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                    appointment.PaymentTransactionId = trnId;
                    appointment.AppointmentPaymentStatus = AppointmentPaymentStatus.Paid;
                    //app.FeePaid = string.IsNullOrWhiteSpace(paid_amount) ? 0 : double.Parse(paid_amount);

                    await _appointmentRepository.UpdateAsync(appointment);

                    //await SendNotification(application_code, applicant.Applicant.Mobile);
                }
            }
            catch (Exception ex) { }

        }
        /// <summary>
        /// Appointment status and payment status update after payment operation done
        /// </summary>
        /// <param name="appCode"></param>
        /// <param name="sts"></param>
        /// <returns></returns>
        public async Task<string> UpdateAppointmentStatusAfterPaymentAsync(string appCode, int sts)
        {

            var notificatinCreatorInput = new NotificationInputDto();
            var notificatinReceiverInput = new NotificationInputDto();
            var notificatinAdminInput = new NotificationInputDto();
            var notificatin = new NotificationDto();
            var result = "";
            try
            {
                var allAppointment = await _appointmentRepository.WithDetailsAsync();
                var appointment = allAppointment.Where(a => a.AppointmentCode == appCode).FirstOrDefault();
                var allTransactions = await _paymentHistoryRepository.WithDetailsAsync();
                var transactions = allTransactions.Where(p => p.application_code == appCode).FirstOrDefault();

                var doctor = await _doctorDetails.GetAsync(d => d.Id == appointment.DoctorProfileId);




                if (appointment != null && appointment.AppointmentStatus != AppointmentStatus.Confirmed) //&& app.AppointmentStatus != AppointmentStatus.Confirmed)
                {
                    await StatusUpdateAndSendNotification(sts, notificatinCreatorInput, notificatinReceiverInput, notificatinAdminInput, appointment, transactions, doctor);
                    await _appointmentRepository.UpdateAsync(appointment);

                    await _hubContext.Clients.All.BroadcastMessage();//notifictionInsert.Id

                    result = "Appointmnet and Payment Operation Completed.";
                }
                else
                {
                    await StatusUpdateAndSendNotification(sts, notificatinCreatorInput, notificatinReceiverInput, notificatinAdminInput, appointment, transactions, doctor);
                    await _appointmentRepository.UpdateAsync(appointment);

                    await _hubContext.Clients.All.BroadcastMessage();//notifictionInsert.Id

                    result = "Appointmnet and Payment Operation Completed.";
                }

                // Send for Admin Message 
                var clientKey = "SoowGood_App";
                var adminMobileNumbers = _configuration.GetSection("AdminMobileNumbers").GetSection("AdminMobileNumbers").Value;
                if (!string.IsNullOrEmpty(adminMobileNumbers))
                {
                    var SeparatedadminMobileNumbers = adminMobileNumbers.Split(",").ToList();
                    foreach (var number in SeparatedadminMobileNumbers)
                    {
                        var message = $" New scheduled created for patient {appointment.PatientName} by doctor {appointment.DoctorName} at {Convert.ToDateTime(appointment.AppointmentTime).ToString("hh:mm")} {Convert.ToDateTime(appointment.AppointmentDate).ToString("dd/MM/yyyy")}";
                        await _greenWebSmsService.SendSMS(clientKey, number, message);
                    }
                }

                //
            }
            catch (Exception ex) { }
            return result;
            }
        private IActionResult ExportAppointmentsToExcel(List<AppointmentDto> result)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new { error = ex.Message });
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Appointments");

            // Add headers
            var headers = new[]
            {
        "Appointment Code", "Doctor Name", "Patient Name", "Patient Age", "Appointment Date",
        "Appointment Time", "Status", "Total Fee"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int i = 0; i < result.Count; i++)
            {
                var apt = result[i];
                worksheet.Cells[i + 2, 1].Value = apt.AppointmentCode;
                worksheet.Cells[i + 2, 2].Value = apt.DoctorName;
                worksheet.Cells[i + 2, 3].Value = apt.PatientName;
                worksheet.Cells[i + 2, 4].Value = apt.PatientAge;
                worksheet.Cells[i + 2, 5].Value = apt.AppointmentDate?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = apt.AppointmentTime;
                worksheet.Cells[i + 2, 7].Value = apt.AppointmentStatusName;
                worksheet.Cells[i + 2, 8].Value = apt.TotalAppointmentFee;
            }

            worksheet.Cells.AutoFitColumns();

            using var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"Appointments_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return new FileContentResult(stream.ToArray(), contentType)
            {
                FileDownloadName = fileName
            };
        }

        private async Task StatusUpdateAndSendNotification(int sts, NotificationInputDto notificatinCreatorInput, NotificationInputDto notificatinReceiverInput, NotificationInputDto notificatinAdminInput, Appointment? appointment, PaymentHistory? transactions, DoctorProfile doctor)
        {
            try
            {


                if (sts == 1)
                {
                    appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                    appointment.PaymentTransactionId = transactions?.tran_id;
                    appointment.AppointmentPaymentStatus = AppointmentPaymentStatus.Paid;

                    notificatinCreatorInput.Message = "An appointment " + appointment.AppointmentCode + "  is confirmed  with patient " + appointment.PatientName
                                                          + " at " + appointment.AppointmentTime + " on " + appointment.AppointmentDate.Value.Date + " Please be prepared 5 minutes before the appointment.";

                    var sms = "Dr. " + appointment.DoctorName + ", You have a new appointment scheduled at " + Convert.ToDateTime(appointment.AppointmentTime).ToString("hh:mm") + " on " + Convert.ToDateTime(appointment.AppointmentDate).ToString("dd/MM/yyyy") + ". Please be prepared 5 minutes before the appointment.";
                    var smsForPatient = "Dear " + appointment.PatientName + ", You have a new appointment scheduled at " + Convert.ToDateTime(appointment.AppointmentTime).ToString("hh:mm") + " on " + Convert.ToDateTime(appointment.AppointmentDate).ToString("dd/MM/yyyy") + ". Please be prepared 5 minutes before the appointment.";

                    notificatinCreatorInput.TransactionType = "Add";
                    notificatinCreatorInput.CreatorEntityId = appointment.AppointmentCreatorId;
                    if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var agent = await _agentRepository.GetAsync(a => a.Id == appointment.AppointmentCreatorId);
                        notificatinCreatorInput.CreatorName = agent.FullName;
                    }
                    else
                    {
                        notificatinCreatorInput.CreatorName = appointment.PatientName;
                    }
                    notificatinCreatorInput.CreatorRole = appointment.AppointmentCreatorRole;
                    notificatinCreatorInput.CreateForName = appointment.PatientName;
                    notificatinCreatorInput.NotifyToEntityId = appointment.DoctorProfileId;
                    notificatinCreatorInput.NotifyToName = appointment.DoctorName;
                    notificatinCreatorInput.NotifyToRole = "Doctor";
                    notificatinCreatorInput.NoticeFromEntity = "Appointment";
                    notificatinCreatorInput.NoticeFromEntityId = appointment.Id;

                    var newNotificaitonForceator = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinCreatorInput);
                    var notifictionForceatorInsert = await _notificationRepository.InsertAsync(newNotificaitonForceator);

                    // Doctor SMS
                    SmsRequestParamDto smsInputDoctor = new SmsRequestParamDto();
                    smsInputDoctor.Sms = sms;
                    smsInputDoctor.Msisdn = doctor.MobileNo;
                    smsInputDoctor.CsmsId = Utility.RandomString(16);
                    var resDoctor = await _smsService.SendSmsGreenWeb(smsInputDoctor);

                    if (appointment.AppointmentCreatorRole == "patient")
                    {
                        var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment.AppointmentCreatorId);
                        await PatientSMSSend(smsForPatient, patient);
                    }
                    else if (appointment.AppointmentCreatorRole == "doctor")
                    {
                        var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment.PatientProfileId);
                        SmsRequestParamDto smsInputPatient = new SmsRequestParamDto();
                        smsInputPatient.Sms = smsForPatient;
                        smsInputPatient.Msisdn = patient.PatientMobileNo;
                        smsInputPatient.CsmsId = Utility.RandomString(16);
                        var resPatient = await _smsService.SendSmsGreenWeb(smsInputPatient);
                    }
                    else if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment.PatientProfileId);
                        SmsRequestParamDto smsInputPatient = new SmsRequestParamDto();
                        smsInputPatient.Sms = smsForPatient;
                        smsInputPatient.Msisdn = patient.PatientMobileNo;
                        smsInputPatient.CsmsId = Utility.RandomString(16);
                        var resPatient = await _smsService.SendSmsGreenWeb(smsInputPatient);
                    }
                    else
                    {
                        var patient = await _patientProfileRepository.GetAsync(p => p.Id == appointment.PatientProfileId);
                        patient.PatientMobileNo = patient.MobileNo;
                        await PatientSMSSend(smsForPatient, patient);
                    }

                        //SmsRequestParamDto smsInputNewAdmin = new SmsRequestParamDto();
                        //smsInputNewAdmin.Sms = sms;
                        //smsInputNewAdmin.Msisdn = "01897003369";
                        //smsInputNewAdmin.CsmsId = Utility.RandomString(16);
                        //var resNewAdmin = await _smsService.SendSmsGreenWeb(smsInputNewAdmin);

                        //SmsRequestParamDto smsInputOperation = new SmsRequestParamDto();
                        //smsInputOperation.Sms = sms;
                        //smsInputOperation.Msisdn = "01537204817";
                        //smsInputOperation.CsmsId = Utility.RandomString(16);
                        //var resOperation = await _smsService.SendSmsGreenWeb(smsInputOperation);

                        //SmsRequestParamDto smsInputAdmin = new SmsRequestParamDto();
                        //smsInputAdmin.Sms = sms;
                        //smsInputAdmin.Msisdn = "01676912007";
                        //smsInputAdmin.CsmsId = Utility.RandomString(16);
                        //var resAdmin = await _smsService.SendSmsGreenWeb(smsInputAdmin);

                        //SmsRequestParamDto smsInputAdminPersonal = new SmsRequestParamDto();
                        //smsInputAdminPersonal.Sms = sms;
                        //smsInputAdminPersonal.Msisdn = "01605144633";
                        //smsInputAdminPersonal.CsmsId = Utility.RandomString(16);
                        //var resAdminPersonal = await _smsService.SendSmsGreenWeb(smsInputAdminPersonal);

                        //SmsRequestParamDto smsInputMGT = new SmsRequestParamDto();
                        //smsInputMGT.Sms = sms;
                        //smsInputMGT.Msisdn = "01605144632";
                        //smsInputMGT.CsmsId = Utility.RandomString(16);
                        //var resMGT = await _smsService.SendSmsGreenWeb(smsInputMGT);

                        notificatinReceiverInput.Message = "Mr./Mrs./Ms " + appointment.PatientName + ", your appointment " + appointment.AppointmentCode + "  is confirmed  with doctor " + appointment.DoctorName
                                                              + " at " + appointment.AppointmentTime + " on " + appointment.AppointmentDate.Value.Date + " Please be prepared 5 minutes before the appointment.";


                    notificatinReceiverInput.TransactionType = "Add";
                    notificatinReceiverInput.CreatorEntityId = appointment.AppointmentCreatorId;

                    if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var agent = await _agentRepository.GetAsync(a => a.Id == appointment.AppointmentCreatorId);
                        notificatinReceiverInput.CreatorName = agent.FullName;
                    }
                    else
                    {
                        notificatinReceiverInput.CreatorName = appointment.PatientName;
                    }


                    notificatinReceiverInput.CreatorRole = appointment.AppointmentCreatorRole;
                    notificatinReceiverInput.CreateForName = appointment.PatientName;
                    notificatinReceiverInput.NotifyToEntityId = appointment.PatientProfileId;
                    notificatinReceiverInput.NotifyToName = appointment.PatientName;
                    notificatinReceiverInput.NotifyToRole = "Patient";
                    notificatinReceiverInput.NoticeFromEntity = "Appointment";
                    notificatinReceiverInput.NoticeFromEntityId = appointment.Id;

                    var newNotificaitonForReceiverEntity = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinReceiverInput);
                    var notifictionForReceiverInsert = await _notificationRepository.InsertAsync(newNotificaitonForReceiverEntity);

                    notificatinAdminInput.TransactionType = "Add";
                    notificatinAdminInput.CreatorEntityId = appointment.AppointmentCreatorId;
                    if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var agent = await _agentRepository.GetAsync(a => a.Id == appointment.AppointmentCreatorId);
                        notificatinAdminInput.CreatorName = agent.FullName;
                    }
                    else
                    {
                        notificatinAdminInput.CreatorName = appointment.PatientName;
                    }

                    notificatinAdminInput.CreatorRole = appointment.AppointmentCreatorRole;
                    notificatinAdminInput.CreateForName = appointment.PatientName;
                    notificatinAdminInput.NotifyToEntityId = 0;
                    notificatinAdminInput.NotifyToName = "SG-Admin";
                    notificatinAdminInput.NotifyToRole = "SGAdmin";
                    notificatinAdminInput.NoticeFromEntity = "Appointment";
                    notificatinAdminInput.NoticeFromEntityId = appointment.Id;

                    var newNotificaitonForAdminEntity = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinAdminInput);
                    var notifictionForAdminInsert = await _notificationRepository.InsertAsync(newNotificaitonForAdminEntity);
                }
                else if (sts == 2)
                {
                    appointment.AppointmentStatus = AppointmentStatus.Cancelled;
                    appointment.PaymentTransactionId = transactions?.tran_id;
                    appointment.AppointmentPaymentStatus = AppointmentPaymentStatus.FailedOrCancelled;
                    appointment.CancelledByEntityId = appointment.AppointmentCreatorId;
                    appointment.CancelledByRole = appointment.AppointmentCreatorRole;

                    //Notifiaction
                    notificatinCreatorInput.Message = "Your appointment is cancelled, due to a cancelled payment.";
                    notificatinCreatorInput.TransactionType = "Add";
                    notificatinCreatorInput.CreatorEntityId = appointment.AppointmentCreatorId;
                    if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var agent = await _agentRepository.GetAsync(a => a.Id == appointment.AppointmentCreatorId);
                        notificatinCreatorInput.CreatorName = agent.FullName;
                    }
                    else
                    {
                        notificatinCreatorInput.CreatorName = appointment.PatientName;
                    }
                    notificatinCreatorInput.CreatorRole = appointment.AppointmentCreatorRole;
                    notificatinCreatorInput.CreateForName = appointment.PatientName;
                    notificatinCreatorInput.NotifyToEntityId = 0;
                    notificatinCreatorInput.NotifyToName = "SG Admin";
                    notificatinCreatorInput.NotifyToRole = "Admin";
                    notificatinCreatorInput.NoticeFromEntity = "Appointment";
                    notificatinCreatorInput.NoticeFromEntityId = appointment.Id;

                    var newNotificaitonEntity = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinCreatorInput);
                    var notifictionInsert = await _notificationRepository.InsertAsync(newNotificaitonEntity);
                    // Notification
                }
                else if (sts == 3)
                {
                    appointment.AppointmentStatus = AppointmentStatus.Failed;
                    appointment.PaymentTransactionId = transactions?.tran_id;
                    appointment.AppointmentPaymentStatus = AppointmentPaymentStatus.FailedOrCancelled;
                    appointment.CancelledByEntityId = appointment.AppointmentCreatorId;
                    appointment.CancelledByRole = appointment.AppointmentCreatorRole;

                    //Notifiaction
                    notificatinCreatorInput.Message = "Your appointment is cancelled, due to a faild payment.";
                    notificatinCreatorInput.TransactionType = "Add";
                    notificatinCreatorInput.CreatorEntityId = appointment.AppointmentCreatorId;
                    if (appointment.AppointmentCreatorRole == "agent")
                    {
                        var agent = await _agentRepository.GetAsync(a => a.Id == appointment.AppointmentCreatorId);
                        notificatinCreatorInput.CreatorName = agent.FullName;
                    }
                    else
                    {
                        notificatinCreatorInput.CreatorName = appointment.PatientName;
                    }
                    notificatinCreatorInput.CreatorRole = appointment.AppointmentCreatorRole;
                    notificatinCreatorInput.CreateForName = appointment.PatientName;
                    notificatinCreatorInput.NotifyToEntityId = 0;
                    notificatinCreatorInput.NotifyToName = "SG Admin";
                    notificatinCreatorInput.NotifyToRole = "Admin";
                    notificatinCreatorInput.NoticeFromEntity = "Appointment";
                    notificatinCreatorInput.NoticeFromEntityId = appointment.Id;
                    var newNotificaitonEntity = ObjectMapper.Map<NotificationInputDto, Notification>(notificatinCreatorInput);
                    var notifictionInsert = await _notificationRepository.InsertAsync(newNotificaitonEntity);
                    // Notification
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<DashboardDto> GetAppointmentStatisticsAsync(long? doctorId, long? patientId, string? userType)
        {
            var result = new DashboardDto();
            var item = await _appointmentRepository.WithDetailsAsync(s => s.DoctorSchedule);
            var appointments = new List<Appointment>();
            if (doctorId > 0)
            {
                appointments = item.Where(d => d.DoctorProfileId == doctorId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();

            }
            if (patientId > 0 && userType != "agent")
            {
                appointments = item.Where(d => d.PatientProfileId == patientId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed)).ToList();

            }
            if (patientId > 0 && userType == "agent")
            {
                appointments = item.Where(d => d.AppointmentCreatorId == patientId && (d.AppointmentStatus == AppointmentStatus.Confirmed || d.AppointmentStatus == AppointmentStatus.Completed) && d.AppointmentCreatorRole == userType).ToList();

            }


            result.totalAppointment = appointments.Count();
            result.totalNewAppointment = appointments.Where(a => (AppointmentType)a.AppointmentType == AppointmentType.New).Count();
            result.totalFollowUpAppointment = appointments.Where(a => (AppointmentType)a.AppointmentType == AppointmentType.Followup).Count();


            return result;
        }



        private async Task PatientSMSSend(string smsForPatient, PatientProfile patient)
        {
            SmsRequestParamDto smsInputPatient = new SmsRequestParamDto();
            smsInputPatient.Sms = smsForPatient;
            smsInputPatient.Msisdn = patient.PatientMobileNo;
            smsInputPatient.CsmsId = Utility.RandomString(16);
            var resPatient = await _smsService.SendSmsGreenWeb(smsInputPatient);
        }

        public string testBuildTokenWithUserAccount(string _appId, string _appCertificate, string _channelName, string _account)
        {
            uint privilegeExpiredTs = _expireTimeInSeconds + (uint)Utils.getTimestamp();
            string token = RtcTokenBuilder.buildTokenWithUserAccount(_appId, _appCertificate, _channelName, _account, RtcTokenBuilder.Role.RolePublisher, privilegeExpiredTs);
            return token;
            //Output.WriteLine(">> token");
            //Output.WriteLine(token);
        }

        public string testBuildTokenWithUID(RtcTokenBuilerDto input)
        {
            uint privilegeExpiredTs = _expireTimeInSeconds + (uint)Utils.getTimestamp();
            string token = RtcTokenBuilder.buildTokenWithUID(input.Appid, input.AppCertificate, input.ChanelName, input.Uid, RtcTokenBuilder.Role.RolePublisher, privilegeExpiredTs);
            return token;
            //Output.WriteLine(">> token");
            //Output.WriteLine(token);
        }
        //public string testAcToken(RtcTokenBuilerDto input)
        //{
        //    uint privilegeExpiredTs = _expireTimeInSeconds + (uint)Utils.getTimestamp();
        //    AccessToken accessToken = new AccessToken(input.Appid, input.AppCertificate, input.ChanelName, input.Uid.ToString(), privilegeExpiredTs, 1);
        //    accessToken.addPrivilege(Privileges.kJoinChannel, privilegeExpiredTs);
        //    accessToken.addPrivilege(Privileges.kPublishAudioStream, privilegeExpiredTs);
        //    accessToken.addPrivilege(Privileges.kPublishVideoStream, privilegeExpiredTs);
        //    accessToken.addPrivilege(Privileges.kPublishDataStream, privilegeExpiredTs);

        //    string token = accessToken.build();
        //    return token;
        //    //Output.WriteLine(">> token");
        //    //Output.WriteLine(token);
        //}
    }

}

