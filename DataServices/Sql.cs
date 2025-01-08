using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientTrackingList.Data;
using PatientTrackingList.Models;
using System.Data;
using System.Net;
using System.Xml.Linq;

namespace PatientTrackingList.DataServices
{
    interface ISqlServices
    {
        public void SqlUpdateComments(string comments, int isChecked, string username, int id, string ipAddress);
        public string GetOldComments(int id);
        public void SqlWriteAuditUpdate(string comments, string oldComments, string username, int id, string ipAddress);
        public void SqlWriteUsageAudit(string username, string searchTerm, string formName, string ipAddress);
        public string ValidateLogin(UserDetails user);
    }
    public class SqlServices : ISqlServices
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection _con;
        private readonly SqlCommand _cmd;
        public SqlServices(IConfiguration config)
        {
            _config = config;
            _con = new SqlConnection(_config.GetConnectionString("ConString"));
            _cmd = new SqlCommand("", _con);
        }


        public void SqlUpdateComments(string comments, int isChecked, string username, int id, string ipAddress)
        {
            string oldComment = GetOldComments(id);
            
            if(comments == null) { comments = ""; }

            SqlCommand cmd = new SqlCommand("[sp_PTLUpdateComments]", _con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@staffCode", SqlDbType.VarChar).Value = username;
            cmd.Parameters.Add("@isChecked", SqlDbType.SmallInt).Value = isChecked;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@comment", SqlDbType.VarChar).Value = comments;

            _con.Open();            
            cmd.ExecuteNonQuery();
            _con.Close();

            _con.Open();                
            _cmd.ExecuteNonQuery();
            _con.Close();

            SqlWriteAuditUpdate(comments, oldComment, username, id, ipAddress);
        }

        public string GetOldComments(int id)
        {
            string commentOld = "";
            _cmd.CommandText = "select comments from PTLValidationData where ID = " + id;
            
            _con.Open();
            SqlDataReader reader = _cmd.ExecuteReader();
            if(reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    commentOld = reader.GetString(0);
                }
            }
            _con.Close();
            
            return commentOld;
        }

        public void SqlWriteAuditUpdate(string comments, string oldComments, string username, int id, string ipAddress)
        {            
            SqlCommand cmd = new SqlCommand("[sp_WriteAuditUpdate]", _con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@staffCode", SqlDbType.VarChar).Value = username;
            cmd.Parameters.Add("@form", SqlDbType.VarChar).Value = "comments";
            cmd.Parameters.Add("@tableName", SqlDbType.VarChar).Value = "PTLValidationData";
            cmd.Parameters.Add("@recordkey", SqlDbType.VarChar).Value = id.ToString();
            cmd.Parameters.Add("@oldValue", SqlDbType.VarChar).Value = oldComments;
            cmd.Parameters.Add("@newValue", SqlDbType.VarChar).Value = comments;
            cmd.Parameters.Add("machineName", SqlDbType.VarChar).Value = Dns.GetHostByAddress(ipAddress).HostName.Substring(0, 10);


            _con.Open();
            cmd.ExecuteNonQuery();
            _con.Close();
        }

        public void SqlWriteUsageAudit(string username, string searchTerm, string formName, string ipAddress)
        {
            SqlCommand cmd = new SqlCommand("[sp_CreateAudit]", _con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@staffCode", SqlDbType.VarChar).Value = username;
            cmd.Parameters.Add("@form", SqlDbType.VarChar).Value = formName;
            cmd.Parameters.Add("@database", SqlDbType.VarChar).Value = "PTL-X";
            cmd.Parameters.Add("@searchTerm", SqlDbType.VarChar).Value = searchTerm;
            cmd.Parameters.Add("@machine", SqlDbType.VarChar).Value = Dns.GetHostByAddress(ipAddress).HostName.Substring(0, 10);

            _con.Open();
            cmd.ExecuteNonQuery();
            _con.Close();
        }

        public string ValidateLogin(UserDetails user)
        {   
            SqlCommand cmd = new SqlCommand("sp_ValidateUserLogin", _con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@LoginID", user.EMPLOYEE_NUMBER);
            cmd.Parameters.AddWithValue("@LoginPassword", user.PASSWORD);

            _con.Open();
            string result = cmd.ExecuteScalar().ToString();
            _con.Close();

            return result;
        }

    }
}
