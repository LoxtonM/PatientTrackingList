using Microsoft.EntityFrameworkCore;
using PatientTrackingList.Models;

namespace PatientTrackingList.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<PTL> PTL {  get; set; }
        public DbSet<Activity> Activity { get; set; }
        public DbSet<Diary> Diary { get; set; }
        public DbSet<Letters> Letters { get; set; }
        public DbSet<WaitingList> WaitingList { get; set; }
        public DbSet<Patients> Patients { get; set; }
        public DbSet<StaffMembers> StaffMembers { get; set; }
        public DbSet<ClinicSlots> ClinicSlots { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<TrueWaitingList> TrueWaitingLists { get; set; }
        public DbSet<Icp> Icps { get; set; }
        public DbSet<IcpCancer> IcpCancers { get; set; }
        public DbSet<IcpGeneral> IcpGenerals { get; set; }
        public DbSet<ClinicalOutcome> ClinicalOutcomes { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ListStatusAdmin> ListStatusAdmins { get; set; }
        public bool isDesc { get; set; }
    }
}
