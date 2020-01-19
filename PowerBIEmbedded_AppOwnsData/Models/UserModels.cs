using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PowerBIEmbedded_AppOwnsData.Services;

namespace PowerBIEmbedded_AppOwnsData.Models
{
    public class UserModels
    {
        SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString);
        public UserModels()
        {

        }

        public DataTable GetUserInfo(string userid)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT  *
                             FROM dbo.UserPanel
                             Where UserId = '" + userid + "'";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            return dt;
        }

        public string CreateUser(string email, string orginal_password, string password, string firstname, string lastname, string contact, int role)
        {
            //Check if userid already exists in the database.
            string message;
            conn.Open();
            SqlCommand check_username = new SqlCommand("SELECT * FROM dbo.UserPanel WHERE (userid = @userid)", conn);
            check_username.Parameters.AddWithValue("@userid", email);
            SqlDataReader reader = check_username.ExecuteReader();
            if (reader.HasRows)
            {
                conn.Close();
                return @"<div class='alert alert-danger' role='alert'>
                            <strong> Oh snap!</ strong > This userid already exists in the system.
                        </div> ";
            }
            conn.Close();

            conn.Open();
            SqlCommand command = new SqlCommand();
            var guid = Guid.NewGuid().ToString();
            command.Connection = conn;
            command.CommandType = CommandType.Text;
            command.CommandText = "INSERT into dbo.UserPanel (UserId, PasswordEncrypted, Email, IsActive, IsRegistered, Role, FirstName, LastName, ContactNumber, ActivationCode, ActivationDate) VALUES (@userid, @password, @email, @isactive, @isregistered, @role, @firstname, @lastname, @phone, @activation_code, @activation_date)";
            command.Parameters.AddWithValue("@userid", email);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@isactive", 1);
            command.Parameters.AddWithValue("@isregistered", 0);
            command.Parameters.AddWithValue("@role", role);
            command.Parameters.AddWithValue("@firstname", firstname);
            command.Parameters.AddWithValue("@lastname", lastname);
            command.Parameters.AddWithValue("@phone", contact);
            command.Parameters.AddWithValue("@activation_code", guid);
            command.Parameters.AddWithValue("@activation_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                command.ExecuteNonQuery();
                message = @"<div class='alert alert-success' role='alert'>
                                <strong> Done. </ strong > User has been created successfully. An email has been sent to user with the userid and password.
                            </div>";
                conn.Close();
                try
                {
                    SendGridEmailService(email, firstname + " " + lastname, "Registration Confirmation : Confirmation from Datawater BI Portal", GenerateMessageBody(firstname, guid, orginal_password));
                }
                catch (Exception ex)
                {
                    message = @"<div class='alert alert-success'><strong>User Creation Successful. Unfortunately, email service is not available now. Please click this <a class=""btn btn-primary"" href=""http://" + HttpContext.Current.Request.Url.Host + "/Home/MessageCenter?type=2&activation_code=" + guid + @""">ACTIVATION LINK</a> to complete your registration.</strong></div>";
                }
                
            }
            catch (SqlException)
            {
                message = "<div class='alert alert-danger'><strong>Exception occured while inserting data into database. Please check urgently.</strong></div>";
            }
            finally
            {
                conn.Close();
            }

            return message;
        }

        //signup is same as CreateUser. But here we return integer code as response based on which notification message on Login page
        //is displayed.We cant return direct message here because the message is html message and we are redirecting to login page with 
        //from signup page with parameters sending over url.Because of html message sending problem over url, we are sending integer
        //code.
        public int Signup(string email, string orginal_password, string password, string firstname, string lastname, string contact, int role)
        {
            //Check if userid already exists in the database.
            int messageCode = 0;
            conn.Open();
            SqlCommand check_username = new SqlCommand("SELECT * FROM dbo.UserPanel WHERE (userid = @userid)", conn);
            check_username.Parameters.AddWithValue("@userid", email);
            SqlDataReader reader = check_username.ExecuteReader();
            if (reader.HasRows)
            {
                conn.Close();
                return 0;
            }
            conn.Close();

            conn.Open();
            SqlCommand command = new SqlCommand();
            var guid = Guid.NewGuid().ToString();
            command.Connection = conn;
            command.CommandType = CommandType.Text;
            command.CommandText = "INSERT into dbo.UserPanel (UserId, PasswordEncrypted, Email, IsActive, IsRegistered, Role, FirstName, LastName, ContactNumber, ActivationCode, ActivationDate) VALUES (@userid, @password, @email, @isactive, @isregistered, @role, @firstname, @lastname, @phone, @activation_code, @activation_date)";
            command.Parameters.AddWithValue("@userid", email);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@isactive", 1);
            command.Parameters.AddWithValue("@isregistered", 0);
            command.Parameters.AddWithValue("@role", role);
            command.Parameters.AddWithValue("@firstname", firstname);
            command.Parameters.AddWithValue("@lastname", lastname);
            command.Parameters.AddWithValue("@phone", contact);
            command.Parameters.AddWithValue("@activation_code", guid);
            command.Parameters.AddWithValue("@activation_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                command.ExecuteNonQuery();
                messageCode = 1;
                conn.Close();
            }
            catch (SqlException)
            {
                messageCode = 2;
            }
            finally
            {
                conn.Close();
            }

            return messageCode;
        }

        public async Task SendGridEmailService(string ToEmail, string name, string subject, string message)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            // Send a Single Email using the Mail Helper
            var from = new EmailAddress("admin@datawater.tech", "Datawater Admin");
            var to = new EmailAddress(ToEmail, name);
            var plainTextContent = "Hello, Email from the helper [SendSingleEmailAsync]!";
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public int RegistrationVerification(string code)
        {
            string message = "<div class='alert alert-danger'><strong>Invalid attempt.</strong></div>";
            int messageCode = 1;
            conn.Open();
            SqlCommand command = new SqlCommand("SELECT * FROM dbo.UserPanel WHERE (ActivationCode = @code) AND IsActive = 1", conn);
            command.Parameters.AddWithValue("@code", code);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int isRegistered = Convert.ToInt32(reader["IsRegistered"].ToString());  //check whether the user is registered or not
                    string userid = reader["UserId"].ToString();
                    reader.Close();
                    if (isRegistered == 0)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;            // <== lacking
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "Update dbo.UserPanel Set IsRegistered = 1 Where UserId = @userid";
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.ExecuteNonQuery();
                        //message = "<div class='alert alert-success'><strong>Congratulation! </strong> Your email: " + userid + " has been registered successfully. Please <a target='_blank' href='http://" + HttpContext.Current.Request.Url.Host + "/Home/RegistrationVerification?activation_code=" + code + "'>login</a> with your email address and password provided in the confirmation email.</div>";
                        //message = @"<div class=""alert content-alert content-alert--purple"" role=""alert"">
                        //              <div class=""content-alert__info"">
                        //                <span class=""content-alert__info-icon ua-icon-warning""></span>
                        //              </div>
                        //              <div class=""content-alert__content"">
                        //                <div class=""content-alert__heading"">Congratulation!</div>
                        //                <div class=""content-alert__message"">
                        //                  You have successfully registered into our system. Please login with your email address and password provided in your activation email.
                        //                </div>
                        //              </div>
                        //              <span class=""close ua-icon-alert-close content-alert__close"" data-dismiss=""alert""></span>
                        //            </div>";
                        messageCode = 1;
                    }
                    else
                    {
                        //message = "<div class='alert alert-danger'><strong>Alert! </strong> Your email: " + userid + " is already registered.</div>";
                        //message = @"<div class=""alert alert-danger"" role=""alert"">
                        //              <span class=""alert-icon ua-icon-info""></span>
                        //              <strong>Warning!</strong> Your email is already registered.
                        //              <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                        //            </div>";
                        messageCode = 2;
                    }
                    break;
                }
            }
            else
            {
                //message = "<div class='alert alert-danger'><strong>Bad request. You may not have access to this system or activation code is not valid.</strong></div>";
                messageCode = 3;
                reader.Close();
            }

            conn.Close();
            return messageCode;
        }

        private string GenerateMessageBody(string firstname, string code, string orginal_password)
        {
            string body = @"<!doctype html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <title></title>
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
	<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
	<style type=""text/css"">
		  #outlook a { padding: 0; }
		  .ReadMsgBody { width: 100%; }
		  .ExternalClass { width: 100%; }
		  .ExternalClass * { line-height:100%; }
		  body { margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
		  table, td { border-collapse:collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
		  img { border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
		  p { display: block; margin: 13px 0; }
		  
		  @media only screen and (max-width:480px) {
			@-ms-viewport { width:320px; }
			@viewport { width:320px; }
		  }
</style>

<link href=""https://fonts.googleapis.com/css?family=Open+Sans:300,400,500,700"" rel=""stylesheet"" type=""text/css"">
<style type=""text/css"">
	@import url(https://fonts.googleapis.com/css?family=Open+Sans:300,400,500,700);

  @media only screen and (min-width:480px) {
    .mj-column-per-100 { width:100%!important; }
  }
</style>
</head>
<body style=""background: #EBF2F5;"">
  
  <div class=""mj-container"" style=""background-color:#EBF2F5;"">
  
	<div style=""margin:0px auto;max-width:600px;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;padding-bottom:30px;padding-top:0px;""></td></tr></tbody></table>
	</div>
	<div style=""margin:0px auto;border-radius:4px;max-width:600px;background:#fff;"">
		<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;border-radius:4px;background:#fff;"" align=""center"" border=""0"">
			<tbody>
				<tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:1px;"">
					<div class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;"">
  
						  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:white;"" width=""100%"" border=""0"">
						  <tbody>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:20px 30px 18px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:20px;line-height:22px;text-align:left;"">Welcome to Data Water BI.</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 10px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">Hi <b>" + firstname + @"</b>, thanks for signing up</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 6px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">Please click the activation link below to activate your account. Please use your email address as userid and the password " + orginal_password + @" to login into the datawater BI portal. You can contact the support team at admin@datawater.tech for any issue.</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:11px 20px;padding-bottom:16px;padding-right:30px;padding-left:30px;"" align=""left""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:separate;"" align=""left"" border=""0""><tbody><tr><td style=""border:none;border-radius:3px;color:white;cursor:auto;padding:10px 25px;"" align=""center"" valign=""middle"" bgcolor=""#269af1""><a style=""text-decoration:none;background:#269af1;color:white;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;font-weight:400;line-height:120%;text-transform:none;margin:0px;"" target=""_blank"" href=""http://" + HttpContext.Current.Request.Url.Host + "/Home/RegistrationConfirmation?activation_code=" + code + @""">Activate Your Account</a></td></tr></tbody></table></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 20px 30px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">Thanks.<br>Datawater BI Team.</div></td>
							  </tr>
						  </tbody>
						  </table>
					</div>
				  </td>		
				</tr>
		</tbody>
	</table>
  </div>
  <div style=""margin:0px auto;max-width:600px;"">
  
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:15px 0px 0px;""><div class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" border=""0""><tbody><tr><td style=""word-wrap:break-word;font-size:0px;padding:0px;"" align=""center""><div style=""cursor:auto;color:#939daa;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:center;"">Visit
              <a href=""http://datawater.tech/"" style=""text-decoration: none; color: inherit;"">
                <span style=""border-bottom: solid 1px #b3bac1"">Datawater</span>
              </a></div></td></tr></tbody></table></div>
			  </td></tr></tbody></table></div>
	  <div style=""margin:0px auto;max-width:600px;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;padding-bottom:30px;padding-top:0px;""></td></tr></tbody></table></div>
	  </div>
</body>
</html>";
            return body;

        }

        public DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT *
                             FROM dbo.UserPanel Order By ActivationDate Desc";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            return dt;
        }

        public string[] GetUsersEmail()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT Email FROM dbo.UserPanel";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            string[] emails = new String[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                emails[i] = dt.Rows[i]["Email"].ToString();
            }
            return emails;
        }

        public string UpdateUser(string userid, string firstname, string lastname, string contact, int role, int isactive)
        {
            conn.Open();
            string qry = "UPDATE UserPanel SET FirstName = '" + firstname + "', LastName = '" + lastname + "', ContactNumber = '" + contact + "', IsActive = " + isactive + ", Role = " + role + " Where userid = '" + userid + "'";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = qry;
            cmd.ExecuteNonQuery();
            string message = @"<div class='alert alert-success'><strong>User information updated successfully.</strong></div>";
            conn.Close();
            return message;
        }

        public string ResetUserPassword(string userid, string password)
        {
            conn.Open();
            var guid = Guid.NewGuid().ToString();
            string qry = "UPDATE UserPanel SET PasswordEncrypted = '" + PasswordHash.HashPassword(password) + "', IsRegistered = 0, ActivationCode = '" + guid + "' Where userid = '" + userid + "'";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = qry;
            cmd.ExecuteNonQuery();
            conn.Close();
            SendGridEmailService(userid, GetUsersFullName(userid), "Password Reset: Your Password Has Been Reset", GeneratePasswordResetMessage(guid, password));
            string message = "Password reset successfully. New Password " + password + " is sent to user by email. User can activate his account by clicking the activation link.";
            return message;
        }

        public int ResetPasswordSelf(string userid, string password)
        {
            DataTable dt = GetUserInfo(userid);
            if (dt.Rows.Count == 0)  //no user found
                return 0;
            if(Convert.ToInt32(dt.Rows[0]["IsActive"]) == 0 || Convert.ToInt32(dt.Rows[0]["IsRegistered"]) == 0)    //for inactive or not registered users, no reset password link
                return 0;
            conn.Open();
            var guid = Guid.NewGuid().ToString();
            string qry = "UPDATE UserPanel SET PasswordEncrypted = '" + PasswordHash.HashPassword(password) + "', IsRegistered = 0, ActivationCode = '" + guid + "' Where userid = '" + userid + "'";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = qry;
            cmd.ExecuteNonQuery();
            conn.Close();
            SendGridEmailService(userid, GetUsersFullName(userid), "Password Reset: Your Password Has Been Reset", GeneratePasswordResetMessage(guid, password));
            //string message = "Password reset successfully. New Password " + password + " is sent to user by email. User can activate his account by clicking the activation link.";
            return 1;
        }

        public string GetUsersFullName(string userid)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT FirstName + ' ' + LastName as FullName FROM dbo.UserPanel";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            return dt.Rows[0]["FullName"].ToString();
        }

        public bool DeleteUser(string userid)
        {
            string qry = "Delete From UserPanel Where UserId = '" + userid + "'";
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;            // <== lacking
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = qry;
                cmd.ExecuteNonQuery();
                //string message = "<div class='alert alert-success'>User information deleted successfully.</div>";
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string UpdatePassword(string userid, string old_password, string new_password)
        {
            if(!ValidatePassword(userid, old_password))
            {
                return "<div class='alert alert-danger'>Please provide old password correctly to change password.</div>";
            }
            string qry = "UPDATE UserPanel SET PasswordEncrypted = '" + PasswordHash.HashPassword(new_password) + "' Where UserId = '" + userid + "'";
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;            // <== lacking
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = qry;
                cmd.ExecuteNonQuery();
                string message = "<div class='alert alert-success'>Password updated successfully. Please logout and then login with the new password.</div>";
                conn.Close();
                return message;
            }
            catch (Exception ex)
            {
                return "<div class='alert alert-danger'>Error while updating password.</div>";
            }
        }

        public bool ValidatePassword(string userid, string password)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT  PasswordEncrypted
                             FROM dbo.UserPanel
                             Where UserId = '" + userid + "' and IsActive = 1 and IsRegistered = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            conn.Open();
            adp.Fill(dt);
            conn.Close();
            if (dt.Rows.Count > 0)
            {
                return PasswordHash.ValidatePassword(password, dt.Rows[0]["PasswordEncrypted"].ToString());
            }
            else
                return false;

        }


        private string GeneratePasswordResetMessage(string code, string orginal_password)
        {
            string body = @"<!doctype html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <title></title>
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
	<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
	<style type=""text/css"">
		  #outlook a { padding: 0; }
		  .ReadMsgBody { width: 100%; }
		  .ExternalClass { width: 100%; }
		  .ExternalClass * { line-height:100%; }
		  body { margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
		  table, td { border-collapse:collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
		  img { border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
		  p { display: block; margin: 13px 0; }
		  
		  @media only screen and (max-width:480px) {
			@-ms-viewport { width:320px; }
			@viewport { width:320px; }
		  }
</style>

<link href=""https://fonts.googleapis.com/css?family=Open+Sans:300,400,500,700"" rel=""stylesheet"" type=""text/css"">
<style type=""text/css"">
	@import url(https://fonts.googleapis.com/css?family=Open+Sans:300,400,500,700);

  @media only screen and (min-width:480px) {
    .mj-column-per-100 { width:100%!important; }
  }
</style>
</head>
<body style=""background: #EBF2F5;"">
  
  <div class=""mj-container"" style=""background-color:#EBF2F5;"">
  
	<div style=""margin:0px auto;max-width:600px;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;padding-bottom:30px;padding-top:0px;""></td></tr></tbody></table>
	</div>
	<div style=""margin:0px auto;border-radius:4px;max-width:600px;background:#fff;"">
		<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;border-radius:4px;background:#fff;"" align=""center"" border=""0"">
			<tbody>
				<tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:1px;"">
					<div class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;"">
  
						  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""background:white;"" width=""100%"" border=""0"">
						  <tbody>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:20px 30px 18px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:20px;line-height:22px;text-align:left;"">Welcome to Data Water BI.</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 10px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">Hi,</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 6px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">This is to inform you that your password has been reset by system. Please note your new password " + orginal_password + @" to login into the datawater BI portal. You can contact the support team at admin@datawater.tech for any issue.</div></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:11px 20px;padding-bottom:16px;padding-right:30px;padding-left:30px;"" align=""left""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:separate;"" align=""left"" border=""0""><tbody><tr><td style=""border:none;border-radius:3px;color:white;cursor:auto;padding:10px 25px;"" align=""center"" valign=""middle"" bgcolor=""#269af1""><a style=""text-decoration:none;background:#269af1;color:white;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;font-weight:400;line-height:120%;text-transform:none;margin:0px;"" target=""_blank"" href=""http://" + HttpContext.Current.Request.Url.Host + "/Home/RegistrationConfirmation?activation_code=" + code + @""">Activate Your Account</a></td></tr></tbody></table></td>
							  </tr>
							  <tr><td style=""word-wrap:break-word;font-size:0px;padding:0px 30px 20px 30px;"" align=""left""><div style=""cursor:auto;color:#000000;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:left;"">Thanks.<br>Datawater BI Team.</div></td>
							  </tr>
						  </tbody>
						  </table>
					</div>
				  </td>		
				</tr>
		</tbody>
	</table>
  </div>
  <div style=""margin:0px auto;max-width:600px;"">
  
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:15px 0px 0px;""><div class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" border=""0""><tbody><tr><td style=""word-wrap:break-word;font-size:0px;padding:0px;"" align=""center""><div style=""cursor:auto;color:#939daa;font-family:Open Sans, Proxima Nova, Arial, Arial, Helvetica, sans-serif;font-size:14px;line-height:22px;text-align:center;"">Visit
              <a href=""http://datawater.tech/"" style=""text-decoration: none; color: inherit;"">
                <span style=""border-bottom: solid 1px #b3bac1"">Datawater</span>
              </a></div></td></tr></tbody></table></div>
			  </td></tr></tbody></table></div>
	  <div style=""margin:0px auto;max-width:600px;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;"" align=""center"" border=""0""><tbody><tr><td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;padding-bottom:30px;padding-top:0px;""></td></tr></tbody></table></div>
	  </div>
</body>
</html>";
            return body;

        }
    }
}