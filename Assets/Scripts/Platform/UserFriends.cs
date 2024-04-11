using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;

namespace ProjectQ.PlatformServices.Integration
{
    public class UserFriends : MonoBehaviour
    {
       // <Summary>
        // If the user is fully verified, You can now start calling UserFriends,
        // to show on its leaderboard, achievements, etc.
        // </Summary>

        public UserVerification userVerification;

        private bool hasCalledGetFriends = false;

        [SerializeField]
        private List<string> userLists = new List<string>();

        [SerializeField]
        private TMP_Text user_friends_log;

        void Start() {
            userVerification = GetComponent<UserVerification>();
        }

        void FixedUpdate() {
            if (!userVerification.HasVerificationSucceeded() || hasCalledGetFriends) return;

            GetFriends();
            hasCalledGetFriends = true;
        }

        void GetFriends() {
            Users.GetLoggedInUserFriends().OnComplete(OnGetUserFriendsCallback);
        }

        void OnGetUserFriendsCallback(Message<UserList> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting logged in user friends: {0}", msg.GetError().Message);
            }
            else
            {
                PopulateUserFriends(msg);
            }
        }

        private void PopulateUserFriends(Message<UserList> msg) {
            UserList users = msg.GetUserList();

            foreach (User user in users) {
                userLists.Add("Display Name: " + user.DisplayName + "\n" + "Status: " + user.Presence + "\n \n");

                userVerification.log.text += $"Display Name: + {user.DisplayName} ::::: Status: {user.Presence} \n\n";
            }
        }
    }
}
