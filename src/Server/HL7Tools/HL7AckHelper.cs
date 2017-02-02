using System;
using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Util;

namespace Receiver.HL7Tools
{
    public static class HL7AckHelper
    {
        /// <summary>
        /// Create an Ack message based on a received message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        internal static IMessage CreateACK(IMessage message, string messageCode, string errorMessage)
        {
            IMessage result = null;

            // Using reflection to load the right NHapi assembly
            // and create a new instance of the ACK class from there
            var ackClassType = string.Format("NHapi.Model.V{0}.Message.ACK, NHapi.Model.V{0}",
                message.Version.Remove(message.Version.IndexOf('.'), 1));

            var x = Type.GetType(ackClassType);

            if (x != null)
                result = (IMessage)Activator.CreateInstance(x);
            
            // Fill the ACK message with the right values, the method from
            // paragraph 2.4 is used here
            MakeACK(message, messageCode, result, errorMessage);

            return result;
        }


        /// <summary>
        /// Create an Ack message based on a received message
        /// </summary>
        /// <param name="inboundMessage">received message</param>
        /// <param name="ackCode">Acknowledge code</param>
        /// <param name="ackMessage">Message to be created</param>
        /// <param name="errorMessage">Error message to send</param>
        internal static void MakeACK(IMessage inboundMessage, string ackCode, IMessage ackMessage, string errorMessage)
        {
            Terser t = new Terser(inboundMessage);
            ISegment headerSegment;
            try
            {
                headerSegment = t.getSegment("MSH");
            }
            catch (HL7Exception)
            {
                throw new HL7Exception("Need an MSH segment to create a response ACK");
            }
            MakeACK(headerSegment, ackCode, ackMessage, string.Empty);
        }

        /// <summary>
        /// Create an Ack message based on a received message
        /// </summary>
        /// <param name="headerSegment">received header segment</param>
        /// <param name="ackCode">Acknowledge code</param>
        /// <param name="ackMessage">Message to be created</param>
        /// <param name="errorMessage">Error message to send</param>
        private static void MakeACK(ISegment headerSegment, string ackCode, IMessage ackMessage, string errorMessage)
        {
            if (!headerSegment.GetStructureName().Equals("MSH"))
                throw new HL7Exception("Need an MSH segment to create a response ACK(got " + headerSegment.GetStructureName() + ")");
            
            // Find the HL7 version of the inbound message:
            string version;
            try
            {
                version = Terser.Get(headerSegment, 12, 0, 1, 1);
            }
            catch (HL7Exception)
            {
                // I'm not happy to proceed if we can't identify the inbound
                // message version.
                throw new HL7Exception("Failed to get valid HL7 version from inbound MSH - 12 - 1");
            }
            // Create a Terser instance for the outbound message (the ACK).
            var terser = new Terser(ackMessage);
            // Populate outbound MSH fields using data from inbound message
            var outHeader = terser.getSegment("MSH");

            DeepCopy.copy(headerSegment, outHeader);
            // Now set the message type, HL7 version number, acknowledgement code
            // and message control ID fields:


            var sendingApp = terser.Get("/MSH-3");
            var sendingEnv = terser.Get("/MSH-4");

            terser.Set("/MSH-3", "CommunicationName");
            terser.Set("/MSH-4", "EnvironmentIdentifier");
            terser.Set("/MSH-5", sendingApp);
            terser.Set("/MSH-6", sendingEnv);
            terser.Set("/MSH-7", DateTime.Now.ToString("yyyyMMddmmhh"));
            terser.Set("/MSH-9", "ACK");
            terser.Set("/MSH-12", version);
            terser.Set("/MSA-1", ackCode ?? "AA");
            terser.Set("/MSA-2", Terser.Get(headerSegment, 10, 0, 1, 1));

            // Set error message
            if (errorMessage != null)
                terser.Set("/ERR-7", errorMessage);
        }
    }
}
