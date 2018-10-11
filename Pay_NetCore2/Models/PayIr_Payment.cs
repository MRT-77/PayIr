using System;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PayIr
{
    public static class Payment
    {
        private const string UrlSend = "https://pay.ir/payment/send";
        private const string UrlResult = "https://pay.ir/payment/verify";
        private const string UrlGateway = "https://pay.ir/payment/gateway/";

        public static PayResponse Pay(PayRequest pay)
        {
            if (pay == null)
                throw new NullReferenceException(nameof(pay));

            string postString = string.Join("&", GetEncodeUrlParamsQueryString(pay));

            HttpWebRequest webRequest = WebRequest.CreateHttp(UrlSend);
            webRequest.Method = "POST";
            webRequest.ContentLength = postString.Length;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                streamWriter.Write(postString);
                streamWriter.Flush();
                streamWriter.Dispose();
            }

            Stream responseStream = null;
            try
            {
                var webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                    responseStream = e.Response.GetResponseStream();
            }

            string result = "";
            if (responseStream != null)
                using (StreamReader streamReader = new StreamReader(responseStream))
                {
                    result = streamReader.ReadToEnd();
                    streamReader.Dispose();
                }

            return JsonConvert.DeserializeObject<PayResponse>(result);
        }

        public static VerifyResponse Verify(VerifyRequest verify)
        {
            if (verify == null)
                throw new NullReferenceException(nameof(verify));

            string postString = GetEncodeUrlParamsQueryString(verify);

            HttpWebRequest webRequest = WebRequest.CreateHttp(UrlResult);
            webRequest.Method = "POST";
            webRequest.ContentLength = postString.Length;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Timeout = 10 * 6000;
            using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                streamWriter.Write(postString);
                streamWriter.Flush();
                streamWriter.Dispose();
            }

            Stream responseStream = null;
            try
            {
                var webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                    responseStream = e.Response.GetResponseStream();
            }

            string result = "";
            if (responseStream != null)
                using (StreamReader streamReader = new StreamReader(responseStream))
                {
                    result = streamReader.ReadToEnd();
                    streamReader.Dispose();
                }

            return JsonConvert.DeserializeObject<VerifyResponse>(result);
        }

        public static string GatewayAddress(string transId)
        {
            return UrlGateway + transId;
        }

        public static VerifyData GetFormVerifyData(IFormCollection form)
        {
            VerifyData verifyData = new VerifyData();
            var properties = typeof(VerifyData).GetProperties();
            foreach (var property in properties)
            {
                if (!form.TryGetValue(property.Name, out var values)) continue;
                string value = values.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(value)) continue;
                property.SetValue(verifyData, Convert.ChangeType(value, property.PropertyType));
            }
            return verifyData;
        }

        public static string GetEncodeUrlParamsQueryString<T>(T t)
        {
            var properties = typeof(T).GetProperties();
            string queryString = "";
            foreach (var property in properties)
            {
                object propertyValue = property.GetValue(t);
                if (propertyValue == null) continue;
                queryString += property.Name + "=" + HttpUtility.UrlEncode(propertyValue.ToString()) + "&";
            }
            return queryString.TrimEnd('&');
        }
    }

    public class PayRequest
    {
        public string api { get; set; }
        public int amount { get; set; }
        public string redirect { get; set; }
        public string mobile { get; set; }
        public string factorNumber { get; set; }
        public string description { get; set; }
    }
    public class PayResponse
    {
        public int status { get; set; }
        public string transId { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class VerifyRequest
    {
        public string api { get; set; }
        public int transId { get; set; }
    }
    public class VerifyResponse
    {
        public int status { get; set; }
        public int amount { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class VerifyData
    {
        public int status { get; set; }
        public int transId { get; set; }
        public string factorNumber { get; set; }
        public string mobile { get; set; }
        public string description { get; set; }
        public string cardNumber { get; set; }
        public string traceNumber { get; set; }
        public string message { get; set; }
    }
}