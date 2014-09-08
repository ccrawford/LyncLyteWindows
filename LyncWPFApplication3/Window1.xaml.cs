using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Lync.Model;
using System.Diagnostics;
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
        private LyncComm comm;


        public Window1()
        {
            InitializeComponent();


            vm = (LinkStatusVM)linkData.DataContext;

            InitializeComm();
            initializeLyncClient();
        }


        private void InitializeComm()
        {
            comm = new LyncComm(vm.ComPort);
            comm.CommStatusChanged +=comm_CommStatusChanged;
            comm.PortsChanged += comm_PortsChanged;
            ProcessComStatus(comm.CommunicationStatus);
        }

        void comm_PortsChanged(object sender, PortsChangedArgs e)
        {
            vm.comLinkStatus = "New port found";
            vm.ComPorts = new System.Collections.ObjectModel.ObservableCollection<string>(e.SerialPorts);
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
                    vm.comLinkStatus = "Connected";
                    break;
                case COM_STATUS.Disconnected:
                    vm.comLinkStatus = "Disconnected";
                    break;
                case COM_STATUS.BadPort:
                    vm.comLinkStatus = "Check Port selection";
                    break;
                case COM_STATUS.NotDetermined:
                    vm.comLinkStatus = "Checking...";
                    break;
                default:
                    vm.comLinkStatus = "Com Status unknown.";
                    break;
            }
        }

#region TestButtons
        
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
#endregion

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

        void InitializeClient()
        {
            if (_lyncClient == null)
                _lyncClient = Microsoft.Lync.Model.LyncClient.GetClient();

            _self = _lyncClient.Self;

            string currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            
            // Necessary to process manual contact state changes.
            _self.Contact.ContactInformationChanged += new EventHandler<ContactInformationChangedEventArgs>(Contact_InformationChanged);

            foreach(Conversation c in _lyncClient.ConversationManager.Conversations)
            {
                NewConversation(c);
            }

            // Watch conversations for mute feature
            _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;

            // Mute states have been set. Now you can update lights.
            updateLights(currentPresence);

        }

        void NewConversation(Conversation conversation)
        {
            // For each new conversation, check the current state and watch for changes.

            var m_audioVideo = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            var m_properties = m_audioVideo.Properties;

            if (m_properties[ModalityProperty.AVModalityAudioCaptureMute] != null)
            {
                vm.isMicMuted = (bool)m_properties[ModalityProperty.AVModalityAudioCaptureMute];
            }

            var videoState = m_audioVideo.VideoChannel.State;
            vm.isVideoOff = !(videoState == ChannelState.Send || videoState == ChannelState.SendReceive);
            
            // Watch for camera changes
            m_audioVideo.AVModalityPropertyChanged += m_audioVideo_AVModalityPropertyChanged;
            // Watch for mute changes
            m_audioVideo.VideoChannel.StateChanged += VideoChannel_StateChanged;

        }

        void VideoChannel_StateChanged(object sender, ChannelStateChangedEventArgs e)
        {
            // THIS IS A GOOD WAY TO CHECK IF THE CAMERA IS ON.
            // However, need to check all the video channels when one gets turned off, another may also be on?

            Debug.WriteLine("Video State Changed: " + e.NewState);
            
            // Check if sending video by looking to see if current state is one of the sending states.
            vm.isVideoOff = !(e.NewState == ChannelState.Send || e.NewState == ChannelState.SendReceive);

            // Get the presence...did we hang up? and send to the processor.
            string currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            updateLights(currentPresence);
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

            string currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            updateLights(currentPresence);
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
            comm.PortName = vm.ComPort;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            comm.CleanUp();
            comm.ActivateLight(LIGHTS.OFF);
        }


    }
        
    


}
