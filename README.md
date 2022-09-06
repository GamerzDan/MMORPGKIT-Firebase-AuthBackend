
# MMORPGKIT-Firebase-AuthBackend
Use [FirebaseAuth](https://firebase.google.com/products/auth) as a Authentication and User Management backend for [MMORPGKIT](https://assetstore.unity.com/packages/templates/systems/mmorpg-kit-2d-3d-survival-110188) for Unity  

## Features  

 - Provides a fast and scalable full user-management and authentication backend (with a web UI) thanks to Firebase    
- Email-based registration and login 
- Email verification 
- Disable/Ban user 
- Conditional login check to disallow banned or unverified (email) accounts 
- Additional host of information like registration date, last password update time, last login time, displayName, etc. 
- Asynchronous API calls
- Uses Firebase REST APIs and REST Client for Unity, so cross-platform compatible including webgl and mobile
- Supports MySQL and SQLite database backends 
- ServerSide Logic (all API calls are done server-side) 
- Uses Partial Classes, no changes done to MMORPGKIT core files

## Workflow
The below flow(s) works linearly as long as each step reports success, if any step reports error or false, that step reverts back to client with the error message and displays it to the client.  
**During registration and login, the addon automatically sends the account verification email (through firebase) if the account is not emailVerified.**
#### Registration
Client(register)->Server(firebaseRegister)->Server(mmoRegister)->Client(success)
#### Login
Client(login)->Server(firebaseLogin)->Server(firebaseGetUserData)->Server(checkEmailVerified || checkUserDisabled)->Server(mmoLogin)->Client(success)  

**If user is registered in firebase but not mmoKit, addon automatically registers them in mmoKit upon Login.  
If firebase password changed but mmoKit hasn't, addon automatically updates the firebase password in mmoKit.
With this addon, any password change/reset needs to be done from Firebase.**

## Required (install them before installing this addon)
#### MMORPGKIT   (tested on v1.76)
https://assetstore.unity.com/packages/templates/systems/mmorpg-kit-2d-3d-survival-110188
#### Rest Client for Unity (tested on 2.62)  (INCLUDED IN THE PROJECT, DELETE IF INSTALLING FROM ASSETSTORE)  
https://assetstore.unity.com/packages/tools/network/rest-client-for-unity-102501        
#### MMORPGKIT-RESET-PASSWORD-ADDON  
https://github.com/GamerzDan/MMORPGKIT-Reset-Password-Addon    
#### Firebase Account
https://firebase.google.com/

---

## Setup
0. Create your Free Firebase account by signing in with a google account (https://console.firebase.google.com/) and setup your project.  
On the left hand menu, Click Authentication (under Build menu) and enable Firebase Authentication for your project and also enable email (password signin) provider for it.    
Open your project's settings and copy the **WEB API KEY**. You need to save/replace this key in **CentralNetworkManager_APIManager**.cs (under APIManager folder).    
![enter image description here](https://i.imgur.com/kOTz0Fw.jpeg)
![enter image description here](https://i.imgur.com/57cpME3.jpeg)
1. Increase username character length limit to **255** in **CentralNetworkManager** (via Unity Inspector)
2. Install other Required Dependencies
3. Drag and Drop this addon to your project (under Assets folder or any sub-folder within it)
4. Edit the UIMmoLogin.cs and UIMmoRegister.cs `(UnityMultiplayerARPG/MMO/Scripts/MMOGame/UI/)` classes of the MMORPGKIT to partial classes  
 ```
 Change
 public class UIMmoLogin : UIBase
 public class UIMmoRegister : UIBase
 to
 public partial class UIMmoLogin : UIBase
 public partial class UIMmoRegister : UIBase
 ```
 5. To use the Firebase Addon's login or registration system, you need to call `tryMMOLogin();` or `tryMMORegister();` 

**(Method 1)**
 Since this Addon uses Partial Classes, this is as easy as finding the Login/Registration UI Button object in your project and change it's **OnClick()** method. 
![Method 1 - Using UI Buttons](https://i.imgur.com/IR2bQ0V.jpeg)

**(Method 2 - EASY)**
Easiest way is to just edit the kit's UIMmoLogin or UIMmoRegister class's `OnClickLogin` or `OnClickRegister()` methods and call the `tryMMOLogin();` or `tryMMORegister();`  at start itself and return or comment the rest of the code.  
![enter image description here](https://i.imgur.com/YiT0mfj.jpeg) 

![enter image description here](https://i.imgur.com/lYhMjE2.jpeg)
