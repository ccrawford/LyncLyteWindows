using System.Management;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

namespace LyncLights
{
    public enum LIGHTS
    {
        RED = 5,
        YELLOW = 6,
        GREEN = 7,
        OFF = 4,
        STATUS = 8
    }
    public class LyncComm
    {
        private SerialPort _port;
        private static string[] _serialPorts;
        private static ManagementEventWatcher arrival;
        private static ManagementEventWatcher removal;

        private char QUERY_CHAR = '?';
        private char QUERY_RESPONSE_GOOD = '!';
        private long KEEP_ALIVE_MS = 5 * 1000;
        private Timer keepAlive;

        public LIGHTS CurrentLight = LIGHTS.OFF;
        private COM_STATUS _communicationStatus;

        ManagementEventWatcher watcher;


        public COM_STATUS CommunicationStatus
        {
            get { return _communicationStatus; }
            set
            {
                Debug.WriteLine("Comm status: " + value);
                if (value != _communicationStatus)
                {
                    _communicationStatus = value;
                    OnCommStatusChanged(new CommStatusChanagedEventArgs(value));
                    RaisePortsChangedIfNecessary(EventType.Removal);
                }
                return;
            }
        }

        //Constructor
        public LyncComm(string port)
        {

            // See if the port requested even exists in the system.
            // CheckPortAvail(port);

            keepAlive = new Timer();
            keepAlive.AutoReset = true;
            keepAlive.Interval = KEEP_ALIVE_MS;
            keepAlive.Elapsed += keepAlive_Elapsed;

            _serialPorts = SerialPort.GetPortNames();
            MonitorDeviceChanges();

            _port = new SerialPort(port, 9600);
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;

            CheckDeviceHandshake();

            /*
            var query = new WqlEventQuery("Select * from Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            watcher = new ManagementEventWatcher();
            watcher.EventArrived += watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();
            */

            keepAlive.Start();

        }

        private void MonitorDeviceChanges()
        {
            try
            {
                var deviceArrivalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                var deviceRemovalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");

                arrival = new ManagementEventWatcher(deviceArrivalQuery);
                removal = new ManagementEventWatcher(deviceRemovalQuery);

                arrival.EventArrived += (o, args) => RaisePortsChangedIfNecessary(EventType.Insertion);
                removal.EventArrived += (sender, eventArgs) => RaisePortsChangedIfNecessary(EventType.Removal);

                arrival.Start();
                removal.Start();
            }
            catch (Exception)
            {
                // Eat it.
            }
        }


        public event EventHandler<PortsChangedArgs> PortsChanged;
        private void RaisePortsChangedIfNecessary(EventType eventType)
        {
            lock (_serialPorts)
            {
                var availableSerialPorts = SerialPort.GetPortNames();
                if (!_serialPorts.SequenceEqual(availableSerialPorts))
                {
                    _serialPorts = availableSerialPorts;
                    OnPortsChanged(new PortsChangedArgs(eventType, _serialPorts));
                }
            }
        }

        protected virtual void OnPortsChanged(PortsChangedArgs e)
        {
            EventHandler<PortsChangedArgs> handler = PortsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        public static void CleanUp()
        {
            arrival.Stop();
            removal.Stop();
        }



        void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Debug.WriteLine("Device Plugged...");
            foreach (var p in e.NewEvent.Properties)
            {
                Debug.WriteLine("Event arrived {0} : {1}", p.Name, p.Value);
            }
        }



        public event CommStatusChangedEventHandler CommStatusChanged;
        public delegate void CommStatusChangedEventHandler(object sender, CommStatusChanagedEventArgs e);

        protected virtual void OnCommStatusChanged(CommStatusChanagedEventArgs e)
        {
            CommStatusChangedEventHandler handler = CommStatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }

        }

        // Just checks to see if the requested port is in the list of available ports.
        public bool CheckPortAvail(string port)
        {
            return SerialPort.GetPortNames().Contains(port);
        }

        void keepAlive_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckDeviceHandshake();
        }

        public bool ActivateLight(LIGHTS light)
        {
            Debug.WriteLine("ActivateLight: " + light);
            if (light != CurrentLight)
            {
                if (SendMessage(((int)light).ToString())) CurrentLight = light;
                else return false;
            }
            return true;
        }

        public bool CheckDeviceHandshake()
        {

            if (!SendMessage(QUERY_CHAR.ToString())) return false;
            // Cheesy, but the 1/10th of a second wait gives the connection time to spool up.
            System.Threading.Thread.Sleep(100);
            char response = ' ';

            try
            {
                // Debug.WriteLine("Looking for response");

                while (_port.BytesToRead > 0 && response != QUERY_RESPONSE_GOOD)
                {
                    //  Debug.WriteLine("Reading response");
                    response = (char)_port.ReadChar();
                }
            }
            catch (Exception)
            {
                CommunicationStatus = COM_STATUS.BadPort;
            }

            if (response == QUERY_RESPONSE_GOOD)
            {
                CommunicationStatus = COM_STATUS.Connected;
                return true;
            }
            else
            {
                CommunicationStatus = COM_STATUS.Disconnected;
                return false;
            }
        }

        private bool SendMessage(string message)
        {
            // If the port isn't open, make sure it's available.
            if (!_port.IsOpen && !CheckPortAvail(_port.PortName))
            {
                CommunicationStatus = COM_STATUS.BadPort;
                return false;
            }

            System.Diagnostics.Debug.WriteLine("SendMessage " + message);
            try
            {
                if (!_port.IsOpen) _port.Open();
                _port.Write(message);
            }
            catch (Exception)
            {
                CommunicationStatus = COM_STATUS.Disconnected;
                return false;
            }
            return true;
        }




    }

    public class PortsChangedArgs : EventArgs
    {
        private readonly EventType _eventType;
        private readonly string[] _serialPorts;

        public PortsChangedArgs(EventType eventType, string[] serialPorts)
        {
            _eventType = eventType;
            _serialPorts = serialPorts;
        }

        public string[] SerialPorts
        {
            get { return _serialPorts; }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

    }

    
        public enum EventType
        {
            Insertion,
            Removal
        }


    public class CommStatusChanagedEventArgs : EventArgs
    {
        public COM_STATUS NewStatus { get; set; }
        public CommStatusChanagedEventArgs(COM_STATUS status)
        {
            NewStatus = status;
        }
    }

    public enum COM_STATUS
    {
        Connected,
        Disconnected,
        BadPort,
        NotDetermined
    }

}
