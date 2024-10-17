using Daedalin.Core.Enum;
using Daedalin.Core.OperationResult;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace SymptomTracker.BLL
{

    public delegate void SampleEventHandler();
    public class LoginBll
    {
        #region Needs
        private bool m_IsDebugUser;
        private FirebaseAuthClient m_firebaseAuthClient;
        #endregion

        #region Public Prpoperties
        public event SampleEventHandler PlsLogin;
        public bool HasLogin { get => m_firebaseAuthClient?.User != null; }
        #endregion

        public string GetToken() => m_firebaseAuthClient?.User?.Credential?.RefreshToken;
        public string GetUid()
        {
#if RELEASE
            return m_firebaseAuthClient?.User?.Uid;
#else
            return m_IsDebugUser ? "Debug" : m_firebaseAuthClient?.User?.Uid;
#endif
        }

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

#if RELEASE
#else
                if (EMail == "t@t.de")
                {
                    m_IsDebugUser = true;
                    await SecureStorage.Default.SetAsync("EncryptPassword", "00000000000000000000000000000000");
                }
#endif

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
                SecureStorage.Default.Remove("EncryptPassword");

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

    }
}
