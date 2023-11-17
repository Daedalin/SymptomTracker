using Firebase.Database;
using Firebase.Auth;
using Daedalin.Core.OperationResult;
using SymptomTracker.Utils.Entities;
using Firebase.Database.Query;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.VisualBasic;
using Daedalin.Core.Enum;

namespace SymptomTracker.BLL
{
    public delegate void SampleEventHandler();
    public class FirebaseBll
    {
        #region Needs
        private const string RealtimeDB_URL = "https://symptomtracker-0702-default-rtdb.europe-west1.firebasedatabase.app/";
        private FirebaseAuthClient m_firebaseAuthClient;
        private FirebaseClient m_firebaseClient;
        #endregion

        public event SampleEventHandler PlsLogin;

        public bool HasLogin { get => m_firebaseAuthClient?.User != null; }

        #region All About Login
        #region CreateUser
        public async Task<OperatingResult<bool>> CreateUser(string EMail, string Password, string UserName)
        {
            try
            {
                if (m_firebaseAuthClient == null)
                    m_firebaseAuthClient = FirebaseClientFactory.CreateLoginClient();
                else if (m_firebaseAuthClient.User != null)
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
                    MessageType = eMessageType.Warning,
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
                {
                    PlsLogin?.Invoke();
                    return OperatingResult<bool>.OK(true);
                }

                SecureStorage.Default.Remove("EMail");
                SecureStorage.Default.Remove("Password");

                m_firebaseAuthClient.SignOut();
                PlsLogin?.Invoke();
                return OperatingResult<bool>.OK(true);
            }
            catch (Exception ex)
            {
                PlsLogin?.Invoke();
                return OperatingResult<bool>.Fail(ex, nameof(Login));
            }
        }
        #endregion
        #endregion

        #region IsKeyOK
        public async Task<OperatingResult<bool>> IsKeyOK()
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<bool>.Fail(ClientRault.Message, eMessageType.Error);

                var AllTypsOfTitles = await m_firebaseClient.Child(m_firebaseAuthClient.User.Uid)
                                                            .Child("Titles")
                                                            .OnceSingleAsync<List<string>>();

                var Titles = AllTypsOfTitles.FirstOrDefault(t => !string.IsNullOrEmpty(t));

                if (string.IsNullOrEmpty(Titles))
                    return OperatingResult<bool>.OK(true);

                var result = await DecryptMessage<List<Strings>>(Titles);

                return OperatingResult<bool>.OK(result.Success);
            }
            catch (Exception ex)
            {
                return OperatingResult<bool>.Fail(ex);
            }
        }
        #endregion

        #region GetLastTitles
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

                return await DecryptMessage<List<string>>(Titles);
            }
            catch (Exception ex)
            {
                return OperatingResult<List<string>>.Fail(ex);
            }
        }
        #endregion

        #region AddLastTitles
        public async Task<OperatingResult> AddLastTitles(eEventType eventType, string Title)
        {
            try
            {
                var ClientRault = await GetLastTitles(eventType);
                if (!ClientRault.Success)
                    return OperatingResult.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                var list = ClientRault.Result == null ? new List<string>() : ClientRault.Result;

                list.Add(Title);

                string Data = await EncryptMessage(list);

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
        #endregion

        #region GetDay
        public async Task<OperatingResult<Day>> GetDay(DateTime Date)
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<Day>.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                var Data = await m_firebaseClient.Child(m_firebaseAuthClient.User.Uid)
                                                   .Child("Dates")
                                                   .Child($"{Date.Year}-{Date.Month}-{Date.Day}")
                                                   .OnceSingleAsync<string>();

                return await DecryptMessage<Day>(Data);
            }
            catch (Exception ex)
            {
                return OperatingResult<Day>.Fail(ex);
            }
        }
        #endregion

        #region UpdateDay
        public async Task<OperatingResult> UpdateDay(Day day)
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                string Data = await EncryptMessage(day);

                await m_firebaseClient.Child(m_firebaseAuthClient.User.Uid)
                                      .Child("Dates")
                                      .Child($"{day.Date.Year}-{day.Date.Month}-{day.Date.Day}")
                                      .PutAsync<string>(Data);

                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex);
            }
        }
        #endregion

        #region Priart

        #region Crypto
        #region EncryptMessage
        private static async Task<string> EncryptMessage(object data)
        {
            var Base64 = Cryptography.Base64Encode(JsonSerializer.Serialize(data));

            var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");
            if (EncryptPassword == null)
            {
                EncryptPassword = Guid.NewGuid().ToString().Replace("-", "");
                await SecureStorage.Default.SetAsync("EncryptPassword", EncryptPassword);
            }

            return Cryptography.Encrypt(Base64, EncryptPassword);
        }
        #endregion

        #region DecryptMessage
        private static async Task<OperatingResult<T>> DecryptMessage<T>(string Titles)
        {
            if (string.IsNullOrEmpty(Titles))
                return OperatingResult<T>.OK(default(T));

            var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");
            if (string.IsNullOrEmpty(EncryptPassword))
                return OperatingResult<T>.Fail("Es wurde Kein Key eingegeben.", eMessageType.Warning, nameof(DecryptMessage));

            var Base64 = Cryptography.Decrypt(Titles, EncryptPassword);

            var List = JsonSerializer.Deserialize<T>(Cryptography.Base64Decode(Base64));

            return OperatingResult<T>.OK(List);
        }
        #endregion
        #endregion

        #region CreateFirebaseClient
        private async Task<OperatingResult> CreateFirebaseClient()
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
        #endregion
        #endregion
    }
}