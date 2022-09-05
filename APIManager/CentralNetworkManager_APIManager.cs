using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System.Linq;
using System;
using MultiplayerARPG.MMO;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
#if UNITY_STANDALONE && !CLIENT_BUILD
    public partial class CentralNetworkManager
    {
        public string FirebaseEndpoint = @"https://identitytoolkit.googleapis.com/v1/accounts";
        public string FirebaseKey = @"AIzaSyA4sj5mUuvJIQWp1mdxm5Xbf_ffQLLPqIM";

        public void callFirebaseLogin(string username, string password, RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string url = FirebaseEndpoint + ":signInWithPassword?key=" + FirebaseKey;
            Debug.Log(url);
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["email"] = username.ToLower().Trim();
            data["password"] = password.Trim();

            var currentRequest = new RequestHelper
            {
                Uri = url,
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded", // Here you can attach a UploadHandler/DownloadHandler too :)
                SimpleForm = data,
                Params = new Dictionary<string, string> {
                { "key", FirebaseKey}
                }
            };

            RestClient.Request(currentRequest).Then(res =>
            {
                Debug.Log("APIResponse: " + res.Text);
                result.Invoke(AckResponseCode.Success,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = res.Text,
                    });
            /*
            ApiResponse dat = JsonUtility.FromJson<ApiResponse>(res.Text);

            if (!dat.error)
            {
                //Debug.Log(dat.message);
                //Debug.Log(dat.response);
                PlayerPrefs.SetString("keyUsername", username);
                PlayerPrefs.SetString("accpkey", dat.privkey);
                PlayerPrefs.SetString("accskey", dat.mnemonic);
                _login.proceedMMOLogin();
            }
            else
            {
                LoadingScreen.instance.showError(true, "ErrorCode: ", dat.message);
                _login.OnLogin(false);
            }
            */
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.Log("API Call Error: " + err.Message);
                Debug.Log("API Call ErrorResponse: " + error.Response);
                Debug.Log("API Call ErrorRequest: " + error.Request.BodyString);
                //LoadingScreen.instance.showError(true, "API Call Error: ", err.Message);
                //LoadingScreen.instance.enableDisableLoadingScreen(false, "Logging In");
                //_login.OnLogin(false);
                result.Invoke(AckResponseCode.Success,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = error.Response,
                    });
            });
        }
    }
#endif
}
