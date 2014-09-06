using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Lync.Model;
using System.Diagnostics;
using System.IO.Ports;
using System.Timers;
using System.Windows.Threading;
using LyncLights;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace LyncWPFApplication3
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        LinkStatusVM vm;

        private LyncClient _lyncClient;
        private Self _self;
        private string _currentPresence;
        private DispatcherTimer _keepAlivetimer;
        private LyncComm comm;


        public Window1()
        {
            InitializeComponent();
            vm = (LinkStatusVM)linkData.DataContext;

            LoadPrefs();
            InitializeComm();
            initializeLyncClient();
            intializeTimer();
        }

        private bool LoadPrefs()
        {
            
            Serializer serializer = new Serializer();
            SerializePrefs prefs = new SerializePrefs();

            prefs = serializer.DeSerializeObject();
            if (prefs != null && prefs.Statuses != null)
            {
                vm.UserStatuses = prefs.Statuses;
            }
            else vm.CreateDefaultStatuses();

            if (prefs != null && prefs.ComPort != null)
            {
                vm.ComPort = prefs.ComPort;
            }
            else vm.ComPort = "COM5";

            return true;
        }

        private void intializeTimer()
        {
            _keepAlivetimer = new DispatcherTimer();
            _keepAlivetimer.Interval = TimeSpan.FromSeconds(20);
            _keepAlivetimer.Start();
            _keepAlivetimer.Tick += _keepAlivetimer_Tick;
        }

        void _keepAlivetimer_Tick(object sender, EventArgs e)
        {
            comm.CheckDeviceHandshake();
            CheckKeepAlive();
            
            _keepAlivetimer.Start();
        }

        private void CheckKeepAlive()
        {
            //FIX this is a crappy way to do this. Should listen to an event instead.
            if (comm.LinkSstatus)
            {
                vm.comLinkStatus = "Connected";
            }
            else
            {
                vm.comLinkStatus = "No connection";
                this.Icon = new BitmapImage(new Uri("pack://application:,,,/Icons/x.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void InitializeComm()
        {
            comm = new LyncComm(vm.ComPort);
            comm.CheckDeviceHandshake();
            CheckKeepAlive();
        }

        private void Button_Click_RED(object sender, RoutedEventArgs e)
        {
            //Test button.
            comm.ActivateLight(LIGHTS.RED);
        }
        private void Button_Click_YELLOW(object sender, RoutedEventArgs e)
        {
            //Test button.
            comm.ActivateLight(LIGHTS.YELLOW);
        }
        private void Button_Click_GREEN(object sender, RoutedEventArgs e)
        {
            //Test button.
            comm.ActivateLight(LIGHTS.GREEN);
            // vm.UserStatuses.Add(new UserStatus { StatusName = "On Active Call", LyncStatus = "In Call", Light = LIGHTS.RED, AudioMuted = false, VideoMuted = false });
        }
        private void Button_Click_OFF(object sender, RoutedEventArgs e)
        {
            //Test button.
            comm.ActivateLight(LIGHTS.OFF);
        }

        //http://blogs.claritycon.com/blog/2011/04/lync-api-object-life-cycle/
        private void initializeLyncClient()
        {
            try
            {
                _lyncClient = Microsoft.Lync.Model.LyncClient.GetClient();
            }
            catch
            {
                MessageBox.Show(
                    "Microsoft Lync is not running.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }

            if (_lyncClient != null)
            {
  /*              _lyncClient.StateChanged +=
                    new EventHandler<ClientStateChangedEventArgs>
                        (LyncClient_StateChanged);
                */

                if (_lyncClient.State != ClientState.SignedIn)
                {

                    _lyncClient.BeginSignIn(
                        null,
                        null,
                        null,
                        result =>
                        {
                            if (result.IsCompleted)
                            {
                                _lyncClient.EndSignIn(result);

                                InitializeClient(); // Setup application logic
                            }
                        },
                        "Local user signing in" as object);
                }
                else
                {
                    // Set up Self object
                    InitializeClient();
                }
            }
        }

/*        private void LyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            Debug.WriteLine("State Changed: " + e.NewState.ToString());
        }
*/
        void InitializeClient()
        {
            if (_lyncClient == null)
                _lyncClient = Microsoft.Lync.Model.LyncClient.GetClient();

            _self = _lyncClient.Self;

            var foo = _lyncClient.DeviceManager.ActiveAudioDevice;
            

            _currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            
            _self.Contact.ContactInformationChanged += new EventHandler<ContactInformationChangedEventArgs>(Contact_InformationChanged);

            foreach(Conversation c in _lyncClient.ConversationManager.Conversations)
            {
                NewConversation(c);
            }
            // Watch conversations for mute feature
            _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;

            // Mute states have been set. Now you can update lights.
            updateLights(_currentPresence);

        }

        void NewConversation(Conversation conversation)
        {
            var m_audioVideo = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            var m_properties = m_audioVideo.Properties;

            if (m_properties[ModalityProperty.AVModalityAudioCaptureMute] != null)
            {
                vm.isMicMuted = (bool)m_properties[ModalityProperty.AVModalityAudioCaptureMute];
            }

            var videoState = m_audioVideo.VideoChannel.State;
            vm.isVideoOff = !(videoState == ChannelState.Send || videoState == ChannelState.SendReceive);
            
            m_audioVideo.AVModalityPropertyChanged += m_audioVideo_AVModalityPropertyChanged;
            m_audioVideo.VideoChannel.StateChanged += VideoChannel_StateChanged;

            // string videoMuted = 
            // var properties = m_properties.Keys;
        }

        void VideoChannel_StateChanged(object sender, ChannelStateChangedEventArgs e)
        {
            // THIS IS A GOOD WAY TO CHECK IF THE CAMERA IS ON
            Debug.WriteLine("Video State Changed: " + e.NewState);
            vm.isVideoOff = !(e.NewState == ChannelState.Send || e.NewState == ChannelState.SendReceive);

            _currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            updateLights(_currentPresence);
        }


        void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            NewConversation(e.Conversation);
        }

        void m_audioVideo_AVModalityPropertyChanged(object sender, ModalityPropertyChangedEventArgs e)
        {
            //THIS IS A GOOD WAY TO MONITOR USE OF THE MUTE BUTTON.
            // Dont forget to change the lights when this changes.

            System.Diagnostics.Debug.WriteLine("avModProp: " + e.Property + " : " + e.Value);
            if(e.Property == ModalityProperty.AVModalityAudioCaptureMute)
            {
                vm.isMicMuted = (bool)e.Value;
            }

            _currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            updateLights(_currentPresence);
        }

      /*  private void ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {

            Modality avM = (Modality)sender;
            var properties = avM.Properties;
            string bMuted = avM.Properties[ModalityProperty.AVModalityAudioCaptureMute].ToString();
            System.Diagnostics.Debug.WriteLine("Modality State Change. Muted: " + bMuted);

        } 
        */

        private void Contact_InformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine("Contact info changed");
            var contact = sender as Contact;

                if (e.ChangedContactInformation.Contains(ContactInformationType.Activity))
                {
                    // TODO hold presence and don't update if not necessary
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        string cur_activity;
                        string cur_availability;
                        ContactAvailability av;

                        if (_lyncClient.State == ClientState.SignedIn)
                        {
                            cur_activity = contact.GetContactInformation(ContactInformationType.Activity).ToString();
                            cur_availability = contact.GetContactInformation(ContactInformationType.Availability).ToString();
                        }
                        else
                        { 
                            // cur_activity = _lyncClient.State.ToString(); 
                            cur_activity = "Away";
                            cur_availability = "Away";
                        }

                        updateLights(cur_activity);
                    }));

                }
        }

        private void updateLights(string presence)
        {
            Debug.WriteLine("UpdateLights: " + presence);
            var user_status = vm.UserStatuses.Where(s => s.LyncStatus == presence).FirstOrDefault();
            var user_statuses = vm.UserStatuses.Where(s => s.LyncStatus == presence);
            if (user_statuses.Count() > 1)
            {
                //Check for muting condition
                user_status = user_statuses.Where(s => s.AudioMuted == vm.isMicMuted && s.VideoMuted == vm.isVideoOff).FirstOrDefault();
            }


            if (user_status != null)
            {
                this.Icon = lightIcon[user_status.Light];
                comm.ActivateLight(user_status.Light);
                vm.currentLight = user_status.Light;
            }
            else
            {
                vm.UserStatuses.Add(new UserStatus { LyncStatus = presence, StatusName = presence, Light= LIGHTS.OFF, AudioMuted=false, VideoMuted=false });
            }

        }

        // Used to set the icon to the current light value.
        private Dictionary<LIGHTS, BitmapImage> lightIcon = new Dictionary<LIGHTS, BitmapImage> 
        {
            { LIGHTS.RED, new BitmapImage(new Uri("pack://application:,,,/Icons/red.png", UriKind.RelativeOrAbsolute))},
            { LIGHTS.YELLOW, new BitmapImage(new Uri("pack://application:,,,/Icons/yellow.png", UriKind.RelativeOrAbsolute))},
            { LIGHTS.GREEN, new BitmapImage(new Uri("pack://application:,,,/Icons/green.png", UriKind.RelativeOrAbsolute))},
            { LIGHTS.OFF, new BitmapImage(new Uri("pack://application:,,,/Icons/off.png", UriKind.RelativeOrAbsolute))}
        };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the window's view model.
            System.Windows.Data.CollectionViewSource linkStatusVMViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("linkStatusVMViewSource")));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SavePrefs();
        }

        private void SavePrefs()
        {
            // Save Prefs
            SerializePrefs prefs = new SerializePrefs();
            prefs.Statuses = vm.UserStatuses;
            prefs.ComPort = vm.ComPort;
            Serializer serializer = new Serializer();
            serializer.SerializeObject(prefs);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SavePrefs();
            comm.ActivateLight(LIGHTS.OFF);
        }


    }
        
    


}
