using Firebase.Database;
using Firebase.Auth;
using Daedalin.Core.OperationResult;
using SymptomTracker.Utils.Entities;
using Firebase.Database.Query;
using System.Text.Json;

namespace SymptomTracker.BLL
{
    public class FirebaseBll
    {
        #region Needs
        private const string RealtimeDB_URL = "https://symptomtracker-0702-default-rtdb.europe-west1.firebasedatabase.app/";
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

        public async Task<OperatingResult<List<string>>> GetLastTitles(eEventType eventType)
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<List<string>>.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                var Titles = await m_firebaseClient.Child(m_firebaseAuthClient.User.Uid)
                                                   .Child("Titles")
                                                   .Child($"{(int)eventType}")
                                                   .OnceSingleAsync<string>();

                var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");

                if(EncryptPassword == null || Titles == null)
                    return OperatingResult<List<string>>.OK(new List<string>());

                var Base64 = Encrypt.DecryptMessage(Titles, EncryptPassword);

                var List = JsonSerializer.Deserialize<List<string>>(Encrypt.Base64Decode(Base64));

                return OperatingResult<List<string>>.OK(List);
            }
            catch (Exception ex)
            {
                return OperatingResult<List<string>>.Fail(ex);
            }
        }

        public async Task<OperatingResult> AddLastTitles(eEventType eventType, string Title)
        {
            try
            {
                var ClientRault = await GetLastTitles(eventType);
                if (!ClientRault.Success)
                    return OperatingResult.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                var list = ClientRault.Result;

                list.Add(Title);

                var Base64 = Encrypt.Base64Encode(JsonSerializer.Serialize(list));

                var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");
                if (EncryptPassword == null)
                {
                    EncryptPassword = Guid.NewGuid().ToString();
                    await SecureStorage.Default.SetAsync("EncryptPassword", EncryptPassword);
                }

                var Data = Encrypt.EncryptMessage(Base64, EncryptPassword);

                await m_firebaseClient.Child(m_firebaseAuthClient.User.Uid)
                                      .Child("Titles")
                                      .Child($"{(int)eventType}")
                                      .PutAsync<string>(Data);

                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                    return OperatingResult.Fail(ex);
                }

        }

    }
}