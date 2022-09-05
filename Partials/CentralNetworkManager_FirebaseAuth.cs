using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;

namespace MultiplayerARPG.MMO
{
#if UNITY_STANDALONE && !CLIENT_BUILD
    public static class FirebaseConfig
    {
        public const string FirebaseEndpoint = @"https://identitytoolkit.googleapis.com/v1/accounts";
        public const string FirebaseKey = @"AIzaSyA4sj5mUuvJIQWp1mdxm5Xbf_ffQLLPqIM";
    }
#endif

    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        [DevExtMethods("RegisterMessages")]
        protected void DevExtRegisterFirebaseAuthMessages()
        {
            RegisterRequestToServer<RequestUserLoginMessage, ResponseFirebaseAuthLoginMessage>(MMORequestTypes.RequestFirebaseLogin, HandleRequestFirebaseLogin);
        }
#endif
    }

    public static partial class MMORequestTypes
    {
        public const ushort RequestFirebaseLogin = 5010;
    }

    public struct ResponseFirebaseAuthLoginMessage : INetSerializable
    {
        public string response;
        public void Deserialize(NetDataReader reader)
        {
            response = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(response);
        }
    }

    public partial class CentralNetworkManager
    {
        /// <summary>
        /// Custom Name validation to be used in delegate of NameValidating class
        /// Currently using it to disable name validation as we will use email for username
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool customNameValidation(string name)
        {
            Debug.Log("Using customNameValidation");
            return true;
        }
#if UNITY_STANDALONE && !CLIENT_BUILD
        public bool RequestFirebaseLogin(string username, string password, ResponseDelegate<ResponseFirebaseAuthLoginMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestFirebaseLogin, new RequestUserLoginMessage()
            {
                username = username,
                password = password,
            }, responseDelegate: callback);
        }

        protected async UniTaskVoid HandleRequestFirebaseLogin(
            RequestHandlerData requestHandler,
            RequestUserLoginMessage request,
            RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string message = "";
            string username = request.username;
            string password = request.password;
            NameValidating.overrideUsernameValidating = customNameValidation;
            //string email = request.email;
            Debug.Log("Pre API call");
            callFirebaseLogin(username, password, result);
            Debug.Log("Post API call");
            /*
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("email", username.Trim()));
            formData.Add(new MultipartFormDataSection("password", password.Trim()));
            Debug.Log("email=" + username.Trim() + "&password=" + password.Trim());
            Debug.Log(formData.ToArray());
            UnityWebRequest www = UnityWebRequest.Post(FirebaseConfig.FirebaseEndpoint + ":signInWithPassword?key=" + FirebaseConfig.FirebaseKey, formData);
            try { 
                var asyncOp = await www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("FirebaseApiCallFailed- " + www.error);
                    message = "FirebaseApiCallFailed- " + www.error;
                }
                else
                {
                    Debug.Log("FirebaseApiCallFailed- " + www.downloadHandler.text);
                    message = www.downloadHandler.text;
                }
            } catch(Exception e)
            {
                Debug.Log(e.Message);
                message = e.Message;
            }

            /*
            AsyncResponseData<FindUsernameResp> findUsernameResp = await DbServiceClient.FindUsernameAsync(new FindUsernameReq()
            {
                Username = username
            });
            if (findUsernameResp.Response.FoundAmount < 1)
                message = UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD;
            else if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                message = UITextKeys.UI_ERROR_USERNAME_TOO_SHORT;
            else if (username.Length > maxUsernameLength)
                message = UITextKeys.UI_ERROR_USERNAME_TOO_LONG;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
            {
                message = UITextKeys.UI_ERROR_PASSWORD_TOO_SHORT;
            }
            else
            {
                await DbServiceClient.UpdateUserLoginAsync(new CreateUserLoginReq()
                {
                    Username = username,
                    Password = password,
                    Email = email,
                });
            }
            
            // Response
            result.Invoke(AckResponseCode.Success,
                new ResponseFirebaseAuthLoginMessage()
                {
                    response = message,
                });
            */
        }
#endif
    }


    public partial class MMOClientInstance : MonoBehaviour
    {
        public void RequestFirebaseLogin(string username, string password, ResponseDelegate<ResponseFirebaseAuthLoginMessage> callback)
        {
            centralNetworkManager.RequestFirebaseLogin(username, password, callback);
        }
    }
}