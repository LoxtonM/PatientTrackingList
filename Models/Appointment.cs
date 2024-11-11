using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientTrackingList.Models
{
    
    [Table("ViewPatientAppointmentDetails", Schema = "dbo")]
    public class Appointment
    {
        [Key]
        public string CGU_No { get; set; }
        public int RefID { get; set; }
        public int ReferralRefID { get; set; }
        public string? Attendance { get; set; }
        public string? CancellationReason { get; set; }
        public string? AppType { get; set; }
        public string? Title { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? FamilyName { get; set; }
        public string? FamilyNumber { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? address2 { get; set; }
        public string? address3 { get; set; }
        public string? address4 { get; set; }
        public string?postcode { get; set; }
        public DateTime? dob { get; set; }
        public string? REFERRAL_CLINICNO { get; set; }
        public string? CLINICNO { get; set; }
        public string? Clinician { get; set; }
        public string? Clinic { get; set; }
        public string? ClinicLocation { get; set; }
        public string? FACILITY { get; set; }
        public string? SeenByClinician { get; set; }
        public string? IndicationNotes { get; set; }
        public Int16? Duration { get; set; }
        public DateTime? BOOKED_DATE { get; set; }
        public DateTime? BOOKED_TIME { get; set; }
    }
}
