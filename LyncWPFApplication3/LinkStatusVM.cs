using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyncLights;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Input;

namespace LyncWPFApplication3
{
    class LinkStatusVM : ObservableObject
    {
        public LinkStatusVM()
        {

        }

        public void CreateDefaultStatuses()
        {
            _userStatus.Add(new UserStatus { StatusName = "Available", Light = LIGHTS.GREEN, LyncStatus = "Available", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Do not Disturb", Light = LIGHTS.YELLOW, LyncStatus = "Do Not Disturb", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Presenting", Light = LIGHTS.OFF, LyncStatus = "Presenting", MutingMatters = false });

            _userStatus.Add(new UserStatus { StatusName = "In a conference call both muted", Light = LIGHTS.YELLOW, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = true });
            _userStatus.Add(new UserStatus { StatusName = "In a conference call muted but on camera", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = false });
            _userStatus.Add(new UserStatus { StatusName = "In a conference call mic on", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = false, VideoMuted = true });
            _userStatus.Add(new UserStatus { StatusName = "In a conference call both on", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = false, VideoMuted = false });

            _userStatus.Add(new UserStatus { StatusName = "Busy", Light = LIGHTS.YELLOW, LyncStatus = "Busy", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "In a meeting", Light = LIGHTS.YELLOW, LyncStatus = "In a meeting", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Be right back", Light = LIGHTS.GREEN, LyncStatus = "Be right back", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Inactive", Light = LIGHTS.GREEN, LyncStatus = "Inactive", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Away", Light = LIGHTS.OFF, LyncStatus = "Away", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Off work", Light = LIGHTS.OFF, LyncStatus = "Off work", MutingMatters = false });

            _userStatus.Add(new UserStatus { StatusName = "Default", Light = LIGHTS.OFF, LyncStatus = "Default", MutingMatters = false });
        }



        private bool _isMicMuted;
        public bool isMicMuted
        {
            get { return _isMicMuted; }
            set
            {
                _isMicMuted = value;
                RaisePropertyChangedEvent("isMicMuted");
            }
        }

        private bool _isVideoOff;
        public bool isVideoOff
        {
            get { return _isVideoOff; }
            set
            {
                _isVideoOff = value;
                RaisePropertyChangedEvent("isVideoOff");
            }
        }

        ObservableCollection<UserStatus> _userStatus = new ObservableCollection<UserStatus>();
        public ObservableCollection<UserStatus> UserStatuses
        {
            get { return _userStatus; }
            set
            {
                _userStatus = value;
                RaisePropertyChangedEvent("UserStatuses");
            }
        }

        public string currentLyncStatus { get; set; }

        private LIGHTS _currentLight;
        public LIGHTS currentLight
        {
            get { return _currentLight; }
            set
            {
                _currentLight = value;
                RaisePropertyChangedEvent("currentLight");
                RaisePropertyChangedEvent("currentLightColor");
            }
        }

        public string currentLightColor
        {
            get
            {

                switch (_currentLight)
                {
                    case LIGHTS.RED:
                        return "Red";
                        break;
                    case LIGHTS.YELLOW:
                        return "Yellow";
                        break;
                    case LIGHTS.GREEN:
                        return "Green";
                        break;
                    case LIGHTS.OFF:
                        return "Off";
                        break;
                    case LIGHTS.STATUS:
                        return "Status";
                        break;
                    default:
                        return "Not Set";
                        break;
                }
            }
        }


        private string _comPort { get; set; }
        public string ComPort
        {
            get { return _comPort; }
            set
            {
                _comPort = value;
                RaisePropertyChangedEvent("ComPort");
            }
        }

        public string[] ComPorts
        {
            get
            {
                return SerialPort.GetPortNames();
            }
        }


        public string _comLinkStatus;
        public string comLinkStatus
        {
            get { return _comLinkStatus; }
            set
            {
                _comLinkStatus = value;
                RaisePropertyChangedEvent("comLinkStatus");
            }
        }

        public bool isComLinked { get; set; }
        public bool isLyncLinked { get; set; }
        public string lyncConnectionStatus { get; set; }

        void TestLightExecute()
        {

        }

        bool CanTestLightExecute()
        {
            // Add if connected
            return true;
        }

        public ICommand TestLight { get { return new RelayCommand(TestLightExecute, CanTestLightExecute); } }

    }
}
