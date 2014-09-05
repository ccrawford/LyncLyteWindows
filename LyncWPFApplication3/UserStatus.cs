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
    public class UserStatus : ISerializable
    {

        private string _statusName;

        public string StatusName
        {
            get { return _statusName; }
            set { _statusName = value; }
        }

        private LIGHTS _light;

        public LIGHTS Light
        {
            get { return _light; }
            set { _light = value; }
        }

        public string LyncStatus { get; set; }
        public bool VideoMuted { get; set; }
        public bool AudioMuted { get; set; }
        public bool MutingMatters { get; set; }


        public UserStatus()
        {
        }

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
