using System;
using NHapi.Base.Model;
using NHapi.Base.Util;
using Receiver.HL7Tools;
using Receiver.Interfaces;


namespace Receiver.HL7
{
    public class HL7MsgProcessor : IHL7MsgProcessor
    {
        public bool ProcessMessage(IMessage hl7Message, out string errorMessage)
        {
            errorMessage = null;
            Console.WriteLine("A HL7 message of the type {0} and version {1} is received.",
                hl7Message.GetStructureName(), hl7Message.Version);
            //if (!hl7Message.GetStructureName().StartsWith("ADT_"))
            //{
            //    errorMessage = "This message structure is not supported.";
            //    return false;
            //}
            switch (hl7Message.Version)
            {
                case HL7Version.V23:
                    // Add code to handle the V2.3 of these ADT messages
                    NHapi.Model.V23.Segment.PID pid1 = (NHapi.Model.V23.Segment.PID)
                        hl7Message.GetStructure("PID");
                    Console.WriteLine("PatientID {0}.", pid1.GetAlternatePatientID(0).ID.Value);
                    break;
                case HL7Version.V24:
                    // Add code to handle the V2.4 of these ADT messages
                    NHapi.Model.V24.Segment.PID pid2 = (NHapi.Model.V24.Segment.PID)
                        hl7Message.GetStructure("PID");

                    Console.WriteLine("PatientID {0}.", pid2.PatientID.ID.Value);
                    break;
                case HL7Version.V26:
                    // Add code to handle the V2.6
                    var x = new Terser(hl7Message);

                    var res = x.Get("/.OBX-3-1");

                    //var obx = (NHapi.Model.V26.Segment.OBX) hl7Message.GetStructure("OBX");

                    Console.WriteLine("OvservationValue {0}.", res);
                    break;

                default:
                    errorMessage = "This message version is not supported.";
                    return false;
            }
            return true;
        }
    }
}