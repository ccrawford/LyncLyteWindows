using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LyncWPFApplication3
{

    public class Serializer
    {
        static string PrefsFileName = "LyncLightPref.dat";

        public Serializer()
        {
        }

        public bool SerializeObject(SerializePrefs objectToSerialize)
        {
            return SerializeObject(Serializer.PrefsFileName, objectToSerialize);
        }
        
        public bool SerializeObject(string filename, SerializePrefs objectToSerialize)
        {
            try
            {
                Stream stream = File.Open(filename, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                stream.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public SerializePrefs DeSerializeObject()
        {
            return DeSerializeObject(Serializer.PrefsFileName);
        }

        public SerializePrefs DeSerializeObject(string filename)
        {
            SerializePrefs objectToSerialize;
            try
            {
                if (File.Exists(filename))
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    objectToSerialize = (SerializePrefs)bFormatter.Deserialize(stream);
                    stream.Close();
                }
                else objectToSerialize = null;
            }
            catch
            {
                objectToSerialize = null;
            }
            return objectToSerialize;
        }
    }
}