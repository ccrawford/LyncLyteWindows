using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dweet_CAC;

namespace TestDweet
{
    class Program
    {
        static void Main(string[] args)
        {
            var myContent = new Dictionary<string, string> { { "color", "red" } };
            var dweet = new Dweet_CAC.DweetNet("cac_status", new Dictionary<string, string>()  { { "color", "red" } } );
            dweet.DweetIt();
        }
    }
}
