using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class UIMmoRegister : UIBase
    {
        //We don't need to use register anymore actually, instead we can use a single workflow by hooking this into our Login script's
        // OnLoginCustom() method
        public void proceedMMORegister()
        {
            MMOClientInstance.Singleton.RequestUserRegister(Username, Password, Email, OnRegister);
        }

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