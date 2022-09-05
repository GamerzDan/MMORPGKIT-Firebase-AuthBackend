using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    public partial class UIMmoLogin : UIBase
    {
        /// <summary>
        /// Use this new custom method to login, hook it into the LoginButton's Action or call it manually
        /// It tries to login using the entered username and password and calls the OnLoginCustom callback
        /// OnLoginCustom handles the whole logic of login, registration and password reset depending on error message received
        /// </summary>
        public void tryMMOLogin()
        {
            Debug.Log("proceedMMOLogin " + Username + Password);
            PlayerPrefs.SetString(keyUsername, Username);
            //MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLoginCustom);
            MMOClientInstance.Singleton.RequestFirebaseLogin(Username, Password, OnFirebaseAuthCustom);
        }
        public void OnFirebaseAuthCustom(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseFirebaseAuthLoginMessage response)
        {
            LoggingIn = false;
            Debug.Log(responseCode);
            Debug.Log(response.response);
            //If firebaseLogin was not success
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Timeout Error", "MMO Server did not respond in time");
                return;
            }
            if (responseCode != AckResponseCode.Success)
            {
                FirebaseErrorRes error = JsonUtility.FromJson<FirebaseErrorRes>(response.response);
                UISceneGlobal.Singleton.ShowMessageDialog(error.error.code + " Error", error.error.message);
                return;
            }
            //If success, try Kit's login
            MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLoginCustom);
        }

        /// <summary>
        /// Reset the password using Firebase's workflow (email)
        /// The server will call firebase's password reset API, which in-turn sends password reset email to the user
        /// The user resets his password via the email
        /// </summary>
        public void OnClickResetPassword()
        {
            // Clear stored username and password
            PlayerPrefs.SetString(keyUsername, string.Empty);
            PlayerPrefs.SetString(keyPassword, string.Empty);
            PlayerPrefs.Save();

            UISceneGlobal uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(Username))
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_IS_EMPTY.ToString()));
                return;
            }

            //APIManager.instance.onClickResetPassword(Username, this);
        }

        /// <summary>
        /// This is the callback from our login attempt, depending on the response/error received we will either do new user registration
        /// or reset/update their password if their firebase password changed and does not match the password in MMO Database
        /// </summary>
        /// <param name="responseHandler"></param>
        /// <param name="responseCode"></param>
        /// <param name="response"></param>
        public void OnLoginCustom(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserLoginMessage response)
        {
            Debug.Log("OnLoginMMOCustom");
            LoggingIn = false;
            //If below code is called, instead call 
            Debug.Log("OnLoginCustomResponseCode: " + responseCode);
            Debug.Log("OnLoginCustomResponseMessage: " + response.message);
            //If login was not success
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Timeout Error", "MMO Server did not respond in time");
                return;
            }
            if (responseCode != AckResponseCode.Success)
            {
                //Backend reports incorrect user/password so either mismatch or user not exist, lets try to create the user first
                //But if user exists, that means need to reset/update password
                if (response.message.ToString() == "UI_ERROR_INVALID_USERNAME_OR_PASSWORD")
                {
                    MMOClientInstance.Singleton.RequestUserRegister(Username, Password, "", OnLoginRegister);
                }
                return;
            }
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                Debug.Log("OnLoginCustomFail: " + response.message);
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            if (toggleAutoLogin != null && toggleAutoLogin.isOn)
            {
                // Store username-password                
                PlayerPrefs.SetString(keyUsername, Username);
                PlayerPrefs.SetString(keyPassword, Password);
                PlayerPrefs.Save();
            }

            //Save userid/playerid, usefull for things like steamid
            PlayerPrefs.SetString("_PLAYERID_", response.userId);
            //APIManager.instance.updatePlayerId(response.userId);
            
            if (onLoginSuccess != null)
            {
                onLoginSuccess.Invoke();
            }
        }

        //Failsafe login callback method to show generic errors if the firebase API fails
        public void OnLogin(bool success)
        {
            Debug.Log("OnLoginAPI False");
            LoggingIn = false;
            if (!success)
            {
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            if (toggleAutoLogin != null && toggleAutoLogin.isOn)
            {
                // Store password
                PlayerPrefs.SetString(keyUsername, Username);
                PlayerPrefs.SetString(keyPassword, Password);
                PlayerPrefs.Save();
            }
            if (onLoginSuccess != null)
            {
                onLoginSuccess.Invoke();

            }
        }

        /// <summary>
        /// Callback for the RequestUserRegister method call
        /// We can check here if user did not exist and was registered, or if he exists that means password mismatch between Firebase and MMO DB
        /// In thatcase, if firebase API returned login success, we use that password and update it in MMO DB
        /// </summary>
        /// <param name="responseHandler"></param>
        /// <param name="responseCode"></param>
        /// <param name="response"></param>
        public void OnLoginRegister(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserRegisterMessage response)
        {
            Debug.Log("OnLoginRegisterRespCode: " + responseCode);
            Debug.Log("OnLoginRegisterRespMessage: " + response.message);
            //If not success, means user exists but Firebase and MMO password donot match, so update password
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Timeout Error", "MMO Server did not respond in time");
                return;
            }
            if (responseCode != AckResponseCode.Success)
            {
                Debug.Log("OnLoginRegister: " + "Game login failed, update password from firebase for " + Username);
                MMOClientInstance.Singleton.RequestPasswordReset(Username, Password, OnResetCustom);
            }
            else
            {
                //User did not exist earlier but is now registered, lets log him in
                MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLoginCustom);
            }
        }

        //Password reset/update callback
        private void OnResetCustom(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseUserRegisterMessage response)
        {
            Debug.Log("OnResetCustomCode: " + responseCode);
            Debug.Log("OnResetCustomMsg: " + response.message);
            if (responseCode != AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Timeout Error", "MMO Server did not respond in time");
                return;
            }
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                Debug.Log("OnResetCustom: " + response.message);
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            //Request Login Again
            Debug.Log("Password updated in game backend too");
            PlayerPrefs.SetString(keyUsername, Username);
            MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLoginCustom);
        }
    }
}