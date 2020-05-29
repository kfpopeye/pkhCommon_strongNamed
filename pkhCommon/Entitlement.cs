using System;

namespace pkhCommon
{
    public static class EntitlementHelper
    {
        //public static bool Entitlement(string appId, string userId, string _baseApiUrl)
        //{
        //    //REST API call for the entitlement API.
        //    //We are using RestSharp for simplicity.

        //    //(1) Build request
        //    var client = new RestClient(_baseApiUrl);

        //    //Set resource/end point
        //    var request = new RestRequest();
        //    request.Resource = "webservices/checkentitlement";
        //    request.Method = Method.GET;

        //    //Add parameters
        //    request.AddParameter("userid", userId);
        //    request.AddParameter("appid", appId);

        //    //(2) Execute request and get response
        //    IRestResponse response = client.Execute(request);

        //    //Get the entitlement status.
        //    bool isValid = false;
        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        EntitlementResponse entitlementResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<EntitlementResponse>(response.Content);
        //        isValid = entitlementResponse.IsValid;

        //        System.Diagnostics.Debug.WriteLine("Entitlement: " + entitlementResponse.AppId);
        //        System.Diagnostics.Debug.WriteLine("Entitlement: " + entitlementResponse.Message);
        //    }
        //    return isValid;
        //}
    }

    [Serializable]
    public class EntitlementResponse
    {
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}