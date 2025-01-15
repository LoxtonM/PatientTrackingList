using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientTrackingList.Data;
using PatientTrackingList.Models;
using PatientTrackingList.DataServices;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;

namespace PatientTrackingList.Pages
{
    public class IndexModel : PageModel
    {   
        private readonly IConfiguration _config;
        private readonly IPTLData _ptlData;
        private readonly IStaffUserData _staffData;
        private readonly INotificationData _notificationData;
        private readonly ISqlServices _sql;
        private readonly IListStatusAdminData _statusAdminData;
        //private readonly IAreaNamesData _areaNamesData;
        private readonly IClinicVenueData _venueData;
        private readonly DataContext _context;    
        private readonly ClinicalContext _clinicalContext;
        public IndexModel(DataContext context, ClinicalContext clinicalContext, IConfiguration config)
        {
            _context = context;
            _clinicalContext = clinicalContext;
            _config = config;
            _ptlData = new PTLData(_context);
            _staffData = new StaffUserData(_clinicalContext);
            _notificationData = new NotificationData(_clinicalContext);
            _statusAdminData = new ListStatusAdminData(_context);
            //_areaNamesData = new AreaNamesData(_clinicalContext);
            _venueData = new ClinicVenueData(_clinicalContext);
            _sql = new SqlServices(_config);
        }
        public IEnumerable<PTL> PTL { get; set; }
        public List<StaffMember> consultantList { get; set; }
        public List<StaffMember> GCList { get; set; }
        public List<PTL> pageOfPTL { get; set; }
        public IEnumerable<ListStatusAdmin> listStatusAdmin { get; set; }
        //public IEnumerable<AreaNames> listAdminDistricts { get; set; }
        public IEnumerable<ClinicVenue> clinicVenueList { get; set; }
        public DateTime CurrentYear;
        public DateTime PreviousYear;
        public DateTime EighteenWeekDate;
        public DateTime FiftyTwoWeekDate;
        public DateTime LastUpdatedDate;
                
        public int listTotal;
        public int currentYearTotal;
        public int prevYearTotal;
        public int olderTotal;
        public int breachingTotal;
        public int apptDueTotal;
        public int unapptTotal;

        public string pathwaySelected;
        public bool isCheckedSelected;
        public string prioritySelected;
        public string consultantSelected;
        //public int adminDistrictSelected;
        //public string areaNameSelected;
        public string clinicCodeSelected;
        public string GCSelected;
        public string commentSearch;
        public string notificationMessage;
        public bool isLive;
        public string Message;
        public string triagePathway;
        public string allTriage;
        public string adminStatus;
        public List<string> UniqueTriagePathways { get; set; }



        [Authorize]
        public void OnGet(string? sNameSearch = null, 
            string? sCGUSearch = null, string? priorityFilter = null, bool? isChecked=false, string? pathwayFilter=null, 
            string? consultantFilter = null, string? gcFilter = null, string? commentsearch = null, string? triagePathwayFilter = null, string? statusAdmin=null, 
            string? clinicVenue = null)
        {
            try
            {
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                string staffCode = "";
                if (User.Identity.Name is null)
                {
                    Response.Redirect("Login");
                }
                else
                {
                    notificationMessage = _notificationData.GetMessage("PTLXOutage");
                                        
                    isLive = bool.Parse(_config.GetValue("IsLive", ""));
                    staffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;                    
                    _sql.SqlWriteUsageAudit(staffCode, "", "Index", _ip.GetIPAddress());
                }

                //int pageSize = 20;

                PTL = _ptlData.GetPTLList();

                consultantList = _staffData.GetConsultantsList();
                GCList = _staffData.GetGCList();
                clinicVenueList = _venueData.GetVenueList();

                CurrentYear = DateTime.Parse($"{DateTime.Now.Year}-01-01");
                PreviousYear = DateTime.Parse($"{(DateTime.Now.Year - 1)}-01-01");
                EighteenWeekDate = DateTime.Now.AddDays(-18 * 7);
                FiftyTwoWeekDate = DateTime.Now.AddDays(-365);
                listStatusAdmin = _statusAdminData.GetStatusAdminList();

                pageOfPTL = PTL.OrderBy(p => p.ClockStart).ToList(); //converting to list here makes it much faster!
                
                //variables for totals on main page
                listTotal = pageOfPTL.Count();
                currentYearTotal = pageOfPTL.Where(i => i.ReferralDate > CurrentYear).Count();
                prevYearTotal = pageOfPTL.Where(i => i.ReferralDate > PreviousYear && i.ReferralDate < CurrentYear).Count();
                olderTotal = pageOfPTL.Where(i => i.ReferralDate < PreviousYear).Count();
                breachingTotal = pageOfPTL.Where(i => i.ClockStart < EighteenWeekDate).Count();
                apptDueTotal = pageOfPTL.Where(i => i.TCIDate != null).Count();
                unapptTotal = pageOfPTL.Where(i => i.TCIDate == null).Count();
                var uniqueTriagePathways = pageOfPTL
                .Where(p => p.TriagePathway != null && p.TriagePathway != "None") 
                .Select(p => p.TriagePathway)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
                UniqueTriagePathways = uniqueTriagePathways;
                listStatusAdmin = _statusAdminData.GetStatusAdminList();
                //listAdminDistricts = _areaNamesData.GetAreaNames();

                //for filtering/searching
                if (sNameSearch != null)
                {
                    pageOfPTL = pageOfPTL.Where(p => p.PatientName.Contains(sNameSearch, StringComparison.OrdinalIgnoreCase)).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Name={sNameSearch}", "Index", _ip.GetIPAddress());
                }

                if (sCGUSearch != null)
                {
                    pageOfPTL = pageOfPTL.Where(p => p.CGUNo.Contains(sCGUSearch)).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"CGU_No={sCGUSearch}", "Index", _ip.GetIPAddress());
                }

                if (priorityFilter != null && priorityFilter != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.Class == priorityFilter).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Priority={priorityFilter}", "Index", _ip.GetIPAddress());
                    prioritySelected = priorityFilter;
                }

                if (isChecked.GetValueOrDefault())
                {
                    pageOfPTL = pageOfPTL.Where(p => !p.isChecked).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Checked={isChecked}", "Index", _ip.GetIPAddress());
                    isCheckedSelected = isChecked.GetValueOrDefault();
                }

                if (pathwayFilter != null && pathwayFilter != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.ReferralReason == pathwayFilter).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Pathway={pathwayFilter}", "Index", _ip.GetIPAddress());
                    pathwaySelected = pathwayFilter;
                }

                if (consultantFilter != null && consultantFilter != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.ReferralConsultant == consultantFilter).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Consultant={consultantFilter}", "Index", _ip.GetIPAddress());
                    consultantSelected = consultantFilter;
                }

                if (gcFilter != null && gcFilter != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.ReferralGC == gcFilter).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"GC={gcFilter}", "Index", _ip.GetIPAddress());
                    GCSelected = gcFilter;
                }

                if (commentsearch != null && commentsearch != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.Comments != null && p.Comments.Contains(commentsearch, StringComparison.OrdinalIgnoreCase)).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Comments={commentsearch}", "Index", _ip.GetIPAddress());
                    commentSearch = commentsearch;
                }
            
               if (triagePathwayFilter != null && triagePathwayFilter != "")
               {
                    pageOfPTL = pageOfPTL.Where(p => p.TriagePathway == triagePathwayFilter).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"TriagePathway={triagePathwayFilter}", "Index", _ip.GetIPAddress());
                    triagePathway = triagePathwayFilter;
                }

                if (statusAdmin != null && statusAdmin != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.Status_Admin == statusAdmin).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"Status_Admin={statusAdmin}", "Index", _ip.GetIPAddress());
                    adminStatus = statusAdmin;
                }

                /*if(adminDistrict != 0)
                {
                    string areaName = _areaNamesData.GetAreaNameDetailsByID(adminDistrict.GetValueOrDefault()).AreaName;

                    pageOfPTL = pageOfPTL.Where(p => p.PtAreaName == areaName).ToList();
                    adminDistrictSelected = adminDistrict.GetValueOrDefault();
                    areaNameSelected = areaName;
                }*/

                if(clinicVenue != null && clinicVenue != "")
                {
                    pageOfPTL = pageOfPTL.Where(p => p.ConsWLClinic == clinicVenue).ToList();
                    _sql.SqlWriteUsageAudit(staffCode, $"ClinicCode={clinicVenue}", "Index", _ip.GetIPAddress());
                    clinicCodeSelected = clinicVenue;
                }


            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        public void OnPost(string? sNameSearch=null, 
            string? sCGUSearch=null, string? priorityFilter = null, bool? isChecked=false, string? pathwayFilter=null, 
            string? consultantFilter=null, string? gcFilter=null, string? commentsearch=null, string? triagePathwayFilter = null, string? statusAdmin = null, 
            string? clinicVenue = "")
        {
            try
            {
                PTL = _ptlData.GetPTLList();

                consultantList = _staffData.GetConsultantsList();
                GCList = _staffData.GetGCList();
                clinicVenueList = _venueData.GetVenueList();
                pageOfPTL = PTL.OrderBy(p => p.ClockStart).ToList();
                listStatusAdmin = _statusAdminData.GetStatusAdminList();
                //listAdminDistricts = _areaNamesData.GetAreaNames();

                Response.Redirect($"Index?sNameSearch={sNameSearch}&sCGUSearch={sCGUSearch}" +
                    $"&priorityFilter={priorityFilter}&isChecked={isChecked}&pathwayFilter={pathwayFilter}&consultantFilter={consultantFilter}&gcFilter={gcFilter}" +
                    $"&commentsearch={commentsearch}&triagePathwayFilter={triagePathwayFilter}&statusAdmin={statusAdmin}&clinicVenue={clinicVenue}");
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

        }
    }
}