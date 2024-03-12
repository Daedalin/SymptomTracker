using Daedalin.Core.Enum;
using Daedalin.Core.OperationResult;
using Firebase.Database;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    public class StorageBll
    {
        private const string StorageBucket_URL = "symptomtracker-0702.appspot.com";
        private FirebaseStorage m_FirebaseStorage;
        private LoginBll m_LoginBll;


        public LoginBll LoginBll
        {
            get
            {
                if (m_LoginBll == null)
                    m_LoginBll = new LoginBll();
                return m_LoginBll;
            }
        }

        public async Task<OperatingResult<string>> UploadeImage(string imagePath, DateTime date, int EventId)
        {
            try
            {
                var stream = File.Open(imagePath, FileMode.Open);

                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<string>.Fail(ClientRault.Message, eMessageType.Error);

                var Task = m_FirebaseStorage.Child(LoginBll.GetUid())
                                            .Child($"{date.ToShortDateString()}_{EventId}")
                                            .PutAsync(stream);

                // Track progress of the upload
                Task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                // await the task to wait until upload completes and get the download url
                var downloadUrl = await Task;

                return OperatingResult<string>.OK(downloadUrl);
            }
            catch (Exception ex)
            {
                return OperatingResult<string>.Fail(ex);
            }
        }

        #region CreateFirebaseClient
        private async Task<OperatingResult> CreateFirebaseClient()
        {
            var LoginResult = await LoginBll.Login();

            if (!LoginResult.Result)
                return OperatingResult.Fail("Nicht eingelogt.", eMessageType.Info, "Firebase");

            m_FirebaseStorage = new FirebaseStorage(StorageBucket_URL, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(LoginBll.GetToken()),
                ThrowOnCancel = true,
            });

            return OperatingResult.OK();
        }
        #endregion
    }
}
