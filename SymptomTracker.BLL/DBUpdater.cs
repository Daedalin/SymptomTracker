using Daedalin.Core.OperationResult;
using Firebase.Database;
using Firebase.Database.Query;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    internal static class DBUpdater
    {
        public async static Task<OperatingResult> From1VTo2V(FirebaseClient firebaseClient, string Uid)
        {
            try
            {
                var Days = await firebaseClient.Child(Uid)
                                               .Child("Dates")
                                               .OnceAsync<string>();

                foreach (var Day in Days)
                {
                    try
                    {
                        var DecryptResult = await Cryptography.Decrypt(Day.Object);
                        if (DecryptResult == null || !DecryptResult.Success)
                            continue;

                        var DayData = DecryptResult.Result.Replace("\"$type\":\"Normal\"", "\"$type\":\"Event\"")
                                                          .Replace("\"$type\":\"Stress\"", "\"$type\":\"WorkRelatedEvent\"");

                        var EncryptedData = await Cryptography.Encrypt(DayData);

                        await firebaseClient.Child(Uid)
                                            .Child("Dates")
                                            .Child(Day.Key)
                                            .PutAsync<string>(EncryptedData);
                    }
                    catch
                    {
                        continue;
                    }
                }

                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex);
            }
        }
    }
}
