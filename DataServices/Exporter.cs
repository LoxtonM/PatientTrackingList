﻿using Microsoft.AspNetCore.Mvc;
using PatientTrackingList.Data;
using PatientTrackingList.Models;
using System.Data;


namespace PatientTrackingList.DataServices
{
    public class Exporter : Controller
    {
        public string dlFilePath;
        private readonly DataContext _context;
        private readonly PTLData _ptlData;
        private readonly WaitingListData _waitingListData;
        private readonly ClinicSlotData _clinicSlotData;

        public Exporter(DataContext context)
        {
            _context = context;
            _ptlData = new PTLData(_context);
            _waitingListData = new WaitingListData(_context);
            _clinicSlotData = new ClinicSlotData(_context);
        }
        public void ExportPTL(List<PTL> ptlToExport, string username)
        {
            DataTable table = new DataTable();
            
            table.Columns.Add("CGU Number", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Ref Date", typeof(string));
            table.Columns.Add("Clock Start", typeof(string));
            table.Columns.Add("Reason", typeof(string));
            table.Columns.Add("Urgency", typeof(string));
            table.Columns.Add("TCI Date", typeof(string));
            table.Columns.Add("Clock Ticking (Days)", typeof(int));
            table.Columns.Add("Clock Ticking (Weeks)", typeof(int));
            table.Columns.Add("Clock Days At TCI", typeof(string));
            table.Columns.Add("Comments", typeof(string));
            table.Columns.Add("Checked", typeof(string));
            table.Columns.Add("Last Updated", typeof (string));
            table.Columns.Add("User", typeof(string));

            foreach (var ptl in ptlToExport) 
            {
                int ctdays = 0;
                int ctweeks = 0;
                int tcidays = 0;
                string tcidate = "N/A";

                if (ptl.ClockStart != null)
                {
                    TimeSpan ct = DateTime.Now - ptl.ClockStart.GetValueOrDefault();
                    ctdays = (int)ct.TotalDays;
                    ctweeks = ctdays / 7;
                }
                if (ptl.TCIDate != null)
                {
                    tcidate = ptl.TCIDate.Value.ToString("dd/MM/yyyy");
                    TimeSpan tci = ptl.TCIDate.GetValueOrDefault() - ptl.ClockStart.GetValueOrDefault();
                    tcidays = (int)tci.TotalDays;
                }
                table.Rows.Add(ptl.CGUNo, 
                    ptl.PatientName, 
                    ptl.ReferralDate.Value.ToString("dd/MM/yyyy"), 
                    ptl.ClockStart.Value.ToString("dd/MM/yyyy"),
                    ptl.ReferralReason, 
                    ptl.Class, 
                    tcidate,
                    ctdays, 
                    ctweeks, 
                    tcidays, 
                    ptl.Comments, 
                    ptl.isChecked,
                    ptl.UpdatedDate,
                    ptl.UpdatedBy
                    );
            }

            //return table;
            ToCSV(table, username, "ptl");
        }

        public void ExportWL(List<WaitingList> wlToExport, string username)
        {
            DataTable table = new DataTable();

            table.Columns.Add("CGU Number", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Clinician", typeof(string));
            table.Columns.Add("Clinic", typeof(string));
            table.Columns.Add("Comments", typeof(string));
            //table.Columns.Add("Instructions", typeof(string));
            table.Columns.Add("Added Date", typeof(string));
            

            foreach (var wl in wlToExport)
            {
                int ctdays = 0;
                int ctweeks = 0;
                int tcidays = 0;
                string tcidate = "N/A";
                                
                table.Rows.Add(wl.CGU_No,
                    wl.FIRSTNAME + " " + wl.LASTNAME,
                    wl.ClinicianName,
                    wl.ClinicName,
                    wl.Comment,
                    //wl.Instructions,                    
                    wl.AddedDate.Value.ToString("dd/MM/yyyy"));
            }

            //return table;
            ToCSV(table, username, "waitinglist");
        }

        public void ExportCap(List<ClinicSlots> capToExport, string username)
        {
            DataTable table = new DataTable();

            table.Columns.Add("Clinician", typeof(string));
            table.Columns.Add("Clinic", typeof(string));
            table.Columns.Add("SlotDate", typeof(string));
            table.Columns.Add("SlotTime", typeof(string));
            table.Columns.Add("Status", typeof(string));
            


            foreach (var cs in capToExport)
            {
                int ctdays = 0;
                int ctweeks = 0;
                int tcidays = 0;
                string tcidate = "N/A";

                table.Rows.Add(cs.Clinician,
                    cs.Facility+" ("+cs.ClinicID + ")",
                    cs.SlotDate.Value.ToString("dd/MM/yyyy"),
                    cs.SlotTime.Value.ToString("HH:mm"),
                    cs.SlotStatus);
            }

            //return table;
            ToCSV(table, username, "capacityutil");
        }

        public void ToCSV(DataTable table, string username, string type)
        {
            //string filePath = $"C:\\CGU_DB\\ptl-{username}.csv";
            string fileName = $"{type}-{username}.csv";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Downloads\\" + fileName);
            StreamWriter sw = new StreamWriter(filePath, false);
            //headers
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sw.Write(table.Columns[i]);
                if (i < table.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < table.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }            
            sw.Close();

            dlFilePath = filePath;

            //DownloadFile(filePath);
        }

        [HttpGet("download")]
        //public async Task<IActionResult> DownloadFile(string filePath)
        public async Task<IActionResult> DownloadFile(string type, string username, string consultantFilter, string gcFilter, string pathwayFilter, 
            string clinicianFilter, string clinicFilter, string statusFilter, string dateTo, string dateFrom)
        {
            if (type == "ptl")
            {
                List<PTL> ptlToExport = new List<PTL>();
                ptlToExport = _ptlData.GetPTLList().ToList();

                if (pathwayFilter != null && pathwayFilter != "")
                {
                    ptlToExport = ptlToExport.Where(p => p.ReferralReason == pathwayFilter).ToList();
                }

                if (consultantFilter != null && consultantFilter != "")
                {
                    ptlToExport = ptlToExport.Where(p => p.ReferralConsultant == consultantFilter).ToList();
                }

                if (gcFilter != null && gcFilter != "")
                {
                    ptlToExport = ptlToExport.Where(p => p.ReferralGC == gcFilter).ToList();
                }

                ExportPTL(ptlToExport, username);
            }

            else if (type == "waitinglist")
            {
                List<WaitingList> wlToExport = new List<WaitingList>();
                wlToExport = _waitingListData.GetWaitingList().ToList();

                
                if (clinicianFilter != null && clinicianFilter != "")
                {
                    wlToExport = wlToExport.Where(p => p.ClinicianID == clinicianFilter).ToList();
                }

                if (clinicFilter != null && clinicFilter != "")
                {
                    wlToExport = wlToExport.Where(p => p.ClinicID == clinicFilter).ToList();
                }

                ExportWL(wlToExport, username);
            }

            else if (type == "capacityutilisation")
            {
                List<ClinicSlots> capToExport = new List<ClinicSlots>();
                capToExport = _clinicSlotData.GetClinicSlotsList().ToList();


                if (clinicianFilter != null && clinicianFilter != "")
                {
                    capToExport = capToExport.Where(p => p.Clinician == clinicianFilter).ToList();
                }

                if (clinicFilter != null && clinicFilter != "")
                {
                    capToExport = capToExport.Where(p => p.Facility == clinicFilter).ToList();
                }

                if (statusFilter != null && statusFilter != "")
                {
                    capToExport = capToExport.Where(p => p.SlotStatus == statusFilter).ToList();
                }

                //I can't pass it a date, for some reason, so it has to be a string!
                DateTime dfrom = DateTime.Parse(dateFrom);
                DateTime dTo = DateTime.Parse(dateTo);
                capToExport = capToExport.Where(p => p.SlotDate >= dfrom && p.SlotDate <= dTo).ToList();

                ExportCap(capToExport, username);
            }

            if (System.IO.File.Exists(dlFilePath))
            {
                return File(System.IO.File.ReadAllBytes(dlFilePath), "text/csv", System.IO.Path.GetFileName(dlFilePath));
            }
            return Redirect("Error");
        }


    }
}
