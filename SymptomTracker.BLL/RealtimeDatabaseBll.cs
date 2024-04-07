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
using Aspose.Pdf;

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
                if (m_LoginBll == null)
                    m_LoginBll = new LoginBll();
                return m_LoginBll;
            }
        }

        #region GeneratingReports
        public async Task<OperatingResult<List<Day>>> GeneratingReports(eEventType eventType, DateOnly StartDay, DateOnly EndDay)
        {
            try
            {
                List<Day> Result = new List<Day>();

                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<List<Day>>.Fail(ClientRault.Message, eMessageType.Error);

                for (DateOnly current = StartDay.AddDays(StartDay.Day * -1); current > EndDay.AddDays(StartDay.Day * -1); current.AddMonths(1))
                {
                    //Testen
                }             


                var Days = await m_firebaseClient.Child(m_LoginBll.GetUid())
                                                 .Child("Dates")
                                                 .Child(StartDay.ToString("yyyy-MM"))
                                                 .OnceAsync<string>();

                //Parallel
                foreach (var Day in Days)
                {
                    //Date filter with key

                    var DecryptResult = await Cryptography.Decrypt(Day.Object);
                    if (DecryptResult == null || !DecryptResult.Success)
                        continue;

                    var Data = JsonSerializer.Deserialize<Day>(DecryptResult.Result);

                    if (Data.Events.Any(t => t.EventType == eventType))
                    {
                        Data.Events.RemoveAll(t => t.EventType != eventType);
                        Result.Add(Data);
                    }
                }

                //Mehere Seiten?
                Document document = new Document();
                Aspose.Pdf.Page page = document.Pages.Add();
                Table table = new Table();
                table.Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));
                table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));

                foreach (var Day in Result.OrderBy(t => t.Date))
                {
                    Row row = table.Rows.Add();
                    row.Cells.Add(Day.Date.ToShortDateString()).ColSpan = 4;


                    foreach (var Event in Day.Events)
                    {
                        row = table.Rows.Add();
                        row.Cells.Add(Event.Name);
                        row.Cells.Add(Event.Description); // Es gibt warp
                        if (Event.FullTime)
                            row.Cells.Add("Ganztägig").ColSpan = 2;
                        else
                        {
                            row.Cells.Add(Event.StartTime.ToString());
                            row.Cells.Add(Event.EndTime.ToString());
                        }
                    }
                    row = table.Rows.Add();
                    row.Cells.Add().ColSpan = 4;
                }

                page.Paragraphs.Add(table);


                document.Save("C:\\Temp\\Generated-PDFV2.pdf");

                return OperatingResult<List<Day>>.OK(Result);
            }
            catch (Exception ex)
            {
                return OperatingResult<List<Day>>.Fail(ex);
            }
        }
        #endregion

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

        #region GetSettings
        public async Task<OperatingResult<Settings>> GetSettings()
        {
            try
            {
                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<Settings>.Fail(ClientRault.Message, Daedalin.Core.Enum.eMessageType.Error);

                var Data = await m_firebaseClient.Child(LoginBll.GetUid())
                                                 .Child("Settings")
                                                 .OnceSingleAsync<Settings>();

                return OperatingResult<Settings>.OK(Data);
            }
            catch (Exception ex)
            {
                return OperatingResult<Settings>.Fail(ex);
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
                                                 .Child(Date.ToString("yyyy-MM"))
                                                 .Child(Date.ToString("dd"))
                                                 .OnceSingleAsync<string>();

                return await Cryptography.DecryptAndDeserializeMessage<Day>(Data);
            }
            catch (Exception ex)
            {
                return OperatingResult<Day>.Fail(ex);
            }
        }
        #endregion

        #region UpdateDB
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
                    {
                        var result = await DBUpdater.From1VTo2V(m_firebaseClient, LoginBll.GetUid());
                        if (!result.Success)
                            return result;
                    }
                    if (DBVersion < 3)
                    {
                        var result = await DBUpdater.From2VTo3V(m_firebaseClient, LoginBll.GetUid());
                        if (!result.Success)
                            return result;
                    }

                    await m_firebaseClient.Child(LoginBll.GetUid())
                                          .Child("Settings")
                                          .Child("DB_Version")
                                          .PutAsync(DBUpdater.CurrentDBVersion);

                    return OperatingResult.OK($"Datenbankupdate wurde erfolgreich durchgeführt.", eMessageType.Info);
                }

                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex);
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

                string Data = await Cryptography.SerializeAndEncryptMessage(day);

                await m_firebaseClient.Child(LoginBll.GetUid())
                                      .Child("Dates")
                                      .Child(day.Date.ToString("yyyy-MM"))
                                      .Child(day.Date.ToString("dd"))
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