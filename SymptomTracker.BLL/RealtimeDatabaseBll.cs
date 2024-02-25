using Firebase.Database;
using Firebase.Auth;
using Daedalin.Core.OperationResult;
using SymptomTracker.Utils.Entities;
using Firebase.Database.Query;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.VisualBasic;
using Daedalin.Core.Enum;
using Org.BouncyCastle.Utilities;

namespace SymptomTracker.BLL
{
    public class RealtimeDatabaseBll
    {
        #region Needs
        private const string RealtimeDB_URL = "https://symptomtracker-0702-default-rtdb.europe-west1.firebasedatabase.app/";
        private FirebaseClient m_firebaseClient;
        private LoginBll m_LoginBll;
        #endregion

        public LoginBll LoginBll
        {
            get
            {
                if(m_LoginBll == null)
                    m_LoginBll = new LoginBll();
                return m_LoginBll;
            }
        }

        #region IsKeyOK
        public async Task<OperatingResult<bool>> IsKeyOK()
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<bool>.Fail(ClientRault.Message, eMessageType.Error);

                var Data = await m_firebaseClient.Child(LoginBll.GetUid()).OnceSingleAsync<object>();

                if (Data == null)
                    return OperatingResult<bool>.OK(true, "Noch keine Daten Vorhanden", eMessageType.Info);

                var AllTypsOfTitles = await m_firebaseClient.Child(LoginBll.GetUid())
                                                            .Child("Titles")
                                                            .OnceSingleAsync<List<string>>();

                var Titles = AllTypsOfTitles.FirstOrDefault(t => !string.IsNullOrEmpty(t));

                if (string.IsNullOrEmpty(Titles))
                    return OperatingResult<bool>.OK(true);

                var result = await Cryptography.DecryptAndDeserializeMessage<List<string>>(Titles);

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

                var Titles = await m_firebaseClient.Child(LoginBll.GetUid())
                                                   .Child("Titles")
                                                   .Child($"{(int)eventType}")
                                                   .OnceSingleAsync<string>();

                return await Cryptography.DecryptAndDeserializeMessage<List<string>>(Titles);
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

                string Data = await Cryptography.SerializeAndEncryptMessage(list);

                await m_firebaseClient.Child(LoginBll.GetUid())
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

                var Data = await m_firebaseClient.Child(LoginBll.GetUid())
                                                   .Child("Dates")
                                                   .Child($"{Date.Year}-{Date.Month}-{Date.Day}")
                                                   .OnceSingleAsync<string>();

                return await Cryptography.DecryptAndDeserializeMessage<Day>(Data);
            }
            catch (Exception ex)
            {
                return OperatingResult<Day>.Fail(ex);
            }
        }
        #endregion

        public async Task<OperatingResult> UpdateDB()
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult.Fail(ClientRault.Message, eMessageType.Error);

                var DBVersion = await m_firebaseClient.Child(LoginBll.GetUid())
                                                      .Child("Settings")
                                                      .Child("DB_Version")
                                                      .OnceSingleAsync<int>();

                if (DBVersion < DBUpdater.CurrentDBVersion)
                {
                    if (DBVersion < 2)
                        await DBUpdater.From1VTo2V(m_firebaseClient, LoginBll.GetUid());

                    await m_firebaseClient.Child(LoginBll.GetUid())
                                          .Child("Settings")
                                          .Child("DB_Version")
                                          .PutAsync(DBUpdater.CurrentDBVersion);

                }
                return OperatingResult.OK($"Datenbankupdate wurde erfolgreich durchgeführt.", eMessageType.Info);
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex);
            }
        }

        #region UpdateDay
        public async Task<OperatingResult> UpdateDay(Day day)
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                string Data = await Cryptography.SerializeAndEncryptMessage(day);

                await m_firebaseClient.Child(LoginBll.GetUid())
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

        #region CreateFirebaseClient
        private async Task<OperatingResult> CreateFirebaseClient()
        {
            var LoginResult = await LoginBll.Login();

            if (!LoginResult.Result)
                return OperatingResult.Fail("Nicht eingelogt.", eMessageType.Info, "Firebase");

            m_firebaseClient = new FirebaseClient(RealtimeDB_URL, new FirebaseOptions
            {
                AuthTokenAsyncFactory = async () =>
                {
                    await Task.Delay(50);
                    return LoginBll.GetToken();
                }
            });

            return OperatingResult.OK();
        }
        #endregion
        #endregion
    }
}