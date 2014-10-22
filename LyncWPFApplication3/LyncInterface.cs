using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System.Diagnostics;

namespace LyncWPFApplication3
{
    class LyncInterface : ObservableObject
    {
        private LyncClient _lyncClient;
        private Self _self;
        public string lyncStatus;

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

        private String _curPresence;
        public String curPresence
        {
            get { return _curPresence; }
            set
            {
                _curPresence = value;
                RaisePropertyChangedEvent("curPresence");
            }
        }




        public LyncInterface()
        {
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
                lyncStatus = "Lync is not running.";
                return;
            }

            if (_lyncClient != null)
            {

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

            // Necessary to process manual contact state changes.
            _self.Contact.ContactInformationChanged += new EventHandler<ContactInformationChangedEventArgs>(Contact_InformationChanged);
            isMicMuted = true;
            isVideoOff = true;
            // Check mute states in open conversations
            foreach (Conversation c in _lyncClient.ConversationManager.Conversations)
            {
                NewConversation(c);
            }

            // Watch for future conversations.
            _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;

            // Mute states have been set. Now you can update lights. Don't move this earlier.
            curPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();

        }

        // Returns true if broadcasting audio 
        bool IsAudioOn(Conversation conversation)
        {
            var m_audioVideo = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            var m_properties = m_audioVideo.Properties;

            // Check to see if that property exists before dereferencing it.
            if (m_properties.ContainsKey(ModalityProperty.AVModalityAudioCaptureMute) && m_properties[ModalityProperty.AVModalityAudioCaptureMute] != null)
            {
                // var foo = m_properties[ModalityProperty.AVModalityAudioCaptureMute];
                return !(bool)m_properties[ModalityProperty.AVModalityAudioCaptureMute];
            }

            else return false;
        }

        // Returns true if broadcasting Video
        bool IsVideoOn(Conversation conversation)
        {
            var m_audioVideo = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            var videoState = m_audioVideo.VideoChannel.State;

            return (videoState == ChannelState.Send || videoState == ChannelState.SendReceive);
        }

        // For new conversations, check the mute state and watch for changes.
        void NewConversation(Conversation conversation)
        {
            // For each new conversation, check the current state and watch for changes.
            var m_audioVideo = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            var m_properties = m_audioVideo.Properties;
            isMicMuted = (isMicMuted && !IsAudioOn(conversation));
            isVideoOff = (isVideoOff && !IsVideoOn(conversation));

            // Watch for camera changes
            m_audioVideo.AVModalityPropertyChanged += m_audioVideo_AVModalityPropertyChanged;
            // Watch for mute changes
            m_audioVideo.VideoChannel.StateChanged += VideoChannel_StateChanged;

        }

        // Camera state changes.
        void VideoChannel_StateChanged(object sender, ChannelStateChangedEventArgs e)
        {
            // THIS IS A GOOD WAY TO CHECK IF THE CAMERA IS ON.
            // However, need to check all the video channels when one gets turned off, another may also be on?

            Debug.WriteLine("Video State Changed: " + e.NewState);

            // Check if sending video by looking to see if current state is one of the sending states.
            isVideoOff = !(e.NewState == ChannelState.Send || e.NewState == ChannelState.SendReceive);

            // Get the presence...did we hang up? and send to the processor.
            // Check for null here
            if (_self != null && _self.Contact.GetContactInformation(ContactInformationType.Activity) != null)
                curPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
            else curPresence = "Unknown";
        }


        void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            NewConversation(e.Conversation);
        }

        //Audio mute changes
        void m_audioVideo_AVModalityPropertyChanged(object sender, ModalityPropertyChangedEventArgs e)
        {
            //THIS IS A GOOD WAY TO MONITOR USE OF THE MUTE BUTTON.
            // Dont forget to change the lights when this changes.

            if (e.Property == ModalityProperty.AVModalityAudioCaptureMute)
            {
                System.Diagnostics.Debug.WriteLine("avModProp: " + e.Property + " : " + e.Value);

                isMicMuted = (bool)e.Value;
                // Add null check here.
                if (_self != null && _self.Contact.GetContactInformation(ContactInformationType.Activity) != null)
                    curPresence = _self.Contact.GetContactInformation(ContactInformationType.Activity).ToString();
                else
                    curPresence = "Unknown";
            }
        }

        // Fires when the user's availability/presence changes.
        private void Contact_InformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            var contact = sender as Contact;

            if (e.ChangedContactInformation.Contains(ContactInformationType.Activity))
            {

                string cur_activity;
                // string cur_availability;

                if (_lyncClient.State == ClientState.SignedIn)
                {
                    // Activity is the description of the activity. Availability is the lync software color 
                    cur_activity = contact.GetContactInformation(ContactInformationType.Activity).ToString();
                    // cur_availability = contact.GetContactInformation(ContactInformationType.Availability).ToString();
                }
                else
                {
                    cur_activity = "Away";
                }

                curPresence = cur_activity;

            }
        }
    }
}
