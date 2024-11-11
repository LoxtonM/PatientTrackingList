using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientTrackingList.Data;
using PatientTrackingList.DataServices;
using PatientTrackingList.Models;

namespace PatientTrackingList.Pages
{
    public class AppointmentModel : PageModel
    {

        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly ISqlServices _sql;
        private readonly IStaffData _staffData;
        private readonly IIcpData _icpData;
        private readonly IConfiguration Configuration;
        private readonly INotificationData _notificationData;
        private readonly IAppointmentData _appointmentData;

        public AppointmentModel(DataContext context, IConfiguration config, IConfiguration configuration)
        {
            _context = context;
            _config = config;
            _sql = new SqlServices(_config);
            _staffData = new StaffData(_context);
            _icpData = new IcpData(_context);
            _notificationData = new NotificationData(_context);
            _appointmentData = new AppointmentData(_context);
            Configuration = configuration;
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
                notificationMessage = _notificationData.GetMessage();

                isLive = bool.Parse(_config.GetValue("IsLive", ""));
                staffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _sql.SqlWriteUsageAudit(staffCode, "", "Index");
            }

            appointments = _appointmentData.GetAppointment(sClinicno);
        }
    }
}
