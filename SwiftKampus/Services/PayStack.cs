using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SwiftKampus.Services
{

    public class TransactionResult
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public Data Data { get; set; }
    }

    //Note: Don't change the property names to avoid errors 
    public class Data
    {
        [JsonProperty("authorization_url")]
        public string Authorization_url { get; set; }

        [JsonProperty("access_code")]
        public string Access_code { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class PaystackPayment
    {
        private readonly string _payStackSecretKey;
        /// <summary>
        /// The secret key is needed for authentication and verification purposes. 
        /// Note: It is best to store the secret key in the web.config file.
        /// </summary>
        /// <param name="secretKey"></param>
        public PaystackPayment(string secretKey)
        {
            _payStackSecretKey = secretKey;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //Don't for get to add this. 
        }
        /// <summary>
        /// Initialize a transaction. Check out documentation on https://developers.paystack.co/docs/initialize-a-transaction
        /// </summary>
        /// <param name="amountInKobo">For eg, 5000000 is 5000 that is "5000*100"</param>
        /// <param name="email">The customer email</param>
        /// <param name="metadata">Additional data can be included. </param>
        /// <param name="resource">The optional api endpoint which is already specified</param>
        /// <returns></returns>
        public async Task<TransactionResult> InitializeTransaction(int amountInKobo, string email, string metadata = "",
            string resource = "https://api.paystack.co/transaction/initialize")
        {
            var client = new HttpClient { BaseAddress = new Uri(resource) };

            client.DefaultRequestHeaders.Add("Authorization", _payStackSecretKey);
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new Dictionary<string, string>
            {

                {"amount", amountInKobo.ToString()},
                {"email", email},
                {"metadata", metadata}
            };
            var content = new FormUrlEncodedContent(body);
            var response = await client.PostAsync(resource, content);
            if (response.IsSuccessStatusCode)
            {
                TransactionResult responseData = await response.Content.ReadAsAsync<TransactionResult>();
                return responseData;
            }
            return null;
        }

        /// <summary>
        /// Verify a transaction
        /// </summary>
        /// <param name="reference">The reference code returned from the transaction initialization result </param>
        /// <param name="resource">The optional api endpoint which is already specified</param>
        /// <returns></returns>
        public async Task<string> VerifyTransaction(string reference, string resource = "https://api.paystack.co/transaction/verify")
        {
            var client = new HttpClient { BaseAddress = new Uri(resource) };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", _payStackSecretKey);

            var response = await client.GetAsync(reference + "/");

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            return null;

        }

    }
}