using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace SwiftKampus.BusinessLogic
{
    public class SMSClass
    {
        static HttpClient client = new HttpClient();

        static async Task<String> GetSendSmsAsync(string path)
        {
            String responseStr = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                responseStr = await response.Content.ReadAsStringAsync();
            }
            return responseStr;
        }

        static void ShowResult(String result)
        {
            Console.WriteLine(result);
            //MessageBox.Show(result, "Success!", MessageBoxButtons.Ok);
            //MessageBox.Show("Your SMS has been sent successfully!", "Success!", MessageBoxButtons.OK);
        }


        public static async Task SendSMS(string SenderName, string message, string receipients)
        {
            //string UserName = "eiemmieguy93@gmail.com";
            //string key = "5be979d46321ee62db5dfaf97d75805686fe60df";
            var URL = "https://api.ebulksms.com:4433/sendsms?username=bulksms@unijos.edu.ng&apikey=b5161605879886fc73d5e3d939214ff146c89c40&sender=" + SenderName + "&messagetext=" + message + "&flash=0&recipients=" + receipients;
            try
            {
                // Make a get request
                String apiresponse = await GetSendSmsAsync(URL);
                ShowResult(apiresponse);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }
    }
}