using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using PowerBIEmbedded_AppOwnsData.Services;

namespace PowerBIEmbedded_AppOwnsData.Models
{
    public class LoginHandler
    {
        SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString);

        public string REG_EMAIL_HTML = "";
        public bool SEND_EMAIL = false;
        public int UserRole;
        public string name;
        public LoginHandler()
        {
            REG_EMAIL_HTML = "";
            SEND_EMAIL = false;
        }

        public bool ValidateLogin(string userid, string password)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT  *
                             FROM dbo.UserPanel
                             Where UserId = '" + userid + "' and IsActive = 1 and IsRegistered = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            //UserRole = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Role"]) : -1; //1 means Admin, 2 means general, 01 means no valid user
            if (dt.Rows.Count > 0)
            {
                UserRole = Convert.ToInt32(dt.Rows[0]["Role"]);
                name = dt.Rows[0]["FirstName"].ToString();
                return PasswordHash.ValidatePassword(password, dt.Rows[0]["PasswordEncrypted"].ToString());
            } 
            else
                return false;

        }
        

    }
}