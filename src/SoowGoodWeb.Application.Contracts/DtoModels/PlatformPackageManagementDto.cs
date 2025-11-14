using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class PlatformPackageManagementDto : FullAuditedEntityDto<long>
    {
        public string? PackageRequestCode { get; set; }
        //public PlatformPackage? PlatformPackage { get; set; }
        public long? PlatformPackageId { get; set; }
        //public PatientProfile? PatientProfile { get; set; }
        public string? PackageName { get; set; }
        public long? PatientProfileId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientCode { get; set; }
        public string? PatientMobileNo { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? AppointmentDate { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PackageFee { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public AppointmentStatus? AppointmentStatus { get; set; }
        public AppointmentPaymentStatus? AppointmentPaymentStatus { get; set; }
        public string? PaymentTransactionId { get; set; }
    }
}
