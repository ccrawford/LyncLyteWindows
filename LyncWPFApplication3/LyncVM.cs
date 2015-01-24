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
using GongSolutions.Wpf.DragDrop.Utilities;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using System.Windows;
using Dweet_CAC;
using Thingspeak_CAC;

namespace LyncWPFApplication3
{
    class LyncVM : ObservableObject, IDropTarget
    {

        // private LyncComm _comm;
        private LyncUSB _usb;
        private LyncInterface _lync;
        private DweetNet _dweet;
        private Thingspeak _ts;

        public LyncVM()
        {
            // Set the user's preferences from the pref file.
            DependencyObject dep = new DependencyObject();
            if (!DesignerProperties.GetIsInDesignMode(dep))
            {
                _usb = new LyncUSB();
                _usb.UsbTransmitterChangeNotifier += _usb_UsbTransmitterChangeNotifier;
            }

            _userStatus = new ObservableCollection<UserStatus>();
            // _userStatus.CollectionChanged += _userStatus_CollectionChanged;
            LoadPrefs();

            _dweet = new DweetNet(dweetThingName, null);
            _ts = new Thingspeak();

            _lync = new LyncInterface();
            _lync.PropertyChanged += _lync_PropertyChanged;

            // removed the this's
            redLights = new LightCollection(LIGHTS.RED);
            yellowLights = new LightCollection(LIGHTS.YELLOW);
            greenLights = new LightCollection(LIGHTS.GREEN);
            offLights = new LightCollection(LIGHTS.OFF);

            foreach (UserStatus s in this.UserStatuses)
            {
                PutStatusInLightBucket(s);
            }

            // Prevent the xaml designer from grabbing the com port.
            if (!DesignerProperties.GetIsInDesignMode(dep))
            {
                PresenceToLight(_lync.curPresence);
            }

        }

        // removed the this's
        private void PutStatusInLightBucket(UserStatus s)
        {
            switch (s.Light)
            {
                case LIGHTS.RED:
                    App.Current.Dispatcher.Invoke((Action)delegate { redLights.userStatuses.Add(s); });
                    break;
                case LIGHTS.YELLOW:
                    App.Current.Dispatcher.Invoke((Action)delegate { yellowLights.userStatuses.Add(s); });
                    break;
                case LIGHTS.GREEN:
                    App.Current.Dispatcher.Invoke((Action)delegate { greenLights.userStatuses.Add(s); });
                    break;
                case LIGHTS.OFF:
                    App.Current.Dispatcher.Invoke((Action)delegate { offLights.userStatuses.Add(s); });
                    break;
                case LIGHTS.STATUS:
                    App.Current.Dispatcher.Invoke((Action)delegate { offLights.userStatuses.Add(s); });
                    break;
                default:
                    break;
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            UserStatus sourceItem = dropInfo.Data as UserStatus;
            ObservableCollection<UserStatus> targetCollection = dropInfo.TargetCollection as ObservableCollection<UserStatus>;
            ObservableCollection<UserStatus> sourceCollection = dropInfo.DragInfo.SourceCollection as ObservableCollection<UserStatus>;

            // Need to add a check for the droppable type
            if (sourceItem != null && targetCollection != null && (targetCollection != sourceCollection))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }

        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            UserStatus sourceItem = (UserStatus)dropInfo.Data;
            var targetCollection = (ObservableCollection<UserStatus>)dropInfo.TargetCollection;
            targetCollection.Add(sourceItem);
            var sourceCollection = (ObservableCollection<UserStatus>)dropInfo.DragInfo.SourceCollection;
            // Problem here
            sourceCollection.Remove(sourceItem);
            PresenceToLight(_lync.curPresence);
        }

        void _usb_UsbTransmitterChangeNotifier(object sender, EventArgs e)
        {
            Debug.WriteLine("USB status changed: " + e.ToString());
            // RaisePropertyChangedEvent("comLinkStatus");
            comLinkStatus = "changed";
        }

        // Watch for changes in the Lync status and update lights as necessary.
        void _lync_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // There must be a better way to do this...
            if (e.PropertyName == "curPresence" || e.PropertyName == "isMicMuted" || e.PropertyName == "isVideoOff")
            {
                isMicMuted = _lync.isMicMuted;
                isVideoOff = _lync.isVideoOff;
                PresenceToLight(_lync.curPresence);
            }
        }

        public void CleanUp()
        {
            // Save the VM Prefs to a file on shutdown.
            SavePrefsExecute(null);
            _usb.ActivateLight(LIGHTS.OFF);
            // _usb.CleanUp();
            _lync = null;
        }

        public LightCollection redLights { get; set; }
        public LightCollection yellowLights { get; set; }
        public LightCollection greenLights { get; set; }
        public LightCollection offLights { get; set; }

        //void comm_PortsChanged(object sender, PortsChangedArgs e)
        //{
        //    comLinkStatus = "New port found";
        //    ComPorts = new System.Collections.ObjectModel.ObservableCollection<string>(e.SerialPorts);
        //}


        //void comm_CommStatusChanged(object sender, CommStatusChanagedEventArgs e)
        //{
        //    Debug.WriteLine("Comm stataus changed: " + e.NewStatus.ToString());
        //    ProcessComStatus(e.NewStatus);
        //}

        //void ProcessComStatus(COM_STATUS curStatus)
        //{
        //    switch (curStatus)
        //    {
        //        case COM_STATUS.Connected:
        //            comLinkStatus = "Connected";
        //            break;
        //        case COM_STATUS.Disconnected:
        //            comLinkStatus = "Disconnected";
        //            break;
        //        case COM_STATUS.BadPort:
        //            comLinkStatus = "Check Port selection";
        //            break;
        //        case COM_STATUS.NotDetermined:
        //            comLinkStatus = "Checking...";
        //            break;
        //        default:
        //            comLinkStatus = "Com Status unknown.";
        //            break;
        //    }
        //}

        #region Preferences

        public void CreateDefaultStatuses()
        {

            _userStatus.Add(new UserStatus { StatusName = "Available", Light = LIGHTS.GREEN, LyncStatus = "Available", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Do not Disturb", Light = LIGHTS.YELLOW, LyncStatus = "Do Not Disturb", MutingMatters = false });
            _userStatus.Add(new UserStatus { StatusName = "Presenting", Light = LIGHTS.OFF, LyncStatus = "Presenting", MutingMatters = false });

            _userStatus.Add(new UserStatus { StatusName = "In conf call both muted", Light = LIGHTS.YELLOW, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = true });
            _userStatus.Add(new UserStatus { StatusName = "In conf call mic off, camera on", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = true, VideoMuted = false });
            _userStatus.Add(new UserStatus { StatusName = "In conf call mic on camera off", Light = LIGHTS.RED, LyncStatus = "In a conference call", MutingMatters = true, AudioMuted = false, VideoMuted = true });
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
            if (prefs == null) 
            {
                CreateDefaultStatuses();
                return false;
            }

            if (prefs.Statuses != null)
            {
                UserStatuses = prefs.Statuses;
            }
            else CreateDefaultStatuses();

/*            
 *          if (prefs.ComPort != null)
            {
                ComPort = prefs.ComPort;
            }
            else ComPort = "COM5";

            // _comPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            
 */
            if (prefs.UseDweet != null) useDweet = prefs.UseDweet;
            if (prefs.DweetThingName != null) dweetThingName = prefs.DweetThingName;

            if (prefs.UseThing != null) useThingSpeak = prefs.UseThing;
            if (prefs.ThingBaseURL != null) thingBaseURL = prefs.ThingBaseURL;
            if (prefs.ThingID != null) thingID = prefs.ThingID;
            if (prefs.ThingWriteKey != null) thingWriteKey = prefs.ThingWriteKey;

            return true;
        }

        private void SavePrefsExecute(object state)
        {
            // Save Prefs
            SerializePrefs prefs = new SerializePrefs();
            prefs.Statuses = UserStatuses;
            prefs.ComPort = ComPort;
            prefs.DweetThingName = dweetThingName;
            prefs.UseDweet = useDweet;
            prefs.ThingID = thingID;
            prefs.UseThing = useThingSpeak;
            prefs.ThingWriteKey = thingWriteKey;
            Serializer serializer = new Serializer();
            serializer.SerializeObject(prefs);
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
                micState = _isMicMuted ? "Muted" : "Live";
                RaisePropertyChangedEvent("isMicMuted");
            }
        }

        public string micState
        {
            get { return _lync.isMicMuted ? "Muted" : "Live"; }
            set { RaisePropertyChangedEvent("micState"); }
        }

        private bool _useDweet;
        public bool useDweet
        {
            get { return _useDweet; }
            set { 
                _useDweet = value;
                RaisePropertyChangedEvent("useDweet");
            }
        }

        private string _dweetThingName;
        public string dweetThingName
        {
            get { return _dweetThingName; }
            set
            {
                _dweetThingName = value;
                RaisePropertyChangedEvent("dweetThingName");
            }
        }

#region ThingSpeak
        private bool _useThingSpeak;
        public bool useThingSpeak
        {
            get { return _useThingSpeak; }
            set { 
                _useThingSpeak = value;
                RaisePropertyChangedEvent("useThingSpeak");
            }
        }

        private string _thingID;
        public string thingID
        {
            get { return _thingID; }
            set
            {
                _thingID = value;
                RaisePropertyChangedEvent("thingID");
            }
        }

        private string _thingBaseURL;
        public string thingBaseURL
        {
            get { return _thingBaseURL; }
            set
            {
                _thingBaseURL = value;
                RaisePropertyChangedEvent("thingBaseURL");
            }
        }

        private string _thingWriteKey;
        public string thingWriteKey
        {
            get { return _thingWriteKey; }
            set
            {
                _thingWriteKey = value;
                RaisePropertyChangedEvent("thingWriteKey");
            }
        }


#endregion

        private Dictionary<LIGHTS, System.Drawing.Icon> lightIconResource = new Dictionary<LIGHTS, System.Drawing.Icon>
        {
            { LIGHTS.RED, Properties.Resources.red},
            { LIGHTS.YELLOW, Properties.Resources.yellow},
            { LIGHTS.GREEN, Properties.Resources.green},
            { LIGHTS.OFF, Properties.Resources.off}
        };

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
        private string _videoStatus;
        public bool isVideoOff
        {
            get
            {
                return _lync.isVideoOff;
                // return _isVideoOff; 
            }
            set
            {
                _isVideoOff = value;
                videoStatus = value ? "Off" : "On";
                RaisePropertyChangedEvent("isVideoOff");
            }
        }

        public string videoStatus
        {
            get { return _lync.isVideoOff ? "Off" : "On"; }
            set { _videoStatus = value; RaisePropertyChangedEvent("videoStatus"); }
        }


        ObservableCollection<UserStatus> _userStatus;
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
                _usb.ActivateLight(value);

                _currentLight = value;
                curWinIcon = lightIcon[value];
                // iconName = "Icons/green.png";
                RaisePropertyChangedEvent("currentLight");
                RaisePropertyChangedEvent("currentLightColor");
                RaisePropertyChangedEvent("iconName");
                SetDweetValue(_currentLight.ToString(), _lync.curPresence);
            }
        }

        private void SetDweetValue(string LightName, string Presence)
        {
            if (useDweet)
            {
                _dweet.Thing = dweetThingName;
                _dweet.Content = new Dictionary<string, string> { { "color", LightName }, {"presence", Presence } };
                _dweet.DweetIt();
            }
            if (useThingSpeak)
            {
                _ts.BaseURL = thingBaseURL;
                _ts.ChannelID = thingID;
                _ts.WriteAPI = thingWriteKey;
                _ts.Content = new Dictionary<int,string> {{1, LightName}};
                _ts.ThingIt();
            }

        }

        public void refreshLight()
        {
            if (_usb.IsAvailable) _usb.ActivateLight(_currentLight);
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
            //Deactivate old status.
            var last_statuses = UserStatuses.Where(s => s.IsActive == true);
            foreach (var s in last_statuses)
            {
                s.IsActive = false;
            }



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
                user_status.IsActive = true;

                // RaisePropertyChangedEvent("IsActive");
            }
            else
            {
                //New Status found. Add it to the list.
                var s = new UserStatus { LyncStatus = presence, StatusName = presence, Light = LIGHTS.OFF, AudioMuted = false, VideoMuted = false, IsActive = true };
                // The list was created in the UI thread...can only be modified from that thread.
                UserStatuses.Add(s); 
                PutStatusInLightBucket(s);
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


        public string comLinkStatus
        {
            get { return _usb.IsAvailable ? "Connected" : "Disconnected"; }
            set
            {
                RaisePropertyChangedEvent("comLinkStatus");
                RaisePropertyChangedEvent("IsLinkAvailable");
                refreshLight();
            }
        }

        public bool IsLinkAvailable { get { return _usb.IsAvailable; } }

        #region Commands

        bool CanTestLightExecute()
        {
            return _usb.IsAvailable;
            // return true;
        }

        void TestLightExecute(object state)
        {
            LIGHTS newLight;
            if (state == null) PresenceToLight(_lync.curPresence);
            if (Enum.TryParse(state.ToString(), out newLight)) currentLight = newLight;
            else PresenceToLight(_lync.curPresence);
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

        void DeleteItemExecute(object state)
        {
            Debug.WriteLine("Delete item: " + state.ToString());
            var stateToDelete = (UserStatus)state;
            if (stateToDelete.IsActive) return; // Don't try to delete an active state.
            UserStatuses.Remove(stateToDelete);
        }
        private RelayCommand _DeleteItemCommand;
        public RelayCommand DeleteItemCommand
        {
            get
            {
                if (_DeleteItemCommand == null)
                {
                    _DeleteItemCommand = new RelayCommand(DeleteItemExecute);
                }
                return _DeleteItemCommand;
            }

        }

        private RelayCommand _SavePrefs;
        public RelayCommand SavePrefs
        {
            get
            {
                if (_SavePrefs == null)
                {
                    _SavePrefs = new RelayCommand(SavePrefsExecute);
                }
                return _SavePrefs;
            }
        }
        bool CanSavePrefs() { return true; }

        private RelayCommand _closeWindow;
        public RelayCommand CloseWindow
        {
            get
            {
                if (_closeWindow == null)
                {
                    _closeWindow = new RelayCommand(CloseWindowExecute);
                }
                return _closeWindow;
            }
        }
        private void CloseWindowExecute(object state)
        {
            this.CleanUp();
        }

        #endregion

    }

    internal class LightCollection
    {
        public LightCollection(LIGHTS lightColor)
        {
            Light = lightColor;
            _userStatuses = new ObservableCollection<UserStatus>();
            _userStatuses.CollectionChanged += _userStatuses_CollectionChanged;

        }

        void _userStatuses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Don't try to process removes!
            if (e.NewStartingIndex == -1)
            {
                return;
            }
            foreach (var s in e.NewItems)
            {
                var item = (UserStatus)s;
                if (item != null && item.Light != this.Light)
                {
                    item.Light = this.Light;

                }
            }
        }


        public LIGHTS Light { get; set; }

        private ObservableCollection<UserStatus> _userStatuses;
        public ObservableCollection<UserStatus> userStatuses { get { return _userStatuses; } }
    }
}
