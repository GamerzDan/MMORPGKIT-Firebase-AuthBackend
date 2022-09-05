using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FirebaseRes
{
    public bool registered;
    public string email;
    //public CustomResponse response;
    public string localId;
    public string displayName;
    public string idToken;
    /// <summary>
    /// Array of Users fetched from Firebase, use index 0
    /// </summary>
    public FirebaseUsersRes[] users;
}
[System.Serializable]
public class FirebaseUsersRes
{
    public bool emailVerified;
    public string email;
    public string displayName;
    public bool disabled;
    public string lastLogin;                     //timestamp, in milliseconds
    public string createdAt;                     //timestamp, in milliseconds
    public string passwordUpdatedAt;             //timestamp, in milliseconds
    public string localId;
}

[System.Serializable]
public class FirebaseErrorRes
{
    public FirebaseErrorDetailsRes error;
}
[System.Serializable]
public class FirebaseErrorDetailsRes
{
    public int code;
    public string message;
}