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

        private string _lyncStatus;
        public string LyncStatus {
            get { return _lyncStatus; }
            set { _lyncStatus = value; }
        }

        private bool _videoMuted;
        public bool VideoMuted { 
            get { return _videoMuted; } 
            set { _videoMuted = value; } 
        }

        private bool _audioMuted;
        public bool AudioMuted
        {
            get { return _audioMuted; }
            set { _audioMuted = value; }
        }

        private bool _mutingMatters;
        public bool MutingMatters
        {
            get { return _mutingMatters; }
            set { _mutingMatters = value; }
        }

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
