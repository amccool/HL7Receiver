using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using NHapi.Base.Model;
using NHapi.Base.Util;
using NHapi.Model.V24.Datatype;
using NHapi.Model.V24.Segment;
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
                    
                    var x = new Terser(hl7Message);
                    PID pid = (PID) x.Finder.findSegment("PID", 0);
                    OBX res = (OBX)x.Finder.findSegment("OBX", 0);
                    


                    ED ov = (ED)res.GetObservationValue(0).Data;

                    var file = Convert.FromBase64String(ov.Data.Value);
                    
                    using (Image image = Image.FromStream(new MemoryStream(file)))
                    {
                        image.Save("output.jpg", ImageFormat.Jpeg); 

                        Process.Start("output.jpg", "");

                    }
                    
                    Console.WriteLine("OvservationValue {0}.", pid.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof +" "+ pid.GetPatientName(0).GivenName +" "+pid.GetPatientName(0).FamilyName);
                    break;

                default:
                    errorMessage = "This message version is not supported.";
                    return false;
            }
            return true;
        }
    }
}