using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dweet_CAC
{
    public class DweetNet
    {
        public string Thing;
        public Dictionary<string, string> Content;

        private string _response;

        public DweetNet(string thing, Dictionary<string, string> content)
        {
            Thing = thing;
            Content = content;
        }

        public void DweetIt()
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://dweet.io/dweet/for/" + Thing);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = null;
                foreach(var pair in Content)
                {
                    json = string.IsNullOrEmpty(json) ? "{" : json + ",";
                    json = string.Format("{0} \"{1}\":\"{2}\"", json, pair.Key, pair.Value);
                }
                
                json = json + "}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                // Skip the update.
               // Return something bad.
            }

        }
    }
}
