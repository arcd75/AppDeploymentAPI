using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Web;
using static SPWSAppDeploymentAPINETFX.Startup;

namespace SPWSAppDeploymentAPINETFX.Models
{
    public class ActiveDirectoryAuthenticationService
    {
        public static string ADURL = "";
        public class AuthenticationResult
        {
            public AuthenticationResult(string errorMessage = "")
            {
                ErrorMessage = errorMessage;
                user = "";
            }

            public String ErrorMessage { get; private set; }
            public Boolean IsSuccess => String.IsNullOrEmpty(ErrorMessage);
            public string user { get; set; }
        }

        private readonly IAuthenticationManager authenticationManager;

        public ActiveDirectoryAuthenticationService(IAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }

        public AuthenticationResult SignIn(string username,string password)
        {
            ContextType authenticationType = ContextType.Domain;
            PrincipalContext principalContext;
            if (string.IsNullOrEmpty(ADURL))
            {
                principalContext = new PrincipalContext(authenticationType);
            }
            else
            {
                principalContext = new PrincipalContext(authenticationType, ADURL);
            }


            bool isAuthenticated = false;

            UserPrincipal userPrincipal = null;
            try
            {
                isAuthenticated = principalContext.ValidateCredentials(username, password, ContextOptions.Negotiate);
                if (isAuthenticated)
                {
                    //UserPrincipal.Current.Context.ConnectedServer
                    //string connectedServer = UserPrincipal.Current.Context.ConnectedServer;
                    userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
                }
                var result = UserPrincipal.FindByIdentity(principalContext, username);
            }
            catch (Exception ex)
            {
                isAuthenticated = false;
                userPrincipal = null;
                //Log.CreateErrorLog(ex);

            }


            if (!isAuthenticated || userPrincipal == null)
            {
                return new AuthenticationResult("Username or Password is not correct");
            }

            if (userPrincipal.IsAccountLockedOut())
            {
                // here can be a security related discussion weather it is worth 
                // revealing this information
                return new AuthenticationResult("Your account is locked out.");
            }

            if (userPrincipal.Enabled.HasValue && userPrincipal.Enabled.Value == false)
            {
                // here can be a security related discussion weather it is worth 
                // revealing this information
                return new AuthenticationResult("Your account is disabled");
            }


            var identity = CreateIdentity(userPrincipal);

            authenticationManager.SignOut(MyAuthentication.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties()
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddHours(6),
            }, identity);



            return new AuthenticationResult()
            {
                user = AuthenticateUser(userPrincipal.SamAccountName)
            };
        }

        private ClaimsIdentity CreateIdentity(UserPrincipal userPrincipal)
        {
            var identity = new ClaimsIdentity(MyAuthentication.ApplicationCookie, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            try
            {
                identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Active Directory"));

                var dbUser = AuthenticateUser(userPrincipal.SamAccountName);
                identity.AddClaim(new Claim(ClaimTypes.Name, userPrincipal.SamAccountName));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userPrincipal.SamAccountName));
                if (dbUser != null)
                {

                    //identity.AddClaim(new Claim(ClaimTypes.Role, dbUser.Role));

                    //identity.AddClaim(new Claim("Image", image));

                    //        new Claim("FullName", user.SamAccountName),
                    //        new Claim(ClaimTypes.Role, dbUser.Role),
                    //        new Claim("IdNumber",dbUser.IdNumber),
                    //        new Claim("image",base64Image)
                }

                if (!String.IsNullOrEmpty(userPrincipal.EmailAddress))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress));
                }

            }
            catch (Exception ex)
            {
                //Log.CreateErrorLog(ex);
                //throw;
            }

            // add your own claims if you need to add more information stored on the cookie

            return identity;
        }

        public string AuthenticateUser(string username)
        {
            string currentUser = null;
            try
            {
                using (var adc = new ADContext())
                {
                    currentUser = adc.ADUsers.FirstOrDefault(u => u.DomainName == username).DomainName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //Log.CreateErrorLog(ex);
                //throw;
            }
            return currentUser;
        }
    }
}