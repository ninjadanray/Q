using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.Networking;

namespace ProjectQ.PlatformServices.Integration
{
    public class FederatedAuthentication : MonoBehaviour
    {
        private WWWForm form;
        private UserVerification userVerification;

        private bool hasCreateUser = false;

        // Credentials
        public string app_id = "7452013554821202";

        public string federated_app_id = "1556425345091022";
        public string federated_access_token = "OC|1556425345091022|a084792ecb888ffa2dae3b441a661643";

        private string access_token;

        void Start()
        {
            form = new WWWForm();
            userVerification = GetComponent<UserVerification>();
        }

        void FixedUpdate() {
            if (!userVerification.HasVerificationSucceeded() || hasCreateUser) return;

            StartCoroutine(CreateFederatedUser());
            StartCoroutine(ReadFederatedUsers());

            hasCreateUser = true;
        }


        IEnumerator CreateFederatedUser()
        {
            yield return new WaitForSeconds(2f);

            form.AddField("access_token", federated_access_token);
            form.AddField("persistent_id", "0000119");
            form.AddField("display_name", "fedUser#0119");
            
            // using (UnityWebRequest www = UnityWebRequest.Post("https://graph.oculus.com/1556425345091022/federated_user_create", form))
            string url = "https://graph.oculus.com/" + federated_app_id + "/federated_user_create";

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    userVerification.log.text += $"Error: {www.error} \n" ;
                }
                else
                {
                    userVerification.log.text += "Create Fed User Form Upload Complete \n";

                    FederatedUser response = JsonUtility.FromJson<FederatedUser>(www.downloadHandler.text);

                    // Extract value of 'response'
                    string federated_id = response.id;
                    string persistent_id = response.persistent_id;
                    string display_name = response.display_name;
                    string unique_name = response.unique_name;

                    // Display the response of Create Fed User API
                    userVerification.log.text += $"Create Federated User Response: \n Federated ID: {federated_id} \n Persistent ID: {persistent_id} \n Display Name: {display_name} \n Unique Name: {unique_name} \n\n";

                    StartCoroutine(GenerateUserAccessToken());
                }
            }
        }

        IEnumerator GenerateUserAccessToken()
        {
            yield return new WaitForSeconds(2f);

            form.AddField("access_token", federated_access_token);
            form.AddField("persistent_id", "0000119");

            // "https://graph.oculus.com/1556425345091022/federated_user_gen_access_token/"

            string url = "https://graph.oculus.com/" + federated_app_id + "/federated_user_gen_access_token/";

            using(UnityWebRequest www = UnityWebRequest.Post(url, form))

            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    userVerification.log.text += $"Error: {www.error} \n" ;
                }
                else
                {
                    AccessToken response = JsonUtility.FromJson<AccessToken>(www.downloadHandler.text);

                    access_token = response.access_token;

                    userVerification.log.text += $"Generated User Token: {access_token} \n";

                    StartCoroutine(ReadFederatedUser());
                }
            }
        }

        IEnumerator ReadFederatedUser()
        {
            yield return new WaitForSeconds(2f);
            
            // using (UnityWebRequest www = UnityWebRequest.Get("https://graph.oculus.com/1556425345091022?fields=id,persistent_id,unique_name,display_name"))

            string baseUrl = "https://graph.oculus.com/" + app_id;
            string parameters = "?fields=id,persistent_id,unique_name,display_name";
            string url = baseUrl + parameters;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                // Set the authorization header
                 www.SetRequestHeader("Authorization", "Bearer " + access_token);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    userVerification.log.text += $"Error: {www.error} \n" ;
                }
                else
                {
                    userVerification.log.text += "Read User Form Upload Complete \n";
                    userVerification.log.text += $"Read User Response: {www.downloadHandler.text} \n";
                }
            }
        }

        IEnumerator ReadFederatedUsers()
        {
            yield return new WaitForSeconds(2f);
            
            // using (UnityWebRequest www = UnityWebRequest.Get("https://graph.oculus.com/1556425345091022?fields=id,persistent_id,unique_name,display_name"))

            string url = "https://graph.oculus.com/" + app_id + "/federated_users/";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                // Set the authorization header
                 www.SetRequestHeader("Authorization", "Bearer " + federated_access_token);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    userVerification.log.text += $"Error: {www.error} \n";
                    userVerification.log.text += $"Read Federated Users Failed. \n";
                }
                else
                {
                    userVerification.log.text += "Read User Form Upload Complete \n";
                    userVerification.log.text += $"Federated Users: {www.downloadHandler.text} \n";
                }
            }
        }
    }

    [System.Serializable]
    public class FederatedUser
    {
        public string id;
        public string persistent_id;
        public string display_name;
        public string unique_name;
    }

    public class AccessToken
    {
        public string access_token;
    }
}
