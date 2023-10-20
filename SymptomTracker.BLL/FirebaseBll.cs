using Firebase.Database;
using Firebase.Auth;
using Daedalin.Core.OperationResult;

namespace SymptomTracker.BLL
{
    public class FirebaseBll
    {
        #region Needs
        private const string RealtimeDB_URL = "";
        private FirebaseAuthClient m_firebaseAuthClient;
        private FirebaseClient m_firebaseClient;
        #endregion           

        #region 
        public async Task<OperatingResult> Registrieren(string EMail, string Password, string UserName)
        {
            try
            {
                if (m_firebaseAuthClient == null)
                    m_firebaseAuthClient = FirebaseClientFactory.CreateLoginClient();
                else
                    m_firebaseAuthClient.SignOut();

                await m_firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(EMail, Password, UserName);
                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex, nameof(Registrieren));
            }
        }

        public async Task<OperatingResult<bool>> Login(string EMail, string Password)
        {
            try
            {
                if (m_firebaseAuthClient?.User != null)
                    return OperatingResult<bool>.OK(true);

                m_firebaseAuthClient = FirebaseClientFactory.CreateLoginClient();
                await m_firebaseAuthClient.SignInWithEmailAndPasswordAsync(EMail, Password);
                return OperatingResult<bool>.OK(true);
            }
            catch (FirebaseAuthException ex)
            {
                return new OperatingResult<bool>()
                {
                    Success = true,
                    Result = false,
                    Message = "Benutzername und Passwort simmen nicht über ein.",
                    MessageType = Daedalin.Core.Enum.eMessageType.Warning,
                    Division = "Login"
                };
            }
            catch (Exception ex)
            {
                return OperatingResult<bool>.Fail(ex, nameof(Login));
            }
        }
        #endregion

        public OperatingResult CreateFirebaseClient()
        {
            if (m_firebaseAuthClient?.User == null)
                return OperatingResult.Fail("Nicht eingelogt.", Daedalin.Core.Enum.eMessageType.Info, "Firebase");

            m_firebaseClient = new FirebaseClient(RealtimeDB_URL, new FirebaseOptions
            {                
                AuthTokenAsyncFactory = async () =>
                {
                    await Task.Delay(50);
                    return m_firebaseAuthClient.User.Credential.RefreshToken;
                }
            });

            return OperatingResult.OK();
        }
    }
}