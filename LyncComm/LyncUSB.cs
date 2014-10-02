using System.Management;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;
using DigiSparkDotNet;
using LibUsbDotNet.DeviceNotify;


namespace LyncLights
{
 
    public class LyncUSB
    {
 
        private char QUERY_CHAR = '?';
        private char QUERY_RESPONSE_GOOD = '!';
        private long KEEP_ALIVE_MS = 5 * 1000;
        private Timer keepAlive;

        private ArduinoUsbDevice digiSpark;
        private EventHandler<DeviceNotifyEventArgs> onDeviceChange;
        
        public LIGHTS CurrentLight = LIGHTS.OFF;
        public event EventHandler<EventArgs> UsbTransmitterChangeNotifier;

        
        //Constructor
        public LyncUSB()
        {

            digiSpark = new ArduinoUsbDevice();

            digiSpark.ArduinoUsbDeviceChangeNotifier += digiSpark_ArduinoUsbDeviceChangeNotifier;

            keepAlive = new Timer();
            keepAlive.AutoReset = true;
            keepAlive.Interval = KEEP_ALIVE_MS;
            keepAlive.Elapsed += keepAlive_Elapsed;

            CheckDeviceHandshake();

            keepAlive.Start();

        }

        void digiSpark_ArduinoUsbDeviceChangeNotifier(object sender, EventArgs e)
        {
            var args = (DeviceNotifyEventArgs)e;
            UsbTransmitterChangeNotifier.Invoke(sender, e);
            
        }


        public bool IsAvailable{
            get
            {
                if (digiSpark != null) return digiSpark.isAvailable;
                else return false;
            }
        }

        public void CleanUp()
        {
            // keepAlive.Stop();
            // keepAlive = null;
            // digiSpark.ArduinoUsbDeviceChangeNotifier -= digiSpark_ArduinoUsbDeviceChangeNotifier;
            // digiSpark = null;
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
                int total = 0;
                byte[] value;
                if (digiSpark.ReadByte(out value))
                {
                    total++;
                    Debug.Write(Encoding.Default.GetString(value));
                    response = Encoding.Default.GetString(value)[0];
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
            // return digiSpark.WriteBytes(GetBytes(message));
            return digiSpark.WriteByte(string.IsNullOrEmpty(message) ? (byte)0 : (byte)message[0]);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


    }



}
