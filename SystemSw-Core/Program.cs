using System;
using System.Threading;

namespace SystemSw_Core
{
    class Program
    {

        static void Main(string[] args)
        {
            var ec = new Extron.ExtronCommunicator("/dev/ttyUSB0", false);
            Console.WriteLine("Press [ENTER] to open communication");
            Console.ReadLine();
            ec.OpenConnection(false);
            Console.WriteLine("Press [ENTER] to identify");
            Console.ReadLine();
            ec.Identify();

            Console.WriteLine($"Channels: {ec.Channels}");
            Console.WriteLine($"Video Channel: {ec.VideoChannel}");
            Console.WriteLine($"Audio Channel: {ec.AudioChannel}");

            Console.WriteLine("Press [ENTER] to begin the cycle check");
            Console.ReadLine();

            ec.ChangeChannel(1);
            Thread.Sleep(1500);
            ec.ChangeChannel(2);
            Thread.Sleep(1500);
            ec.ChangeChannel(3);
            Thread.Sleep(1500);
            ec.ChangeChannel(9);


        }

    }

}
