using Firebase.Auth;
using Firebase.Auth.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    internal class FirebaseClientFactory
    {
        private const string API_KEY = "AIzaSyAvIiymPrSqZmvKIT1jzmBywZWa3-Efujo";
        private const string AUTH_DOMAIN = "symptomtracker-0702.firebaseapp.com";

        public static FirebaseAuthClient CreateLoginClient()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = API_KEY,
                AuthDomain = AUTH_DOMAIN,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };

            return new FirebaseAuthClient(config);
        }
    }
}
