using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.UI;
using TMPro;

namespace ProjectQ.PlatformServices.Integration
{
    public class IAPManager : MonoBehaviour
    {
        // <Summary>
        // This script is responsible for managing the IAP Integration from
        // making a purchase, getting all available and purchased SKUs.
        // </Summary>

        // Credentials
        public string access_token = "OC|7452013554821202|52f7e02d33a151683fef33a0c221eb7e";
        public string verify_entitlement_api = "https://graph.oculus.com/7452013554821202/verify_entitlement";

        public UserVerification userVerification;

        [SerializeField] private TMP_Text availableItems, purchasedItems, assetLists, notice;

        private WWWForm form;

        // Create new skus in the dashboard.oculus.com with the name following.
        private string[] skus = { "physics-grabbable-cube", "kinematic-grabbable-cube", "poke-interactable" };

        private bool isIAPManagerCalled =  false;

        void Awake()
        {
            form = new WWWForm();
            userVerification = GetComponent<UserVerification>();
        }

        void FixedUpdate()
        {
            // UserVerification userVerification = GetComponent<UserVerification>();

            if (!userVerification.HasVerificationSucceeded() || isIAPManagerCalled) return;

            // Call to get the available and purchased products
            GetProductPrices();
            GetPurchasedProducts();
            GetLitOfAssets();

            isIAPManagerCalled = true;
        }

        public void Buy()
        {   
            notice.text += $" User ID: {userVerification.user_id}\n";
            // IAP.LaunchCheckoutFlow(skus[2]).OnComplete(BuyCallback);
            StartCoroutine(VerifyEntitlementOnProduct());
        }
        // private

        public void GetLitOfAssets()
        {
            AssetFile.GetList().OnComplete(GetListOfAssetsCallback);
        }

        private void GetListOfAssetsCallback(Message<AssetDetailsList> msg)
        {
            if (msg.IsError) return;

            foreach (var asset in msg.GetAssetDetailsList())
            {
                assetLists.text += $"{asset.AssetId} - {asset.Metadata} - {asset.AssetType} - {asset.IapStatus} - {asset.DownloadStatus} - {asset.Filepath} \n";

                HandleAssets(asset);
            }
        }

        private void HandleAssets(AssetDetails asset)
        {
            if (asset.DownloadStatus == "installed")
            {
                StartCoroutine(ReadAndInstantiateInstalledDLC(asset.Filepath));
            }
            else if (asset.DownloadStatus == "available")
            {
                DownloadAndReadDLC(asset);
            }
            else
            {
                // Handle Error
            }
        }

        private IEnumerator ReadAndInstantiateInstalledDLC(string filepath)
        {
            var bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
            yield return bundleRequest;

            // string[] parts = filepath.Split('/');
            // string gameObjectName = parts[parts.Length - 1];

            AssetBundle bundle = bundleRequest.assetBundle;

            if (bundle == null)
            {
                notice.text += "Failed to load AssetBundle! \n";
                yield break;
            }
            
            string[] assetNames = bundle.GetAllAssetNames();
            foreach (string assetName in assetNames)
            {
                // Load the GameObject directly from the AssetBundle using its name
                GameObject loadedObject = bundle.LoadAsset<GameObject>(assetName);

                if (loadedObject == null)
                {
                    notice.text += $"Failed to load object: {assetName} from AssetBundle\n";
                    continue;
                }

                Instantiate(loadedObject, Vector3.zero, Quaternion.identity);
            }
        }

        private void DownloadAndReadDLC(AssetDetails asset)
        {
            AssetFile.DownloadById(asset.AssetId).OnComplete((msg) => {
                if (msg.IsError)
                {
                    Debug.Log($"Error downloading assetID: {asset.AssetId} - {msg.GetError().Message}");
                }
                else
                {
                    var downloadResult = msg.GetAssetFileDownloadResult();
                    notice.text += $"Download initiated at: {downloadResult.Filepath} \n";
                    // ReadDLC(downloadResult.Filepath);
                    var downloadUpdate = msg.GetAssetFileDownloadUpdate();

                    notice.text += downloadUpdate;
                }
            });
        }
        

        private void GetProductPrices()
        {
            IAP.GetProductsBySKU(skus).OnComplete(GetProductsBySKUCallback);
        }

        private void GetPurchasedProducts()
        {
            IAP.GetViewerPurchases().OnComplete(GetPurchasedProductsCallback);
        }

        void GetProductsBySKUCallback(Message<ProductList> msg)
        {
            if (msg.IsError) return;

            foreach (var p in msg.GetProductList())
            {
                availableItems.text += $"{p.Name} - {p.FormattedPrice}\n";
            }
        }

        void GetPurchasedProductsCallback(Message<PurchaseList> msg)
        {
            if (msg.IsError) return;

            foreach (Purchase p in msg.GetPurchaseList())
            {
                purchasedItems.text += $"Purchased Item: {p.Sku} - {p.GrantTime}\n";
            }

            GetLitOfAssets();
        }
        
        void BuyCallback(Message<Purchase> msg)
        {
            if (msg.IsError) return;

            // purchasedItems.text = string.Empty;
            GetPurchasedProducts();
        }

        IEnumerator VerifyEntitlementOnProduct()
        {
            yield return new WaitForSeconds(2);
            
            form.AddField("access_token", access_token);
            form.AddField("user_id", userVerification.user_id);
            form.AddField("sku", "physics-grabbable-cube");

            using(UnityWebRequest www = UnityWebRequest.Post(verify_entitlement_api, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    notice.text = www.error;
                }
                else
                {
                    string apiResponse = www.downloadHandler.text;
                    ApiResponseData responseData = JsonUtility.FromJson<ApiResponseData>(apiResponse);

                    if (!responseData.success)
                    {
                        notice.text += "Proceeding to checkout \n";
                        IAP.LaunchCheckoutFlow(skus[0]).OnComplete(BuyCallback);
                    }
                    else
                    {
                        notice.text += "Already have this item \n";
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class ApiResponseData
    {
        public bool success;
    }
}
