using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyncWPFApplication3
{
    class LyncComm
    {
        private SerialPort _port;
        private char QUERY_CHAR = '?';
        private char QUERY_RESPONSE_GOOD = '!';
        public enum LIGHTS
        {
            RED = 4,
            YELLOW = 5,
            GREEN = 2,
            OFF = 8,
            STATUS = 7
        }
        
        public LyncComm(string port)
        {
            _port = new SerialPort(port, 9600);
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;
        }

        public bool ActiveLight(LIGHTS light)
        {
            bool response = SendMessage(light.ToString());
            
            return true;
        }



        public bool CheckDeviceHandshake()
        {
            SendMessage(QUERY_CHAR.ToString());

            char response = ' ';

            try
            {
                while (_port.BytesToRead > 0 && response != QUERY_RESPONSE_GOOD)
                {
                    response = (char)_port.ReadChar();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return (response == QUERY_RESPONSE_GOOD);

        }

        private bool SendMessage(string message)
        {
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
