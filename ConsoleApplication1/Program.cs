using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thingspeak_CAC;

namespace TestThingSpeak
{
    class Program
    {
        static void Main(string[] args)
        {
            Thingspeak ts = new Thingspeak("21935", new Dictionary<int, string>() { { 1, "ORANGE" } }, "VB14GGYFTCE8VES0");
            System.Console.WriteLine( ts.ThingIt().ToString());
            // System.Console.ReadLine(); 

            var ts2 = new Thingspeak("1", new Dictionary<int, string>() { { 1, "ORANGE" } }, "CJQBL03588OYTUJQ");
            ts2.BaseURL = "http://cacspeak.cloudapp.net";
            System.Console.WriteLine(ts2.ThingIt().ToString());
            System.Console.ReadLine();
        }
    }
}
