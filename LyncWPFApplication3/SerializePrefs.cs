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
                this.ComPort = (String)info.GetValue("SelectedComPort", typeof(string));
             }
            catch { }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UserStatus", this.Statuses);
            info.AddValue("LastSaved", DateTime.Now);
            info.AddValue("SelectedComPort", this.ComPort);
        }
    }
}
