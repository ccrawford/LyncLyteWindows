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
        public string ComPort { get; set; }
        public DateTime LastSaved { get; set; }
        public string Version { get; set; }
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
            try
            {
                this.Statuses = (ObservableCollection<UserStatus>)info.GetValue("UserStatus", typeof(ObservableCollection<UserStatus>));
                this.LastSaved = (DateTime)info.GetValue("LastSaved", typeof(DateTime));
                this.ComPort = (string)info.GetValue("SelectedComPort", typeof(string));
                this.UseDweet = (Boolean)info.GetValue("UseDweet", typeof(Boolean));
                this.DweetThingName = (string)info.GetValue("DweetThingName", typeof(string));
                this.UseThing = (Boolean)info.GetValue("UseThing", typeof(Boolean));
                this.ThingID = (string)info.GetValue("ThingID", typeof(string));
                this.ThingWriteKey = (string)info.GetValue("ThingWriteKey", typeof(string));

             }
            catch { }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UserStatus", this.Statuses);
            info.AddValue("LastSaved", DateTime.Now);
            info.AddValue("SelectedComPort", this.ComPort);
            info.AddValue("UseDweet", this.UseDweet);
            info.AddValue("DweetThingName", this.DweetThingName);
            info.AddValue("UseThing", this.UseThing);
            info.AddValue("ThingWriteKey", this.ThingWriteKey);
            info.AddValue("ThingID", this.ThingID);
        }
    }
}
