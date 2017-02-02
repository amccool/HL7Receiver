using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Receiver.Interfaces;

namespace Receiver.HL7
{
    public sealed class HL7Client : IHL7Client
    {

        private const int MLLP_START_CHARACTER = 0x0B; // HEX 0B
        private const int MLLP_FIRST_END_CHARACTER = 0x1C; // HEX 1C
        private const int MLLP_LAST_END_CHARACTER = 0x0D; // HEX 0D

        private bool _continueProcessing = true;

        private readonly TcpListener _listener;
        private readonly IHL7MsgParser _parser;

        public HL7Client(IHL7MsgParser parser, int port)
        {
            _parser = parser;
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void StartListener()
        {
            // Catch the break key press
            Console.CancelKeyPress += Console_CancelKeyPress;
            
            _listener.Start();
            while (_continueProcessing)
            {
                Console.WriteLine("Waiting for connection.");
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("Connection received.");


                HandleClient(client, _parser);
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Stopping...");
            _continueProcessing = false;
            _listener.Stop();
        }


        private static void HandleClient(TcpClient client, IHL7MsgParser parser)
        {
            string message = string.Empty;
            while (client.Connected)
            {
                // Read the next byte from the stream
                int b = client.GetStream().ReadByte();
                if (b == -1)
                {
                    // Client disconnected
                    client.Close();
                }
                // Start adding characters to the message
                // if the MLLP start character is received.
                if ((b == MLLP_START_CHARACTER) || (message.Length > 0))

                    message += (char)b;
                // Check if the message string ends with the two
                // MLLP end characters. If so, a complete HL7 message is
                // received.
                if ((message.Length > 3) && ((message[message.Length - 2] ==
                                              MLLP_FIRST_END_CHARACTER) && (message[message.Length - 1] ==
                                                                            MLLP_LAST_END_CHARACTER)))
                {
                    // String away the MLLP characters to keep the pure HL7 message
                    var hl7Message = message.Substring(1, message.Length - 3);
                    // Parse the HL7 message and get a response


                    var hl7Response = parser.ParseHL7Message(hl7Message);

                    // Add MLLP characters to the response message
                    var responseMessage = (char)MLLP_START_CHARACTER + hl7Response +
                    (char)MLLP_FIRST_END_CHARACTER + (char)MLLP_LAST_END_CHARACTER;
                    StreamWriter writer = new StreamWriter(client.GetStream());
                    writer.Write(responseMessage);
                    writer.Flush();
                }
            }
            Console.WriteLine("Connection closed.");
        }
    }
}
