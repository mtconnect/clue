// Copyright (c) 2008, AMT - The Association For Manufacturing
// Technology ("AMT") All rights reserved.
// 
// Redistribution and use in source and binary forms, with or
// without modification, are permitted provided that the
// following conditions are met:
// 
//     * Redistributions of source code must retain the above
//       copyright notice, this list of conditions and the
//       following disclaimer.
// 
//     * Redistributions in binary form must reproduce the
//       above copyright notice, this list of conditions and the
//       following disclaimer in the documentation and/or other
//       materials provided with the distribution.
// 
//     * Neither the name of the AMT nor the names of its
//       contributors may be used to endorse or promote products
//       derived from this software without specific prior
//       written permission.
// 
// DISCLAIMER OF WARRANTY. ALL MTCONNECT MATERIALS AND
// SPECIFICATIONS PROVIDED BY AMT, MTCONNECT OR ANY PARTICIPANT
// TO YOU OR ANY PARTY ARE PROVIDED "AS IS" AND WITHOUT ANY
// WARRANTY OF ANY KIND. AMT, MTCONNECT, AND EACH OF THEIR
// RESPECTIVE MEMBERS, OFFICERS, DIRECTORS, AFFILIATES,
// SPONSORS, AND AGENTS (COLLECTIVELY, THE "AMT PARTIES") AND
// PARTICIPANTS MAKE NO REPRESENTATION OR WARRANTY OF ANY KIND
// WHATSOEVER RELATING TO THESE MATERIALS, INCLUDING, WITHOUT
// LIMITATION, ANY EXPRESS OR IMPLIED WARRANTY OF
// NONINFRINGEMENT, MERCHANTABILITY, OR FITNESS FOR A
// PARTICULAR PURPOSE.
// 
// LIMITATION OF LIABILITY. IN NO EVENT SHALL AMT, MTCONNECT,
// ANY OTHER AMT PARTY, OR ANY PARTICIPANT BE LIABLE FOR THE
// COST OF PROCURING SUBSTITUTE GOODS OR SERVICES, LOST
// PROFITS, LOSS OF USE, LOSS OF DATA OR ANY INCIDENTAL,
// CONSEQUENTIAL, INDIRECT, SPECIAL OR PUNITIVE DAMAGES OR
// OTHER DIRECT DAMAGES, WHETHER UNDER CONTRACT, TORT, WARRANTY
// OR OTHERWISE, ARISING IN ANY WAY OUT OF THIS AGREEMENT, USE
// OR INABILITY TO USE MTCONNECT MATERIALS, WHETHER OR NOT SUCH
// PARTY HAD ADVANCE NOTICE OF THE POSSIBILITY OF SUCH DAMAGES.



using System;
using MTConnect;


namespace SimpleBrowser
{
    class Program
    {
        public static void Main(String[] args)
        {
            String agentUrl = "http://agent.mtconnect.org/LinuxCNC"; // the Test Agent
            MTCSampleResponse resp;   // to hold responses from the Agent
            String input;

            // create Agent object used to communicate with the Agent
            MTCAgent ag = new MTCAgent(agentUrl);

            // first do a Probe and print some interesting info about the device
            MTCProbeResponse prb = ag.Probe();
            if (!prb.isSuccess)
            {
                Console.WriteLine("Probe failed: " + prb.errors);
                return;
            }

            Console.WriteLine("MTConnect devices attached to this Agent:");
            foreach (MTCDevice d in prb.devices)
            {
                Console.WriteLine("{0} {1} s/n#{2}, UUID={3}, sample rate={4:F}",
                    d.manufacturer, d.name, d.serialNumber,
                    d.uuid, d.sampleRate);
            }
            // The component name and type we are interested in:
            String name = "Y";
            String comp = "Linear";
            // Continuously get next sample until user exits.
            Console.WriteLine("Hit return for next sample or x to exit");
            ag.nextSequence = 3;
            while (true)
            {
                resp = ag.Current();    // get a set of samples
                if (!resp.isSuccess)
                {
                    Console.WriteLine("Error: " + resp.errors);
                    break;
                }
                Console.WriteLine("Getting next sample {0}", ag.nextSequence);
                MTCEvent[] e = resp.FindByComponentAndName(comp, name);
                Console.WriteLine("Found {0} events", e.Length);
                foreach (MTCEvent ev in e)
                {
                    Console.WriteLine("{0} {1} {2:D8} {3:-9}{4:9} {5:hh:mm:ss.ff} {6:9} {7}",
                        ev.componentType,
                        ev.componentName,
                        ev.sequence,
                        ev.valueType.ToString(),
                        (ev.valueSubtype == MTCEvent.ValueSubtype.UNKNOWN ? "" :
                         "[" + ev.valueSubtype.ToString() + "]"),
                        ev.timestamp.ToLocalTime(),
                        ev.value,
                        ev.units.ToString());
                    if (ev.hasParseErrors)
                        Console.WriteLine("  (Warning: " + ev.parseErrors);
                }
                input = Console.ReadLine();
                if (input.StartsWith("x")) { break; }
            }

        }
    }
}

