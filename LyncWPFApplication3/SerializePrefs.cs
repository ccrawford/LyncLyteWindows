using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LyncWPFApplication3
{
    [Serializable()]
    public class SerializePrefs : ISerializable
    {
        public DateTime LastSaved { get; set; }
        public string Version { get; set; }
        public Boolean ShowConfig { get; set; }
        public Boolean UseDweet { get; set; }
        public string DweetThingName { get; set; }
        public string ThingID { get; set; }
        public string ThingBaseURL { get; set; }
        public Boolean UseThing { get; set; }
        public string ThingWriteKey { get; set; }
        public ObservableCollection<UserStatus> Statuses { get; set; }

        public SerializePrefs()
        {
        }

        public SerializePrefs(SerializationInfo info, StreamingContext ctx)
        {

            foreach (SerializationEntry entry in info)
            {
                switch (entry.Name)
                {
                    case "UserStatus":
                        this.Statuses = (ObservableCollection<UserStatus>)info.GetValue("UserStatus", typeof(ObservableCollection<UserStatus>));
                        break;
                    case "LastSaved":
                        this.LastSaved = (DateTime)info.GetValue("LastSaved", typeof(DateTime));
                        break;
                    case "ShowConfig":
                        this.ShowConfig = (Boolean)info.GetValue("ShowConfig", typeof(Boolean));
                        break;
                    case "UseDweet":
                        this.UseDweet = (Boolean)info.GetValue("UseDweet", typeof(Boolean));
                        break;
                    case "DweetThingName":
                        this.DweetThingName = (string)info.GetValue("DweetThingName", typeof(string));
                        break;
                    case "UseThing":
                        this.UseThing = (Boolean)info.GetValue("UseThing", typeof(Boolean));
                        break;
                    case "ThingID":
                        this.ThingID = (string)info.GetValue("ThingID", typeof(string));
                        break;
                    case "ThingWriteKey":
                        this.ThingWriteKey = (string)info.GetValue("ThingWriteKey", typeof(string));
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown value in config.");
                        break;
                }
            }

        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UserStatus", this.Statuses);
            info.AddValue("LastSaved", DateTime.Now);
            info.AddValue("UseDweet", this.UseDweet);
            info.AddValue("ShowConfig", this.ShowConfig);
            info.AddValue("DweetThingName", this.DweetThingName);
            info.AddValue("UseThing", this.UseThing);
            info.AddValue("ThingWriteKey", this.ThingWriteKey);
            info.AddValue("ThingID", this.ThingID);
        }
    }
}
