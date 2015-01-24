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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace LyncWPFApplication3
{
    class LinkStatusVM : ObservableObject
    {

        private LyncComm _comm;
        private LyncUSB _usb;
        private LyncInterface _lync;

        public LinkStatusVM()
        {
            // Set the user's preferences from the pref file.
            LoadPrefs();
            
            _lync = new LyncInterface();
            _lync.PropertyChanged += _lync_PropertyChanged;
            
            // Prevent the xaml designer from grabbing the com port.
            DependencyObject dep = new DependencyObject();
            if (!DesignerProperties.GetIsInDesignMode(dep))
            {
                _usb = new LyncUSB();
                InitializeComm();
                PresenceToLight(_lync.curPresence);
            }


        }

        // Watch for changes in the Lync status and update lights as necessary.
        void _lync_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // There must be a better way to do this...
            if (e.PropertyName == "curPresence" || e.PropertyName == "isMicMuted" || e.PropertyName == "isVideoOff" )
            {
                isMicMuted = _lync.isMicMuted;
                isVideoOff = _lync.isVideoOff;
                PresenceToLight(_lync.curPresence);
            }
        }



        public void CleanUp()
        {
            // Save the VM Prefs to a file on shutdown.
            SavePrefs();
            _comm.ActivateLight(LIGHTS.OFF);
            _comm.CleanUp();
        }
        

        private void InitializeComm()
        {
            _comm = new LyncComm(ComPort);
            _comm.CommStatusChanged += comm_CommStatusChanged;
            _comm.PortsChanged += comm_PortsChanged;
            ProcessComStatus(_comm.CommunicationStatus);
        }

        void comm_PortsChanged(object sender, PortsChangedArgs e)
        {
            comLinkStatus = "New port found";
            ComPorts = new System.Collections.ObjectModel.ObservableCollection<string>(e.SerialPorts);
        }


        void comm_CommStatusChanged(object sender, CommStatusChanagedEventArgs e)
        {
            Debug.WriteLine("Comm stataus changed: " + e.NewStatus.ToString());
            ProcessComStatus(e.NewStatus);
        }

        void ProcessComStatus(COM_STATUS curStatus)
        {
            switch (curStatus)
            {
                case COM_STATUS.Connected:
                    comLinkStatus = "Connected";
                    break;
                case COM_STATUS.Disconnected:
                    comLinkStatus = "Disconnected";
                    break;
                case COM_STATUS.BadPort:
                    comLinkStatus = "Check Port selection";
                    break;
                case COM_STATUS.NotDetermined:
                    comLinkStatus = "Checking...";
                    break;
                default:
                    comLinkStatus = "Com Status unknown.";
                    break;
            }
        }
        #region Preferences

        public void CreateDefaultStatuses()
        {
            _userStatus.Add(new UserStatus { StatusName = "Available", Light = LIGHTS.GREEN, LyncStatus = "Available", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Do not Disturb", Light = LIGHTS.RED, LyncStatus = "Do Not Disturb", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Presenting", Light = LIGHTS.RED, LyncStatus = "Presenting", MutingMatters = false });

            _userStatus.Add(new UserStatus { StatusName = "In conf call both muted", Light = LIGHTS.YELLOW, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = true });
            _userStatus.Add(new UserStatus { StatusName = "In conf call muted, cam on", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = false });
            _userStatus.Add(new UserStatus { StatusName = "In conf call mic on, cam off", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = false, VideoMuted = true });
            _userStatus.Add(new UserStatus { StatusName = "In conf call both on", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = false, VideoMuted = false });

            _userStatus.Add(new UserStatus { StatusName = "Busy", Light = LIGHTS.YELLOW, LyncStatus = "Busy", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "In a meeting", Light = LIGHTS.YELLOW, LyncStatus = "In a meeting", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Be right back", Light = LIGHTS.GREEN, LyncStatus = "Be right back", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Inactive", Light = LIGHTS.GREEN, LyncStatus = "Inactive", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Away", Light = LIGHTS.OFF, LyncStatus = "Away", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Off work", Light = LIGHTS.OFF, LyncStatus = "Off work", MutingMatters = false });

            _userStatus.Add(new UserStatus { StatusName = "Default", Light = LIGHTS.OFF, LyncStatus = "Default", MutingMatters = false });
        }

        private bool LoadPrefs()
        {

            Serializer serializer = new Serializer();
            SerializePrefs prefs = new SerializePrefs();

            prefs = serializer.DeSerializeObject();
            if (prefs != null && prefs.Statuses != null)
            {
                UserStatuses = prefs.Statuses;
            }
            else CreateDefaultStatuses();

            /*
            if (prefs != null && prefs.ComPort != null)
            {
                ComPort = prefs.ComPort;
            }
            else ComPort = "COM5";

            _comPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            */

            return true;
        }

        private void SavePrefs()
        {
            // Save Prefs
            SerializePrefs prefs = new SerializePrefs();
            prefs.Statuses = UserStatuses;
            // prefs.ComPort = ComPort;
            Serializer serializer = new Serializer();
            serializer.SerializeObject(prefs);
        }

        void SavePrefsExecute()
        {
            SavePrefs();
        }

        bool CanSavePrefsExecute()
        {
            return true;
        }


        #endregion
        
        private bool _isMicMuted;
        public bool isMicMuted
        {
            get
            {
                return _lync.isMicMuted; 
                //return _isMicMuted; 
            }
            set
            {
                _isMicMuted = value;
                RaisePropertyChangedEvent("isMicMuted");
            }
        }

        private Dictionary<LIGHTS, String> lightIcon = new Dictionary<LIGHTS, String> 
        {
            { LIGHTS.RED, "/Icons/red.png"},
            { LIGHTS.YELLOW, "/Icons/yellow.png"},
            { LIGHTS.GREEN, "/Icons/green.png"},
            { LIGHTS.OFF, "/Icons/off.png"}
        };

        private String _curWinIcon;
        public String curWinIcon
        {
            get { return _curWinIcon; }
            set
            {
                _curWinIcon = value;
                RaisePropertyChangedEvent("curWinIcon");
            }
        }

        private string _iconName;
        public string iconName
        {
            get
            {
                // return "Icons/red.png"; 
                if (currentLight == 0) return "/Icons/off.png";
                return lightIcon[(LIGHTS)currentLight];
            }
            set
            {
                _iconName = value;
                RaisePropertyChangedEvent("iconName");
            }
        }

        public string appName
        {
            get { return "Lyte Up"; }
        }

        private bool _isVideoOff;
        public bool isVideoOff
        {
            get {
                return _lync.isVideoOff;
                // return _isVideoOff; 
            }
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

        private LIGHTS _currentLight;
        public LIGHTS currentLight
        {
            get { return _currentLight; }
            set
            {
                _comm.ActivateLight(value);

                _currentLight = value;
                curWinIcon = lightIcon[value];
                // iconName = "Icons/green.png";
                RaisePropertyChangedEvent("currentLight");
                RaisePropertyChangedEvent("currentLightColor");
                RaisePropertyChangedEvent("iconName");
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
                    case LIGHTS.YELLOW:
                        return "Yellow";
                    case LIGHTS.GREEN:
                        return "Green";
                    case LIGHTS.OFF:
                        return "Off";
                    case LIGHTS.STATUS:
                        return "Status";
                    default:
                        return "Not Set";
                }
            }
        }

        public void PresenceToLight(string presence)
        {
            var user_status = UserStatuses.Where(s => s.LyncStatus == presence).FirstOrDefault();
            var user_statuses = UserStatuses.Where(s => s.LyncStatus == presence);
            if (user_statuses.Count() > 1)
            {
                //Check for muting condition
                user_status = user_statuses.Where(s => s.AudioMuted == isMicMuted && s.VideoMuted == isVideoOff).FirstOrDefault();
            }


            if (user_status != null)
            {
                currentLight = user_status.Light;
            }
            else
            {
                //New Status found. Add it to the list.
                UserStatuses.Add(new UserStatus { LyncStatus = presence, StatusName = presence, Light = LIGHTS.OFF, AudioMuted = false, VideoMuted = false });
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


        private ObservableCollection<string> _comPorts = new ObservableCollection<String>();
        public ObservableCollection<string> ComPorts
        {
            get
            {
                return _comPorts;
            }
            set
            {
                _comPorts = value;
                RaisePropertyChangedEvent("ComPorts");
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


        void TestLightExecute(object state)
        {
            LIGHTS newLight;
            if (state == null) return;
            if (Enum.TryParse(state.ToString(), out newLight)) currentLight = newLight;
        }

        bool CanTestLightExecute()
        {
            // Add if connected
            return true;
        }

        private RelayCommand _TestLight;
        public RelayCommand TestLight
        {
            get
            {
                if (_TestLight == null)
                {
                    _TestLight = new RelayCommand(TestLightExecute);
                }
                return _TestLight;
            }
        }
            

    }
}
