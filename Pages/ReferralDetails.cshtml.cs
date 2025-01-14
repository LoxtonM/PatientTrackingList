using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientTrackingList.Data;
using PatientTrackingList.Models;
using PatientTrackingList.DataServices;
using System.Web;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicalXPDataConnections.Data;

namespace PatientTrackingList.Pages
{
    public class ReferralDetailsModel : PageModel
    {
        private readonly ClinicalContext _context;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _config;
        private readonly IPTLData _ptlData;        
        private readonly IStaffUserData _staffData;
        private readonly IActivityData _activityData;
        private readonly IDiaryData _diaryData;
        private readonly ILetterData _letterData;
        private readonly ISqlServices _sql;

        public ReferralDetailsModel(ClinicalContext context, DataContext dataContext, IConfiguration config)
        {
            _context = context;
            _dataContext = dataContext;
            _config = config;
            _ptlData = new PTLData(_dataContext);
            _staffData = new StaffUserData(_context);;
            _activityData = new ActivityData(_context);
            _diaryData = new DiaryData(_context);
            _letterData = new LetterData(_dataContext);
            _sql = new SqlServices(_config);
        }

        public PTL RefDet { get; set; }
        public List<ActivityItem> ActivityList { get; set; }        
        public List<Diary> DiaryList { get; set; }
        public List<Letters> LetterList { get; set; }

        public DateTime EighteenWeekDate;
        public DateTime FiftyTwoWeekDate;
        public string Message;
        public bool isSuccess;
        public string consultSelected;
        public string gcSelected;
        public string pathSelected;
        public string triPathway;
        public string priorFilter;

        [Authorize]
        public void OnGet(int id, string? message = "", bool? success = false, string? sNameSearch = null,
            string? sCGUSearch = null, string? priorityFilter = null, bool? isChecked = false, string? pathwayFilter = null,
            string? consultantFilter = null, string? gcFilter = null, string? commentsearch = null, string? triagePathwayFilter = null)
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
                    staffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                    _sql.SqlWriteUsageAudit(staffCode, $"PTL ID={id.ToString()}", "Details", _ip.GetIPAddress());
                }

                Message = message;
                isSuccess = success.GetValueOrDefault();
                RefDet = _ptlData.GetPTLEntryDetails(id);
                int refID = RefDet.RefID;
                var Referral = _activityData.GetActivityDetails(refID);
                ActivityList = _activityData.GetActivityListByClinicno(Referral.CLINICNO);                
                DiaryList = _diaryData.GetDiaryListByRefID(refID);
                LetterList = _letterData.GetLetterList(refID);

                EighteenWeekDate = RefDet.ClockStart.GetValueOrDefault().AddDays(18 * 7);
                FiftyTwoWeekDate = RefDet.ClockStart.GetValueOrDefault().AddDays(365);

                consultSelected = consultantFilter;
                triPathway = triagePathwayFilter;
                gcSelected = gcFilter;
                pathSelected = pathwayFilter;
                priorFilter = priorityFilter;

            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        public void OnPost(int id, string? comments="", bool? isChecked=false, string? sNameSearch = null,
            string? sCGUSearch = null, string? priorityFilter = null, string? pathwayFilter = null,
            string? consultantFilter = null, string? gcFilter = null, string? commentsearch = null, string? triagePathwayFilter = null)
        {
            try
            {                
                RefDet = _ptlData.GetPTLEntryDetails(id);
                int refID = RefDet.RefID;
                var Referral = _activityData.GetActivityDetails(refID);
                ActivityList = _activityData.GetActivityListByClinicno(Referral.CLINICNO);
                DiaryList = _diaryData.GetDiaryList(refID);
                LetterList = _letterData.GetLetterList(refID);

                EighteenWeekDate = RefDet.ClockStart.GetValueOrDefault().AddDays(18 * 7);
                FiftyTwoWeekDate = RefDet.ClockStart.GetValueOrDefault().AddDays(365);
                consultSelected = HttpUtility.UrlEncode(consultantFilter);
                triPathway = HttpUtility.UrlEncode(triagePathwayFilter);
                gcSelected = HttpUtility.UrlEncode(gcFilter);
                pathSelected = HttpUtility.UrlEncode(pathwayFilter);
                priorFilter = HttpUtility.UrlEncode(priorityFilter);

                int iChecked = 0; //because SQL needs it to be a binary value
                
                if(isChecked.GetValueOrDefault()) { iChecked = 1; }

                string username = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);

                _sql.SqlUpdateComments(comments, iChecked, username, id, _ip.GetIPAddress());
                                                
                isSuccess = true;
                string message = "Saved.";
                Response.Redirect($"ReferralDetails?id={id.ToString()}&message={message}&success={isSuccess}&consultantFilter={consultSelected}" +
                    $"&triagePathwayFilter={triPathway}&gcFilter={gcSelected}&pathwayFilter={pathSelected}&priorityFilter={priorFilter}");
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}