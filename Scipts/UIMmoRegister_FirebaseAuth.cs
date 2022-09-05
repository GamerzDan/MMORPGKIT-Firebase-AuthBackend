using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class UIMmoRegister : UIBase
    {
        /// <summary>
        /// Use this method to start new-user registration flow for firebase->mmorpgkit
        /// For Firebase, username needs to be a email address
        /// First register user in firebase, once success, register in the kit
        /// </summary>
        public void tryMMORegister()
        {
            Debug.Log("tryMMORegister");
            // Don't allow to spam register button
            if (Registering)
                return;

            UISceneGlobal uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(Username))
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_IS_EMPTY.ToString()));
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_PASSWORD_IS_EMPTY.ToString()));
                return;
            }

            if (!ValidatePassword())
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_CONFIRM_PASSWORD.ToString()));
                return;
            }

            Registering = true;

            //MMOClientInstance.Singleton.RequestUserRegister(Username, Password, Email, OnRegister);
            MMOClientInstance.Singleton.RequestFirebaseRegister(Username, Password, OnFirebaseRegister);
        }

        public void OnFirebaseRegister(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseFirebaseAuthLoginMessage response)
        {
            Registering = false;
            Debug.Log(responseCode);
            Debug.Log(response.response);
            //If firebaseRegister was not success
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
            //Try MMORPGKit's Registration now
            if (string.IsNullOrEmpty(Email))
            {
                Email = Username;
            }
            MMOClientInstance.Singleton.RequestUserRegister(Username, Password, Email, OnRegisterCustom);
            //Maybe we should show a message after Registration to verify the email
        }

        public void OnRegisterCustom(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserRegisterMessage response)
        {
            Registering = false;
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                if (onRegisterFail != null)
                    onRegisterFail.Invoke();
                return;
            }
            if (onRegisterSuccess != null)
                onRegisterSuccess.Invoke();
            //Success, can send email verification mail or show message about it here
            Debug.Log(Username + " registration success in kit too");
        }

        /// <summary>
        /// Fail-safe callback method, currently not in use
        /// </summary>
        /// <param name="success"></param>
        public void OnRegister(bool success)
        {
            Registering = false;
            if (!success)
            {
                if (onRegisterFail != null)
                    onRegisterFail.Invoke();
                return;
            }
            if (onRegisterSuccess != null)
                onRegisterSuccess.Invoke();
        }

    }

}