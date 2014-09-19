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

        public Window1()
        {
            InitializeComponent();
            vm = (LinkStatusVM)base.DataContext;
            // base.DataContext = vm;

            initializeLyncClient();
        }


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

            if (e.Property == ModalityProperty.AVModalityAudioCaptureMute)
            {
                System.Diagnostics.Debug.WriteLine("avModProp: " + e.Property + " : " + e.Value);

                vm.isMicMuted = (bool)e.Value;
                string currentPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
                updateLights(currentPresence);
            }
        }

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
            vm.PresenceToLight(presence);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.CleanUp();
        }

    }

}
