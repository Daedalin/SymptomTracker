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

        #region All About Login
        #region CreateUser
        public async Task<OperatingResult<bool>> CreateUser(string EMail, string Password, string UserName)
        {
            try
            {
                if (m_firebaseAuthClient == null)
                    m_firebaseAuthClient = FirebaseClientFactory.CreateLoginClient();
                else
                    m_firebaseAuthClient.SignOut();

                await m_firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(EMail, Password, UserName);
                return OperatingResult<bool>.OK(true);
            }
            catch (Exception ex)
            {
                return OperatingResult<bool>.Fail(ex, nameof(CreateUser));
            }
        }
        #endregion

        #region Login
        public async Task<OperatingResult<bool>> Login()
        {
            try
            {
                if (m_firebaseAuthClient?.User != null)
                {
                    await Task.Delay(5);
                    return OperatingResult<bool>.OK(true);
                }

                var EMail = await SecureStorage.Default.GetAsync("EMail");
                var Password = await SecureStorage.Default.GetAsync("Password");

                if (EMail != null && Password != null)
                    return await Login(EMail, Password);
                else
                    return OperatingResult<bool>.OK(false);
            }
            catch (Exception ex)
            {
                return OperatingResult<bool>.Fail(ex, nameof(Login));
            }
        }
        public async Task<OperatingResult<bool>> Login(string EMail, string Password)
        {
            try
            {
                m_firebaseAuthClient = FirebaseClientFactory.CreateLoginClient();
                await m_firebaseAuthClient.SignInWithEmailAndPasswordAsync(EMail, Password);

                await SecureStorage.Default.SetAsync("EMail", EMail);
                await SecureStorage.Default.SetAsync("Password", Password);

                return OperatingResult<bool>.OK(true);
            }
            catch (FirebaseAuthException)
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

        #region Logout
        public OperatingResult<bool> Logout()
        {
            try
            {
                if (m_firebaseAuthClient?.User == null)
                    return OperatingResult<bool>.OK(true);

                SecureStorage.Default.Remove("EMail");
                SecureStorage.Default.Remove("Password");

                m_firebaseAuthClient.SignOut();
                return OperatingResult<bool>.OK(true);
            }
            catch (Exception ex)
            {
                return OperatingResult<bool>.Fail(ex, nameof(Login));
            }
        }
        #endregion
        #endregion

        public async Task<OperatingResult> CreateFirebaseClient()
        {
            var LoginResult = await Login();

            if (!LoginResult.Result)
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