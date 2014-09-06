
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
        private string[] _ports;
        private char QUERY_CHAR = '?';
        private char QUERY_RESPONSE_GOOD = '!';
        private long KEEP_ALIVE_MS = 1500 * 1000;
        private Timer keepAlive;
        private bool _linkStatus;
        private bool _noLink = false;
        public LIGHTS CurrentLight = LIGHTS.OFF;

        public bool LinkSstatus
        {
            get { return _linkStatus; }
            set { return; }
        }
        
        public LyncComm(string port)
        {

            CheckPortAvail(port);


            keepAlive = new Timer();
            keepAlive.AutoReset = true;
            keepAlive.Interval = KEEP_ALIVE_MS;
            keepAlive.Elapsed += keepAlive_Elapsed;
            
            _port = new SerialPort(port, 9600);
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;

            CheckDeviceHandshake();

            keepAlive.Start();
            
        }

        public bool CheckPortAvail(string port)
        {
            _ports = SerialPort.GetPortNames();
            if (!_ports.Contains(port)) { _noLink = true; _linkStatus = false; }

            return _noLink;
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
            if (CheckPortAvail(_port.PortName)) return false;

            SendMessage(QUERY_CHAR.ToString());

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
                _linkStatus = false;
            }

            _linkStatus = (response == QUERY_RESPONSE_GOOD);

            return _linkStatus;

        }

        private bool SendMessage(string message)
        {
            if (_noLink) return false;

            System.Diagnostics.Debug.WriteLine("Sending " + message);
            try
            {
                if (!_port.IsOpen) _port.Open();
                _port.Write(message);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
