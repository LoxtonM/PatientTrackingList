using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientTrackingList.Data;
using PatientTrackingList.DataServices;
using PatientTrackingList.Models;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Data;

namespace PatientTrackingList.Pages
{
    public class CapacityUtilisationModel : PageModel
    {
        private readonly DataContext _context;
        private readonly ClinicalContext _clinicalContext;
        private readonly IClinicSlotData _clinicSlotData;
        private readonly IConfiguration _config;
        private readonly ISqlServices _sql;
        private readonly IStaffUserData _staffData;

        public CapacityUtilisationModel(DataContext context, ClinicalContext clinicalContext, IConfiguration config)
        {
            _context = context;
            _clinicalContext = clinicalContext;
            _config = config;
            _clinicSlotData = new ClinicSlotData(_context);
            Clinicians = new List<string>();
            Clinics = new List<string>();
            Stati = new List<string>();
            _sql = new SqlServices(_config);
            _staffData = new StaffUserData(_clinicalContext);
        }

        public IEnumerable<ClinicSlots> ClinicSlots { get; set; }
        public List<ClinicSlots> pageOfSlot { get; set; }
        public List<string> Clinicians { get; set; }
        public List<string> Clinics { get; set; }
        public List<string> Stati { get; set; }
                
        public int listTotal;
        public string clincianSelected;
        public string clinicSelected;
        public string statusSelected;
        public DateTime toDateSelected;
        public DateTime fromDateSelected;

        public int openSlots;
        public int usedSlots;
        public int unavailableSlots;

        public void OnGet(string? clinician, string? clinic, DateTime? fromDate, DateTime? toDate, string? status)
        {
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            string staffCode = "";
            if (User.Identity.Name is null)
            {
                Response.Redirect("Login");
            }
            else
            {
                staffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _sql.SqlWriteUsageAudit(staffCode, "", "Capacity Utilisation", _ip.GetIPAddress());
            }

            int pageSize = 20;            

            ClinicSlots = _clinicSlotData.GetClinicSlotsList();

            //for total
            //listTotal = ClinicSlots.Count();

            //drop-down menus
            Clinicians = ClinicSlots.Select(c => c.Clinician).Distinct().OrderBy(c => c).ToList();
            Clinics = ClinicSlots.Select(c => c.Facility).Distinct().OrderBy(c => c).ToList();
            Stati = ClinicSlots.Select(c => c.SlotStatus).Distinct().OrderBy(c => c).ToList();

            if (clinician != null)
            {
                ClinicSlots = ClinicSlots.Where(w => w.Clinician == clinician);
                _sql.SqlWriteUsageAudit(staffCode, $"Clinician={clinician}", "Capacity Utilisation", _ip.GetIPAddress());
                clincianSelected = clinician;
            }

            if (clinic != null)
            {
                ClinicSlots = ClinicSlots.Where(w => w.Facility == clinic);
                _sql.SqlWriteUsageAudit(staffCode, $"Clinic={clinic}", "Capacity Utilisation", _ip.GetIPAddress());
                clinicSelected = clinic;
            }

            if (fromDate == null)
            {
                fromDate = DateTime.Now;
            }
            
            if (toDate == null)
            {
                toDate = DateTime.Now.AddDays(500);
            }

            ClinicSlots = ClinicSlots.Where(w => w.SlotDate >= fromDate && w.SlotDate <= toDate);
            toDateSelected = toDate.GetValueOrDefault();
            fromDateSelected = fromDate.GetValueOrDefault();
            

            if (status != null)
            {
                ClinicSlots = ClinicSlots.Where(w => w.SlotStatus == status);
                statusSelected = status;
            }
            
            pageOfSlot = ClinicSlots.OrderBy(s => s.SlotDate).ToList();

            openSlots = pageOfSlot.Where(s => s.SlotStatus == "Open").Count();
            usedSlots = pageOfSlot.Where(s => s.SlotStatus == "Booked").Count();
            unavailableSlots = pageOfSlot.Where(s => s.SlotStatus != "Booked" && s.SlotStatus != "Open").Count();
                        
            listTotal = ClinicSlots.Count();
        }

        public void OnPost(string? clinician, string? clinic, DateTime? fromDate, DateTime? toDate, string? status)
        {            
            ClinicSlots = _clinicSlotData.GetClinicSlotsList();
            pageOfSlot = ClinicSlots.OrderBy(s => s.SlotDate).ToList();

            Clinicians = ClinicSlots.Select(c => c.ClinicianID).Distinct().OrderBy(c => c).ToList();
            Clinics = ClinicSlots.Select(c => c.ClinicID).Distinct().OrderBy(c => c).ToList();

            Response.Redirect($"CapacityUtilisation?clinician={clinician}&clinic={clinic}&fromDate={fromDate.Value.ToString("yyyy-MM-dd")}&toDate={toDate.Value.ToString("yyyy-MM-dd")}&status={status}");
        }
    }
}
