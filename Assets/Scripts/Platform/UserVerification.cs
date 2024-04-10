using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.Networking;
using TMPro;

namespace ProjectQ.PlatformServices
{
    public class UserVerification : MonoBehaviour
    {
        [SerializeField]
        public string userID, userName, userPresence, userNonce, userVerified;
        private string verificationAPI = "https://graph.oculus.com/user_nonce_validate";
        
        [SerializeField]
        private string accessToken = "OC|6856195184390701|d67fc81d71aefbd96dd8ce1c369ef11a";

        private WWWForm form;
        void Awake()
        {
            form = new WWWForm();
        }

        public void GetLoggedInUser()
        {
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        }

        void GetLoggedInUserCallback(Message<User> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting logged in user. Error Message: {0}", msg.GetError().Message);
            }
            else
            {
                userID = msg.Data.ID.ToString();
                // id.text = msg.Data.ID.ToString();
                userName = msg.Data.DisplayName;
                userPresence = msg.Data.PresenceStatus.ToString();

                //After successfully retreive user Data, GetUserProof will call to get the nonce from the API;
                // GetUserProof();
            } 
        }
    }
}
