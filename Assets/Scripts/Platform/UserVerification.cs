using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.Networking;
using TMPro;

namespace ProjectQ.PlatformServices.Integration
{
    public class UserVerification : MonoBehaviour
    {
        // public AppCredentials appCredentials;
        public TMP_Text log;
        private WWWForm form;
        private bool verificationSucceeded = false; // Flag to track verification status

        public string user_id, display_name, presence, nonce, verified, age;

        // Credentials
        public string access_token = "OC|7452013554821202|52f7e02d33a151683fef33a0c221eb7e";
        public string verification_api = "https://graph.oculus.com/user_nonce_validate";
        void Awake()
        {
            form = new WWWForm();
        }

        public void GetLoggedInUser()
        {
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);

             
        }

        public void GetUserAgeCategory()
        {
            UserAgeCategory.Get().OnComplete((msg) => {
                age = msg.Data.AgeCategory.ToString();
                log.text += $"User Age Group : {age}\n";
            });

            // Report the currently logged-in user's age category to Meta with value child. A callback function can be added after the method call.
            if (age == "Ch") {
                UserAgeCategory.Report(AppAgeCategory.Ch).OnComplete((msg) => {
                    if (msg.IsError)
                    {
                        Debug.LogErrorFormat("Oculus: Error getting user proof. Error Message: {0}", msg.GetError().Message);
                        return;
                    }
                    else
                    {
                        log.text += "User Reported\n";
                    }
                });
            }
        }

        void GetLoggedInUserCallback(Message<User> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting logged in user. Error Message: {0}", msg.GetError().Message);

                log.text += $"'Oculus: Error getting logged in user. Error Message: {0}, {msg.GetError().Message} \n";
            }
            else
            {
                user_id = msg.Data.ID.ToString();
                display_name = msg.Data.DisplayName;
                presence = msg.Data.PresenceStatus.ToString();

                log.text += $"User ID: {user_id} \nUser Name: {display_name} \nUser Presence: {presence} \n";

                //After successfully retreive user Data, GetUserProof will call to get the nonce from the API;
                GetUserProof();
            } 
        }

        private void GetUserProof() 
        {
            Users.GetUserProof().OnComplete(GetUserProofCallback);
        }

        void GetUserProofCallback(Message<UserProof> msg) 
        {
            if(msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting user proof. Error Message: {0}", msg.GetError().Message);
                return;
            }
            else 
            {
                nonce = msg.Data.Value;
            }

            log.text += $"USER ID: {user_id} \n USER NONCE: {nonce} \n";

            if (age == "Ch")
            {
                log.text += $"Child age group is {age}, can't be validated.\n";
            }
            else
            {
                // StartCoroutine(ValidateNonce(userID, userNonce));
                log.text += $"Child age group is {age}, proceeding on validation.\n";
                StartCoroutine(ValidateNonce(user_id, nonce));
            }
        }

        IEnumerator ValidateNonce(string id, string nonce)
        {
            yield return new WaitForSeconds(4f);

            form.AddField("access_token", access_token);
            form.AddField("nonce", nonce);
            form.AddField("user_id", id);

            using (UnityWebRequest www = UnityWebRequest.Post(verification_api, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    log.text += $"Error: {www.responseCode} - {www.error}\n";
                }
                else
                {
                    verified = www.downloadHandler.text;
                    log.text += $"Success response: {verified} \n";

                    // Deserialize JSON response
                    VerificationResponse response = JsonUtility.FromJson<VerificationResponse>(www.downloadHandler.text);

                    // Extract value of 'is_valid'
                    bool isValid = response.is_valid;

                    // Assign the value to userVerified
                    log.text += $"Is Valid: {isValid.ToString()} \n";

                    if (isValid)
                    {
                        verificationSucceeded = true; // Set the flag to true

                        log.text += $"Has Verification Succeeded: {HasVerificationSucceeded()} \n";
                    }
                }
            }
        }

        public bool HasVerificationSucceeded() {
            return verificationSucceeded;
        }

        [System.Serializable]
        public class VerificationResponse
        {
            public bool is_valid;
        }
    }
}
