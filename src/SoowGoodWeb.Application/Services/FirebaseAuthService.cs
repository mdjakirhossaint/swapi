using Abp.Dependency;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Services
{
    public class FirebaseAuthService// : ITransientDependency
    {
        private static bool _initialized = false;

        public FirebaseAuthService()
        {
            if (!_initialized)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("wwwroot/Config/serviceAccountKey.json")
                });
                _initialized = true;
            }
        }

        public async Task<FirebaseToken?> VerifyTokenAsync(string idToken)
        {
            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decoded;
            }
            catch
            {
                return null;
            }
        }
    }
}
