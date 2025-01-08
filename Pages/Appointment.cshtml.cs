using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientTrackingList.Data;
using PatientTrackingList.DataServices;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;

namespace PatientTrackingList.Pages
{
    public class AppointmentModel : PageModel
    {

        private readonly DataContext _context;
        private readonly ClinicalContext _clinicalContext;
        private readonly IConfiguration _config;
        private readonly ISqlServices _sql;
        private readonly IStaffUserData _staffData;
        private readonly IIcpData _icpData;
        private readonly INotificationData _notificationData;
        private readonly IAppointmentData _appointmentData;

        public AppointmentModel(DataContext context, ClinicalContext clinicalContext, IConfiguration config)
        {
            _context = context;
            _clinicalContext = clinicalContext;
            _config = config;
            _sql = new SqlServices(_config);
            _staffData = new StaffUserData(_clinicalContext);;
            _icpData = new IcpData(_context);
            _notificationData = new NotificationData(_clinicalContext);
            _appointmentData = new AppointmentData(_clinicalContext);
        }

        public Appointment appointments { get; set; }
        public string notificationMessage;
        public bool isLive;
        public void OnGet(string? sClinicno)
        {
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
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _sql.SqlWriteUsageAudit(staffCode, "", "Index", _ip.GetIPAddress());
            }

            appointments = _appointmentData.GetAppointmentByClinicno(sClinicno);
        }
    }
}
