using LyncLights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsConsoleTestApp
{
    class Program
    {
       
        static void Main(string[] args)
        {
            LyncComm comm = new LyncComm("com5");

            comm.ActivateLight(LIGHTS.RED);
            comm.ActivateLight(LIGHTS.GREEN);
            Console.ReadLine();
        }
    }
}
