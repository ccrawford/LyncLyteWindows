using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Thingspeak_CAC
{
    public class Thingspeak
    {
        public string ChannelID;
        public Dictionary<int, string> Content;
        public string WriteAPI;
        public string ReadAPI;
        public string BaseURL;
        private DateTime lastWrite;
        private TimeSpan apiRate = new TimeSpan(0, 0, 15);



        private string _response;

        public Thingspeak(string channelId, Dictionary<int, string> content, string writeApi)
        {
            ChannelID = channelId;
            Content = content;
            WriteAPI = writeApi;
            BaseURL = "https://api.thingspeak.com";
        }
        public Thingspeak(string channelId, Dictionary<int, string> content, string writeApi, string baseURL)
        {
            ChannelID = channelId;
            Content = content;
            WriteAPI = writeApi;
            BaseURL = baseURL;
        }

        public Thingspeak()
        {
        }

        public int ThingIt()
        {
            HttpWebRequest httpWebRequest = 
                (HttpWebRequest)WebRequest.Create(BaseURL + "/update");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";


            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string data = "key=" + WriteAPI;
                    foreach (var pair in Content)
                    {
                        if (pair.Key >= 1 && pair.Key <= 8)
                            data = string.Concat(data, (string.Format("&field{0}={1}", pair.Key, pair.Value)));
                    }

                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();

                }
            }
            catch (Exception e)
            {
                return -1;
            }

            string result;
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    streamReader.Close();
                }
            }
            catch (Exception e)
            {
               result = "-1";
            }
            
            return int.Parse(result);
            
        }

    }
}


