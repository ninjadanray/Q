using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.Networking;

namespace ProjectQ.PlatformServices.Integration
{
    public class AccountLinking : MonoBehaviour
    {
        private string base64Code = "7PytWdRNF4zrTq65ASNobNSCcrGRa7eH5SKWPZDfy5VjsZFe5qnJerzhMShHp0dN";
        private string org_scoped_id = "5978973908802860";
        private string access_token = "OC|5422694004486430|c6d7704c7f0aab250d661270c6b64639";

        private string refresh_code = "IPacCqq8F2WKpDO7phr2reANZs7vdEUVSn9LhSZeQa3A9HCj3FEvZe6nCqAewv88";
        private string authToken = "OCAeOxwiiI0ufm9feHo9jqZAvmeBe1QTxMKj84J7s4lJb2pDdUMrfXS5oZBTicq2pPy0E0fDZAyHyZAl0d1ZAQiTA8dcml8ZCqfr7Oh2LpsgRgZDZD";

        [Space(20)] // 20 pixels of spacing here.
        [SerializeField] private string authorizeAPI = "https://graph.oculus.com/sso_authorize_code";
        [SerializeField] private string refreshCodeAPI = "https://graph.oculus.com/sso_authorize_refresh_code";

        // private WWWForm form;

        float s = 3.0f;
        // Awake is called before the first frame update
        void Awake()
        {
            // form = new WWWForm();
        }

        void Start()
        {
            StartCoroutine(RefreshCode(s));
        }

        // Update is called once per frame
        void Update()
        {

        }

        /* 
            * API Post Request
            * https://graph.oculus.com/sso_authorize_code?code=somecode&access_token=OC|client-id|client-secret&org_scoped_id=user-id-for-your-org
            * https://graph.oculus.com/sso_authorize_code?code=mFJBMzwHFxMl5yYafllfqpRUDg2OJIPsVf3oopnTU4IQZCM5maeKLhJp6UmARSjL&access_token=OC|5422694004486430|c6d7704c7f0aab250d661270c6b64639&org_scoped_id=5978973908802860
            * 
            * {"oauth_token":"OCAeP5c4HFxMm6OMYrBQuT75L9JQjO6J5fTuf7opl8Tk2X1O78RxAkn10C4GZC9sxwRieN4SCcaycZBiq2RWXn0yAX4mK9gZD","refresh_code":"Fso66ZFhjmCSyLSBsDHabdJlmK56VcJQmpFTiW97ATB2eWAtREjq4h6K4Bz5AnRT"}
            * {"oauth_token":"OCAeOfovbFTeCbyg8xmPvD4lmhC9t9AJ7lzj6tX11qBA3Iq6WI3zIsJ3mUv2Imi6k1UZCNmCPnFoXZBnIaCQGEslYZBRN5hEZD","refresh_code":"NzidKkVNyUJq22HsJblWIqujKb7XQCfICKfj1EXdel4qEd4K0M5dadQFXeq7iyKp"}
        */

        private IEnumerator AuthorizeCode(float deltaTime)
        {
            yield return new WaitForSeconds(deltaTime); // Wait before Initalization of method

            WWWForm form = new WWWForm();

            form.AddField("code", base64Code);
            form.AddField("access_token", access_token);
            form.AddField("org_scoped_id", org_scoped_id);

            using (UnityWebRequest www = UnityWebRequest.Post(authorizeAPI, form))
            {

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                    Debug.Log(UnityWebRequest.Result.Success);
                    Debug.Log(www.downloadHandler.text);
                }
            }

        }

        /*
            * Output when POST Request to https://graph.oculus.com/sso_authorize_code is successful.
            * {
            *  "oauth_token":"OCAeOLitq99G8mnqxmUJOxTaCn6EChttpWuclz1mGtaZB9fZAOfVxtuvLPbhuw3i9I4gBDDdeVfNBjMKjSEZCvCLYM6paUBMZD",
            *  "refresh_code":"wyoTlR5Nrexl2dLv3F982cKq6nqVqMPDvEWvgwyNLJZtnnN3QmoWCx9MXMq7A9Jl"
            *  }
        */

        // Get UserID and Alias using the access_token && refresh_code
        private IEnumerator GetUserData(float deltaTime)
        {
            yield return new WaitForSeconds(deltaTime);

            string URI = "https://graph.oculus.com/me?" + "access_token=" + authToken + "&fields=id,alias";

            using (UnityWebRequest www = UnityWebRequest.Get(URI))
            {
                yield return www.SendWebRequest();

                string[] pages = URI.Split('/');
                int page = pages.Length - 1;

                switch (www.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + www.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + www.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + www.downloadHandler.text);
                        break;
                }
            }
        }

        /*
            * Output after the GET Request to https://graph.oculus.com/me is successful.
            * Received: {"id":"5978973908802860","alias":"DanRaySN"}
            * Received: {"id":"5537122409743332","alias":"ninja01_6s2c3s"}
        */

        private IEnumerator RefreshCode(float deltaTime)
        {
            yield return new WaitForSeconds(deltaTime);

            WWWForm form = new WWWForm();

            form.AddField("access_token", access_token);
            form.AddField("org_scoped_id", org_scoped_id);
            form.AddField("refresh_code", refresh_code);

            using (UnityWebRequest www = UnityWebRequest.Post(refreshCodeAPI, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                    Debug.Log(UnityWebRequest.Result.Success);
                    Debug.Log(www.downloadHandler.text);
                }
            }
        }
    }
}
