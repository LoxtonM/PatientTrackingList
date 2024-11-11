using Microsoft.EntityFrameworkCore;
using PatientTrackingList.Data;
using PatientTrackingList.Models;

namespace PatientTrackingList.DataServices
{
    interface IAppointmentData
    {
        public Appointment GetAppointment(string? clinicno);

    }
    public class AppointmentData : IAppointmentData
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public AppointmentData(DataContext context)
        {
            _context = context;
        }

        public Appointment GetAppointment(string? clinicno)
        {
            Appointment appointment = _context.Appointments.FirstOrDefault(w => w.REFERRAL_CLINICNO == clinicno);
            return appointment;
        }
    }
}
