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
            RegisterRequestToServer<RequestUserRegisterMessage, ResponseFirebaseAuthLoginMessage>(MMORequestTypes.RequestFirebaseRegister, HandleRequestFirebaseRegister);
        }
#endif
    }

    public static partial class MMORequestTypes
    {
        public const ushort RequestFirebaseLogin = 5010;
        public const ushort RequestFirebaseRegister = 5011;
    }

    /// <summary>
    /// General Response handler for firebase, we pass string or jsonText as response in it
    /// </summary>
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

        public bool RequestFirebaseRegister(string email, string password, ResponseDelegate<ResponseFirebaseAuthLoginMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestFirebaseRegister, new RequestUserRegisterMessage()
            {
                username = email,
                password = password,
                email = email
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
        }

        protected async UniTaskVoid HandleRequestFirebaseRegister(
            RequestHandlerData requestHandler,
            RequestUserRegisterMessage request,
            RequestProceedResultDelegate<ResponseFirebaseAuthLoginMessage> result)
        {
            string message = "";
            string email = request.username;
            string password = request.password;
            NameValidating.overrideUsernameValidating = customNameValidation;
            //string email = request.email;
            Debug.Log("Pre API call");
            callFirebaseRegister(email, password, result);
            Debug.Log("Post API call");
        }
#endif
    }


    public partial class MMOClientInstance : MonoBehaviour
    {
        public void RequestFirebaseLogin(string username, string password, ResponseDelegate<ResponseFirebaseAuthLoginMessage> callback)
        {
            centralNetworkManager.RequestFirebaseLogin(username, password, callback);
        }
        public void RequestFirebaseRegister(string email, string password, ResponseDelegate<ResponseFirebaseAuthLoginMessage> callback)
        {
            centralNetworkManager.RequestFirebaseRegister(email, password, callback);
        }
    }
}