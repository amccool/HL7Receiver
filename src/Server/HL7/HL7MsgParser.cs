using System;
using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using Receiver.HL7Tools;
using Receiver.Interfaces;

namespace Receiver.HL7
{
    public class HL7MsgParser : IHL7MsgParser
    {
        private readonly IHL7MsgProcessor _messageProcessor;


        public HL7MsgParser(IHL7MsgProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }
        public string ParseHL7Message(string message)
        {
            string result = string.Empty;
            ParserBase parser = new PipeParser();
            try
            {
                var hl7Message = parser.Parse(message);
                var messageCode = "AA";
                string errorMessage;

                if (!ProcessMessage(hl7Message, out errorMessage)) messageCode = "AE";
                
                // Create a response message
                var ackMessage = HL7AckHelper.CreateACK(hl7Message, messageCode, errorMessage);
                result = parser.Encode(ackMessage);
                
            }
            catch (HL7Exception ex)
            {
                Console.WriteLine($"Error while parsing: {ex.Message}");
            }
            return result;
        }
        
        public bool ProcessMessage(IMessage hl7Message, out string errorMessage)
        {
            return _messageProcessor.ProcessMessage(hl7Message, out errorMessage);
        }

    }
}
