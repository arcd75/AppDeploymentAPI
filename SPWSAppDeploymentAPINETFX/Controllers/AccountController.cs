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
            string result = string.Empty;
            try
            {
                ADUser user = ADUser.local.FirstOrDefault(x => x.DomainName.Equals(req.Username));
                if (user != null)
                {
                    IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                    var authService = new ActiveDirectoryAuthenticationService(authenticationManager);
                    var authResult = authService.SignIn(req.Username, req.Password);
                    authResult.user = user.UserName;
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(authResult);
                }
                else
                {
                    AuthenticationResult res = new AuthenticationResult { ErrorMessage = "Unauthorize Login!", IsSuccess = false };
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(res);
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class AuthenticationResult
        {
            public string ErrorMessage { get; set; }
            public bool IsSuccess { get; set; }
            public string user { get; set; }
        }
    }
}
