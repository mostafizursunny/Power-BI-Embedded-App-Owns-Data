using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBIEmbedded_AppOwnsData.Models;
using PowerBIEmbedded_AppOwnsData.Services;
using PowerBIEmbedded_AppOwnsData.SessionManagement;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;



namespace PowerBIEmbedded_AppOwnsData.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmbedService m_embedService;
        public string DefaultGroupId = "d5e750af-f2d7-4437-a514-95c9d2c4c29d";

        public HomeController()
        {
            m_embedService = new EmbedService();
            //string pass = "Coder1234";
            //var toooo = PasswordHash.HashPassword(pass);
            //var res = PasswordHash.ValidatePassword(pass, "1000:VcMzlTrfXGWXypu5jigcl3KbqKLIIxy/:8GqHqniMIUgOCPnlWvfuGbCelUQ=");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl, bool isError = false, string type="", int code=-1)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Message = isError ? @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong> Login failed!</strong > Please try with correct username and password.
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>" : "";
            //Executed when Registration Confirmation Link from the email address is clicked. Type and Code are pair to perform an action under a certain code. Here "RegConfirm" will initiate 3 different actions
            if (type == "RegConfirm")
            {
                if(code == 1)
                    ViewBag.Message = @"<div class=""alert content-alert content-alert--purple"" role=""alert"">
                                          <div class=""content-alert__info"">
                                            <span class=""content-alert__info-icon ua-icon-warning""></span>
                                          </div>
                                          <div class=""content-alert__content"">
                                            <div class=""content-alert__heading"">Congratulation!</div>
                                            <div class=""content-alert__message"">
                                              You have successfully registered into our system. Please login with your email address and password provided in your activation email.
                                            </div>
                                          </div>
                                          <span class=""close ua-icon-alert-close content-alert__close"" data-dismiss=""alert""></span>
                                        </div>";
                else if (code == 2)
                    ViewBag.Message = @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong>Warning!</strong> Your email is already registered.
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";
                else if (code == 3)
                    ViewBag.Message = @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong>Bad request. You may not have access to this system or activation code is not valid.</strong>
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";

            }

            if (type == "ResetPassword")
            {
                if (code == 1)
                    ViewBag.Message = @"<div class=""alert content-alert content-alert--purple"" role=""alert"">
                                          <div class=""content-alert__info"">
                                            <span class=""content-alert__info-icon ua-icon-warning""></span>
                                          </div>
                                          <div class=""content-alert__content"">
                                            <div class=""content-alert__heading"">Congratulation!</div>
                                            <div class=""content-alert__message"">
                                              Password reset successful. Please check your email address for password and activation link.
                                            </div>
                                          </div>
                                          <span class=""close ua-icon-alert-close content-alert__close"" data-dismiss=""alert""></span>
                                        </div>";
                else if (code == 0)
                    ViewBag.Message = @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong>Bad request. You may not have access to this system.</strong>
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";

            }

            if (type == "SignupConfirm")
            {
                if (code == 0)
                    ViewBag.Message = @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong> Warning!</strong> This userid already exists in the system.
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";
                else if (code == 1)
                    ViewBag.Message = @"<div class=""alert alert-success"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong> Thanks for signup. Your request is waiting for admin approval. Once admin approves, you will receive confirmation email.</strong>
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";
                else if (code == 2)
                    ViewBag.Message = @"<div class=""alert alert-danger"" role=""alert"">
                                          <span class=""alert-icon ua-icon-info""></span>
                                          <strong> Warning!</strong> Exception occured in the system.
                                          <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                        </div>";

            }
            return View("Login");
        }

        [HttpPost]
        public ActionResult LoginConfirm(FormCollection collection)
        {
            string userid = collection["userid"];
            string password = collection["password"];
            string returnUrl = collection["returnUrl"];
            LoginHandler handler = new LoginHandler();
            //if (handler.ValidateLogin(userid, password))
            if ((userid == "admin@training.com" && password == "1234") || (userid == "sunny@training.com" && password == "1234") )
            {
                Session["UserId"] = userid;
                Session["Password"] = password;
                Session["FirstName"] = handler.name;
                Session["Role"] = handler.UserRole;
                if (userid == "admin@training.com")
                    Session["Country"] = "SOUTH AFRICA";
                else
                    Session["Country"] = "SWEDEN";
                CookieHelper newCookieHelper = new CookieHelper(HttpContext.Request, HttpContext.Response);
                newCookieHelper.SetLoginCookie(userid, password, true);
                if (returnUrl != "")
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home", new { Gid = "d5e750af-f2d7-4437-a514-95c9d2c4c29d" });
            }
            else
            {
                return RedirectToAction("Login", "Home", new { returnUrl = returnUrl, isError = true });
            }
            //return RedirectToAction("Index", "Property");
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(FormCollection collection)
        {
            UserModels user = new UserModels();
            string system_generated_password = CreatePassword(8);
            int code = user.ResetPasswordSelf(collection["userid"].ToString(), system_generated_password);
            return RedirectToAction("Login", "Home", new { returnUrl = "", isError = false, type = "ResetPassword", code = code });
        }

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View("ResetPassword");
        }

        [AllowAnonymous]
        public ActionResult Signup()
        {
            return View("Signup");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Signup(FormCollection collection)
        {
            UserModels user = new UserModels();
            int messageCode = user.Signup(collection["userid"], collection["password"], PasswordHash.HashPassword(collection["password"]), collection["first_name"], collection["last_name"], collection["contact"], 2);
            return RedirectToAction("Login", "Home", new { returnUrl = "", isError = false, type = "SignupConfirm", code = messageCode });
        }

        [CustomAuthorize]
        public ActionResult CreateUser()
        {
            ViewBag.Reports = m_embedService.ReportList;
            ViewBag.SystemGeneratedPassword = CreatePassword(8);
            return View();
        }

        [CustomAuthorize]
        public ActionResult ChangePassword()
        {
            ViewBag.Reports = m_embedService.ReportList;
            return View();
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection)
        {
            string userid = collection["userid"];
            string old_password = collection["old_password"];
            string new_password = collection["psw"];
            UserModels user = new UserModels();
            ViewBag.Message = user.UpdatePassword(userid, old_password, new_password);
            ViewBag.Reports = m_embedService.ReportList;
            return View();
        }

        [CustomAuthorize]
        public string CreatePassword(int length)
        {
            const string valid = "abcdefghkmnpqrstuvwxyzABCDEFGHIJMNPQRSTUVWXYZ123456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }


        [CustomAuthorize]
        [HttpPost]
        public ActionResult ManageUsers(FormCollection collection)
        {
            UserModels user = new UserModels();
            if (collection["action"] == "CREATE")
            {
                ViewBag.Message = user.CreateUser(collection["email"], collection["password"], PasswordHash.HashPassword(collection["password"]), collection["first_name"], collection["last_name"], collection["contact"], Convert.ToInt32(collection["role"]));
            }
            if (collection["action"] == "UPDATE")
            {
                ViewBag.Message = user.UpdateUser(collection["email"], collection["first_name"], collection["last_name"], collection["contact"], Convert.ToInt32(collection["role"]), Convert.ToInt32(collection["isactive"]));
            }
            ViewBag.UserList = user.GetAllUsers();
            ViewBag.Reports = m_embedService.ReportList;
            return View();
        }

        [CustomAuthorize]
        public ActionResult ManageUsers()
        {
            UserModels user = new UserModels();
            ViewBag.UserList = user.GetAllUsers();
            ViewBag.Reports = m_embedService.ReportList;
            return View();
        }

        [HttpGet]
        public ActionResult DeleteUserInfo(string userid)
        {
            UserModels user = new UserModels();
            bool result = user.DeleteUser(userid);
            var jsonResult = Json(result ? "{ result: success}" : "{ result: failure}", JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpGet]
        public ActionResult ResetUserPassword(string userid)
        {
            UserModels user = new UserModels();
            string system_generated_password = CreatePassword(8);
            string message = user.ResetUserPassword(userid, system_generated_password);
            var jsonResult = Json(message, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [AllowAnonymous]
        public ActionResult RegistrationConfirmation(string activation_code)
        {
            if (!string.IsNullOrEmpty(activation_code))
            {
                int code = new UserModels().RegistrationVerification(activation_code);
                return RedirectToAction("Login", "Home", new { returnUrl = "", isError = false, type = "RegConfirm", code = code });
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult GetUserInfo(string userid)
        {
            UserModels user = new UserModels();
            DataTable dt = user.GetUserInfo(userid);
            var jsonResult = Json(JsonConvert.SerializeObject(dt), JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [CustomAuthorize]
        public async Task<ActionResult> Index(string Gid)
        {
            ViewBag.Reports = await GetReportList(DefaultGroupId); //This will give us report list. m_embedService.ReportList will contain the list of reports generated
            ViewBag.Reports = m_embedService.ReportList;

            var client = new RestClient("https://api.powerbi.com/v1.0/myorg/availableFeatures");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "Bearer " + GetToken());
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            IRestResponse response = client.Execute(request);
            ViewBag.TokenUsages = response.Content;
            return View();
        }

        private static string GetToken()
        {
            // TODO: Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory -Version 2.21.301221612
            // and add using Microsoft.IdentityModel.Clients.ActiveDirectory

            //The client id that Azure AD created when you registered your client app.
            string clientID = ConfigurationManager.AppSettings["applicationId"];

            string redirectUri = "https://login.live.com/oauth20_desktop.srf";

            string resourceUri = "https://analysis.windows.net/powerbi/api";

            string authorityUri = "https://login.microsoftonline.com/653bd3e1-36ff-4399-b0aa-f7e63a029597";
            AuthenticationContext authContext = new AuthenticationContext(authorityUri);
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken;

            //Console.WriteLine(token);
            //Console.ReadLine();

            return token;
        }

        [CustomAuthorize]
        public async Task<ActionResult> ReportViewer(string Gid, string Rid)
        {
            ViewBag.Message = "Report Viewer Action.";
            ViewBag.Reports = await GetReportList(Gid); //This will give us report list. m_embedService.ReportList will contain the list of reports generated
            ViewBag.EmbedConfig = await EmbedReport(Gid, Rid);
            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(ViewBag.EmbedConfig.Data);
            ViewBag.EmbedJson = json;
            ViewBag.Reports = m_embedService.ReportList;
            ViewBag.ReportId = Rid;
            return View("Report", m_embedService.EmbedConfig);
        }

        [CustomAuthorize]
        public async Task<ActionResult> ReportViewerGooglemap(string Gid = "d5e750af-f2d7-4437-a514-95c9d2c4c29d", string Rid= "ec645509-c5e9-4aec-b611-8bc7daff0582")
        {
            ViewBag.Message = "Report Viewer Action.";
            ViewBag.Reports = await GetReportList(Gid); //This will give us report list. m_embedService.ReportList will contain the list of reports generated
            ViewBag.EmbedConfig = await EmbedReport(Gid, Rid);
            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(ViewBag.EmbedConfig.Data);
            ViewBag.EmbedJson = json;
            ViewBag.Reports = m_embedService.ReportList;
            ViewBag.ReportId = Rid;
            return View("ReportGmap", m_embedService.EmbedConfig);
        }

        //[CustomAuthorize]
        //public ActionResult Index()
        //{
        //    var result = new IndexConfig();
        //    var assembly = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(n => n.Name.Equals("Microsoft.PowerBI.Api")).FirstOrDefault();
        //    if (assembly != null)
        //    {
        //        result.DotNETSDK = assembly.Version.ToString(3);
        //    }
        //    return View(result);
        //}

        public async Task<ActionResult> GetReportList(string GroupId)
        {
            var result = await m_embedService.GetReportList(GroupId);
            if(result)
            {
                return Json(new { ReportList = m_embedService.ReportList, StatusCode = 200 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ReportList = new List<ReportDefinition>(), StatusCode = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EmbedReport(string GroupId, string ReportId)
        {
            var embedResult = await m_embedService.EmbedReport(null, null, GroupId, ReportId);
            if (embedResult)
            {
                //return View(m_embedService.EmbedConfig);
                return Json(new { EmbedToken = m_embedService.EmbedConfig.EmbedToken.Token, EmbedUrl = m_embedService.EmbedConfig.EmbedUrl, Id = m_embedService.EmbedConfig.EmbedToken.TokenId, ErrorMessage = "Token Received Successful.", StatusCode = 200 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { EmbedToken = "", EmbedUrl = "", Id = "", ErrorMessage = "Authentication Failed.", StatusCode = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EmbedDashboard()
        {
            var embedResult = await m_embedService.EmbedDashboard();
            if (embedResult)
            {
                return View(m_embedService.EmbedConfig);
            }
            else
            {
                return View(m_embedService.EmbedConfig);
            }
        }

        public async Task<ActionResult> EmbedTile()
        {
            var embedResult = await m_embedService.EmbedTile();
            if (embedResult)
            {
                return View(m_embedService.TileEmbedConfig);
            }
            else
            {
                return View(m_embedService.TileEmbedConfig);
            }
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            ViewBag.Message = @"<div class=""alert alert-info"" role=""alert"">
                                  <span class=""alert-icon ua-icon-info""></span>
                                  <strong>Logged out successfully.</strong>
                                  <span class=""close alert__close ua-icon-alert-close"" data-dismiss=""alert""></span>
                                </div>";
            return View("Login");
        }
    }


}
