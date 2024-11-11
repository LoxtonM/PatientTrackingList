using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientTrackingList.Models
{
    [Keyless]
    [Table("ViewPatientReferralDetails", Schema = "dbo")]
    public class TrueWaitingList
    {
        [Key]
        public int refid { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public DateTime? dob { get; set; }
        public string? NHSNo { get; set; }
        public string? Complete { get; set; }
        public DateTime? RefDate { get; set; }
        public DateTime? ClockStartDate { get; set; }
        public DateTime? ClockStopDate { get; set; }
        public string? Status_Admin {  get; set; }
        public string?PATHWAY { get; set; }
        public string?GC { get; set; }
        public string? LeadClinician { get; set; }
        public string? RefType { get; set; }
        public string? ReferringClinician { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? address2 { get; set; }
        public string? address3 { get; set; }
        public string? address4 { get; set; }
        public string? postcode { get; set; }
        public string? AdminContact { get; set; }
        public string? RefClass { get; set; }
        public string? INDICATION { get; set; }
        public string? REASON_FOR_REFERRAL { get; set; }
        public string? ReferringFacility { get; set; }
        public string? CancellationReason { get; set; }
        public string? CLINICNO { get; set; }
        public DateTime? BreachDate { get; set; }

    }
}
