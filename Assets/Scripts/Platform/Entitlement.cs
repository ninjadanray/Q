using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

namespace ProjectQ.PlatformServices.Integration
{
    public class Entitlement : MonoBehaviour
    {
        private UserVerification userVerification;
        public bool exitAppOnFailure = true;

        void Awake()
        {
            try
            {
                if (!Oculus.Platform.Core.IsInitialized())
                {
                    Oculus.Platform.Core.Initialize();
                }

                Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
            }
            catch
            {
                // Treat any potential initialization exceptions as an entitlement check failure.
                HandleEntitlementResult(false);
            }

            userVerification = GetComponent<UserVerification>();
        }

        void EntitlementCallback(Message msg)
        {
            // If the user passed the entitlement check, msg.IsError will be false.
            // If the user failed the entitlement check, msg.IsError will be true.
            HandleEntitlementResult(msg.IsError == false);
        }

        void HandleEntitlementResult(bool result)
        {
            if (result) // User passed the entitlement check
            {
                Debug.Log("You're entitled to use this app.");

                // Call the method after passing the entitlement check
                userVerification.GetLoggedInUser();
            }
            else
            {
                if (exitAppOnFailure)
                {
                    // Implements a default behavior for an entitlement check failure -- log the failure and exit the app.
                    Debug.LogError("You're NOT entitlent to use this app.");

                    # if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                    # else
                        UnityEngine.Application.Quit();
                    # endif
                }
            }
        }   
    }
}
