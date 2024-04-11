using System;

namespace ProjectQ.PlatformServices.Integration
{
    [System.Serializable]
    public class AppCredentials
    {
        public string id, username, presence, nonce, verified, age;

        public string access_token = "OC|7452013554821202|52f7e02d33a151683fef33a0c221eb7e";
        public string verification_api = "https://graph.oculus.com/user_nonce_validate";
        public string verify_entitlement_api = "https://graph.oculus.com/7452013554821202/verify_entitlement";

        public string federated_app_id = "1556425345091022";
        public string federated_access_token = "OC|1556425345091022|a084792ecb888ffa2dae3b441a661643";
    }
}
