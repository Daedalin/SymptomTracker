using Daedalin.Core.Enum;
using Daedalin.Core.OperationResult;
using Firebase.Database;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        #region UploadImage
        public async Task<OperatingResult<string>> UploadImage(string imagePath, DateTime date, int EventId)
        {
            try
            {
                var FileName = $"{date.ToShortDateString()}_{EventId}.png";
                var EncryptFilePath = await Cryptography.Encrypt(imagePath, true);
                var stream = File.Open(EncryptFilePath, FileMode.Open);

                var ClientRault = await CreateFirebaseClient();
                if (!ClientRault.Success)
                    return OperatingResult<string>.Fail(ClientRault.Message, eMessageType.Error);

                var Task = m_FirebaseStorage.Child(LoginBll.GetUid())
                                            .Child(FileName)
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
        #endregion

        #region DownloadImage
        public Task<OperatingResult<string>> DownloadImage(DateTime date, int EventId)
        {
            return Task.Run(() =>
            {
                try
                {
                    var FileName = $"{date.ToShortDateString()}_{EventId}.png";
                    var encryptionPath = Path.Combine(FileSystem.CacheDirectory, $"{FileName}.encryption");

                    var ClientRault = CreateFirebaseClient().GetAwaiter()
                                                            .GetResult();
                    if (!ClientRault.Success)
                        return OperatingResult<string>.Fail(ClientRault.Message, eMessageType.Error);

                    var DownloadUrl = m_FirebaseStorage.Child(LoginBll.GetUid())
                                                       .Child(FileName)
                                                       .GetDownloadUrlAsync()
                                                       .GetAwaiter()
                                                       .GetResult();

                    using (var client = new HttpClient())
                    {
                        using (var s = client.GetStreamAsync(DownloadUrl))
                        {
                            using (var fs = new FileStream(encryptionPath, FileMode.OpenOrCreate))
                            {
                                s.Result.CopyTo(fs);
                            }
                        }
                    }

                    return Cryptography.Decrypt(encryptionPath, true)
                                       .GetAwaiter()
                                       .GetResult();
                }
                catch (Exception ex)
                {
                    return OperatingResult<string>.Fail(ex);
                }
            });
        }
        #endregion

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
