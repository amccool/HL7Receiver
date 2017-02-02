using NHapi.Base.Model;

namespace Receiver.Interfaces
{
    public interface IHL7MsgParser
    {
        string ParseHL7Message(string message);
        bool ProcessMessage(IMessage hl7Message, out string errorMessage);
    }
}