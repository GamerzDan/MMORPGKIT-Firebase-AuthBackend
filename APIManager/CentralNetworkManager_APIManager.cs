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

        /// <summary>
        /// Login flow is, tryFirebaseLogin->checkEmailVerified||accDisabled->doKitLogin
        /// At any part if there is errorResponse, return to client
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="result"></param>
        public void callFirebaseLogin(string username, string password, RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string url = FirebaseEndpoint + ":signInWithPassword?key=" + FirebaseKey;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["email"] = username.ToLower().Trim();
            data["password"] = password.Trim();

            var currentRequest = new RequestHelper
            {
                Uri = url,
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded", // Here you can attach a UploadHandler/DownloadHandler too :)
                SimpleForm = data,
                Params = new Dictionary<string, string> {{ "key", FirebaseKey}}
            };

            RestClient.Request(currentRequest).Then(res =>
            {
                Debug.Log("callFirebaseLoginResponse: " + res.Text);
                FirebaseRes dat = JsonUtility.FromJson<FirebaseRes>(res.Text);
                //Get User details to check if emailVerified and accDisabled
                callFirebaseUserData(dat.idToken, result);
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.Log("callFirebaseLoginError: " + err.Message);
                Debug.Log("callFirebaseLoginErrorResponse: " + error.Response);
                Debug.Log("callFirebaseLoginErrorRequest: " + error.Request.BodyString);
                result.Invoke(AckResponseCode.Error,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = error.Response,
                    });
            });
        }

        /// <summary>
        /// If emailVerified and accNotDisabled, try kitLogin
        /// If false, return to client. If email verification needed, send email
        /// </summary>
        /// <param name="idToken"></param>
        /// <param name="result"></param>
        public void callFirebaseUserData(string idToken, RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string url = FirebaseEndpoint + ":lookup?key=" + FirebaseKey;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["idToken"] = idToken.Trim();

            var currentRequest = new RequestHelper
            {
                Uri = url,
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded", // Here you can attach a UploadHandler/DownloadHandler too :)
                SimpleForm = data,
                Params = new Dictionary<string, string> {{ "key", FirebaseKey}}
            };

            RestClient.Request(currentRequest).Then(res =>
            {
                Debug.Log("callFirebaseUserDataResponse: " + res.Text);
                FirebaseRes dat = JsonUtility.FromJson<FirebaseRes>(res.Text);
                //Get User details to check if emailVerified and accDisabled
                FirebaseUsersRes userData = dat.users[0];
                FirebaseErrorRes error = new FirebaseErrorRes();
                if (userData.disabled)
                {
                    error.error.code = 400;
                    error.error.message = "ACCOUNT_DISABLED";
                    result.Invoke(AckResponseCode.Error,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = JsonUtility.ToJson(error),
                    });
                }
                if(!userData.emailVerified)
                {
                    //Send Email Verification Link, donot send callback object
                    callFirebaseSendEmailVerification(idToken, "VERIFY_EMAIL", null);

                    error.error.code = 400;
                    error.error.message = "EMAIL_NOT_VERIFIED";
                    result.Invoke(AckResponseCode.Error,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = JsonUtility.ToJson(error),
                    });
                }
                //Return to client for kit login
                result.Invoke(AckResponseCode.Success,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = res.Text,
                    });

            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.Log("callFirebaseUserDataError: " + err.Message);
                Debug.Log("callFirebaseUserDataErrorResponse: " + error.Response);
                Debug.Log("callFirebaseUserDataErrorRequest: " + error.Request.BodyString);
                result.Invoke(AckResponseCode.Error,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = error.Response,
                    });
            });
        }

        public void callFirebaseSendEmailVerification(string idToken, string requestType, RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string url = FirebaseEndpoint + ":sendOobCode?key=" + FirebaseKey;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["idToken"] = idToken.Trim();
            data["requestType"] = requestType.Trim();

            var currentRequest = new RequestHelper
            {
                Uri = url,
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded", // Here you can attach a UploadHandler/DownloadHandler too :)
                SimpleForm = data,
                Params = new Dictionary<string, string> { { "key", FirebaseKey } }
            };

            RestClient.Request(currentRequest).Then(res =>
            {
                Debug.Log("callFirebaseSendEmailVerification Response: " + res.Text);
                FirebaseRes dat = JsonUtility.FromJson<FirebaseRes>(res.Text);
                if(result != null)
                {
                    result.Invoke(AckResponseCode.Success,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = "Sent verification link to email " + dat.email,
                    });
                }
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.Log("callFirebaseSendEmailVerification Error: " + err.Message);
                Debug.Log("callFirebaseSendEmailVerification ErrorResponse: " + error.Response);
                Debug.Log("callFirebaseSendEmailVerification ErrorRequest: " + error.Request.BodyString);
                result.Invoke(AckResponseCode.Error,
                    new ResponseFirebaseAuthLoginMessage()
                    {
                        response = error.Response,
                    });
            });
        }

    }
#endif
}
