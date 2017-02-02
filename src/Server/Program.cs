using System;
using Receiver.HL7;

namespace Receiver
{
    public class Receiver
    {

        #region Defaults

        private const int DefaultPort = 1250;

        #endregion
        

        private static void Main(string[] args)
        {
            var port = DefaultPort;

            if (args.Length == 1)
                int.TryParse(args[0], out port);

            Console.WriteLine($"Starting HL7 client on port {port}.");
            Console.WriteLine("Press Ctrl-c to exit.");

            var processor = new HL7MsgProcessor();

            var parser = new HL7MsgParser(processor);

            var client = new HL7Client(parser, port);

            client.StartListener();
        }

    }

}

