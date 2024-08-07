﻿using PatientTrackingList.Data;
using PatientTrackingList.Models;

namespace PatientTrackingList.DataServices
{
    interface IClinicSlotData
    {
        public List<ClinicSlots> GetClinicSlotsList();
    }
    public class ClinicSlotData : IClinicSlotData
    {
        private readonly DataContext _context;

        public ClinicSlotData(DataContext context)
        {
            _context = context;            
        }
        
        public List<ClinicSlots> GetClinicSlotsList() 
        {
            IQueryable<ClinicSlots> clinicSlots = from s in _context.ClinicSlots
                              orderby s.SlotDate
                              select s;

            return clinicSlots.ToList();
        }
                
    }
}
