using NHapi.Base.Model;

namespace Receiver.Interfaces
{
    public interface IHL7MsgProcessor
    {
        bool ProcessMessage(IMessage hl7Message, out string errorMessage);

    }
}