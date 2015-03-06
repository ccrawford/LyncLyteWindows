using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyncLights;
using System.Runtime.Serialization;

namespace LyncWPFApplication3
{
    [Serializable()]
    public class UserStatus : ObservableObject, ISerializable
    {

        public string StatusName {get; set;} 
        public LIGHTS Light {get; set;}
        public string LyncStatus { get; set; }
        public bool VideoMuted { get; set; }
        public bool AudioMuted { get; set; }
        public bool MutingMatters { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { 
                _isActive = value;
                RaisePropertyChangedEvent("IsActive");
            }

        }

        public UserStatus()
        {
        }

        // Serialization constructor
        public UserStatus(SerializationInfo info, StreamingContext ctx)
        {
            try
            {
                this.StatusName = (string)info.GetValue("StatusName", typeof(string));
                this.Light = (LIGHTS)info.GetValue("Light", typeof(LIGHTS));
                this.LyncStatus = (string)info.GetValue("LyncStatus", typeof(string));
                this.VideoMuted = (bool)info.GetValue("VideoMuted", typeof(bool));
                this.AudioMuted = (bool)info.GetValue("AudioMuted", typeof(bool));
                this.MutingMatters = (bool)info.GetValue("MutingMatters", typeof(bool));
            }
            catch
            {
                return;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("StatusName", this.StatusName);
            info.AddValue("Light", this.Light);
            info.AddValue("LyncStatus", this.LyncStatus);
            info.AddValue("VideoMuted", this.VideoMuted);
            info.AddValue("AudioMuted", this.AudioMuted);
            info.AddValue("MutingMatters", this.MutingMatters);
        }
    }
}
