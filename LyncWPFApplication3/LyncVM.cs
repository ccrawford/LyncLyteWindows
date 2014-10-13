﻿using System;
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

namespace LyncWPFApplication3
{
    class LyncVM : ObservableObject, IDropTarget
    {

        // private LyncComm _comm;
        private LyncUSB _usb;
        private LyncInterface _lync;

        public LyncVM()
        {
            // Set the user's preferences from the pref file.
            
            _usb = new LyncUSB();
            _usb.UsbTransmitterChangeNotifier += _usb_UsbTransmitterChangeNotifier;

            _userStatus = new ObservableCollection<UserStatus>();
            // _userStatus.CollectionChanged += _userStatus_CollectionChanged;
            LoadPrefs();

            _lync = new LyncInterface();
            _lync.PropertyChanged += _lync_PropertyChanged;

            this.redLights = new LightCollection(LIGHTS.RED);
            this.yellowLights = new LightCollection(LIGHTS.YELLOW);
            this.greenLights = new LightCollection(LIGHTS.GREEN);
            this.offLights = new LightCollection(LIGHTS.OFF);

            foreach(UserStatus s in this.UserStatuses)
            {
                PutStatusInLightBucket(s);
            }

            // Prevent the xaml designer from grabbing the com port.
            DependencyObject dep = new DependencyObject();
            if (!DesignerProperties.GetIsInDesignMode(dep))
            {
                PresenceToLight(_lync.curPresence);
            }

        }

        private void PutStatusInLightBucket(UserStatus s)
        {
            switch (s.Light)
            {
                case LIGHTS.RED:
                    this.redLights.userStatuses.Add(s);
                    break;
                case LIGHTS.YELLOW:
                    this.yellowLights.userStatuses.Add(s);
                    break;
                case LIGHTS.GREEN:
                    this.greenLights.userStatuses.Add(s);
                    break;
                case LIGHTS.OFF:
                    this.offLights.userStatuses.Add(s);
                    break;
                case LIGHTS.STATUS:
                    this.offLights.userStatuses.Add(s);
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

            
            if (prefs != null && prefs.ComPort != null)
            {
                ComPort = prefs.ComPort;
            }
            else ComPort = "COM5";

            _comPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            
            
            return true;
        }

        private void SavePrefsExecute(object state)
        {
            // Save Prefs
            SerializePrefs prefs = new SerializePrefs();
            prefs.Statuses = UserStatuses;
            prefs.ComPort = ComPort;
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
            get
            {
                return _lync.isVideoOff;
                // return _isVideoOff; 
            }
            set
            {
                _isVideoOff = value;
                RaisePropertyChangedEvent("isVideoOff");
            }
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
