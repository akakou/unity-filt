using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using MiniJSON;

namespace Filt
{
    /// <summary>
    /// The request data class of Filt Client.
    /// <summary>
    public class FiltRequest {
        public byte[] Target { get; set; }
        public Dictionary<string, string> Option { get; set; }

        public FiltRequest(byte[] target, Dictionary<string, string> option=null) {
            Target = target;

            if (option == null) {
                Option = new Dictionary<string, string>();
            } else{
                Option = option;
            }
        }
        public String ToJsonString() {
            var request = Option;
            
            string base64_target = Convert.ToBase64String(Target);
            request["target"] = base64_target;
            
            return Json.Serialize(request);
        }
    }

    /// <summary>
    /// The response class of Filt Client.
    /// <summary>
    public class FiltResponse {
        /// <summary>
        /// The json response class of Filt Client.
        /// <summary>
        [Serializable]
        public class FiltJsonResponse {
            public bool hit;
            public bool success;
            public List<String> messages;
        }
        
        public FiltResponse(bool hit, bool success, List<byte[]> messages)
        {
            Hit = hit;
            Success = success;
            Messages = messages;
        }

        public bool Hit { get; set; }
        public bool Success { get; set; }
        public List<byte[]> Messages { get; set; }

        public static FiltResponse FromString (String data) {
            var json_response = JsonUtility.FromJson<FiltJsonResponse>(data);

            Debug.Log("message:");

            Debug.Log(json_response.messages);
            List<byte[]> messages = new List<byte[]>(); 

            foreach(var base64_message in json_response.messages) {
                byte[] message = System.Convert.FromBase64String(base64_message);
                messages.Add(message);
            }

            return new FiltResponse(json_response.hit, json_response.success, messages);
        }
    }

    /// <summary>
    /// The client class of Filt.
    /// </summary>
    public class FiltClient
    {
        string Url;
        public FiltClient(string Url) {
            this.Url = Url;
        }

        /// <summary>
        /// Send data to Filt, and return result.
        /// </summary>
        public FiltResponse Send(FiltRequest Request, bool Verify=true) {
            // init handler
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (!Verify) {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(delegate { return true; });

            }

            // send to filt server
            WebClient web = new WebClient();

            web.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            web.Headers[HttpRequestHeader.Accept] = "application/json";
            var hoge = Request.ToJsonString();

            Debug.Log(hoge);

            // deserialize to object
            string string_response = web.UploadString(new Uri(Url),hoge );
            web.Dispose();

            return FiltResponse.FromString(string_response);
        }
    }
}
