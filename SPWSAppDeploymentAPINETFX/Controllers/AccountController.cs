using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Host.SystemWeb;
using System.Web;
using SPWSAppDeploymentAPINETFX.Models;

namespace SPWSAppDeploymentAPINETFX.Controllers
{
    public class AccountController : ApiController
    {
        [Route("Account/Login")]
        [HttpPost]
        public string Login([FromBody] LoginRequest req)
        {
            string result = "";


            try
            {
                IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                var authService = new ActiveDirectoryAuthenticationService(authenticationManager);
                var authResult = authService.SignIn(req.Username, req.Password);
                result = Newtonsoft.Json.JsonConvert.SerializeObject(authResult);
                //IAuthenticationManager authManager = Request.GetAction
                //Request.GetOwinContext().Authentication
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                //throw;
            }



            return result;
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
