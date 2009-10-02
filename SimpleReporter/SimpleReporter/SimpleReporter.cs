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
using System.Xml.XPath;
using System.IO;
using MTConnect;

// This example program collects X and Y measurements continuously and tries to
//  "pair them up" according to the timestamp.  This isn't always guaranteed to
//  work but it's close enough when using the public Testing Agent.  The TEsting
//  Agent is cutting a spiral pattern in the XY plane, so this program's goal is
//  to collect position measurements and export them to a CSV file that can be
//  opened in Excel, where plotting X vs Y for the two columns of values should
//  show the spiral pattern being done by the test program.

namespace SimpleReporter
{
    class Program
    {
        public static String agtUri = "http://agent.mtconnect.org/LinuxCNC";
        // how long to run for; set to about 60 secs for a full run of the test program,
        // which cuts a spiral in the XY plane
        public static Int32 totalSecs = 30;
        // where to put the CSV file.  WARNING: Vista users may not be able
        //   to write to \windows\temp and should choose a different directory.
        // Warning 2: every backslash in a filename should be represented as
        //   a double-backslash in the string!
        public const String filename = "C:\\windows\\temp\\out.csv";

        public static void Main(string[] args)
        {
            MTCAgent agt;
            DateTime startTime;
            TimeSpan totalSecsAsTime;
            MTCSampleResponse resp;


            agt = new MTCAgent(agtUri);
            MTCProbeResponse prb = agt.Probe();

            // set samplePath to the XPath expression of the data item(s) we want to collect.
            // in this case we're collecting pairs of X,Y measurements.
            String samplePath =
                "Device[@name='LinuxCNC']//Linear[@name='X']|Device[@name='LinuxCNC']//Linear[@name='Y']";

            totalSecsAsTime = new TimeSpan(0, 0, totalSecs);
            startTime = DateTime.Now;

            // open new file for writing
            TextWriter f = new StreamWriter(filename);
            Console.WriteLine("Collecting {0} seconds of data to file {1}...please wait",
                totalSecs, filename);

            while (DateTime.Now - startTime <= totalSecsAsTime)
            {
                resp = agt.Sample(samplePath, agt.nextSequence);
                if (!resp.isSuccess)
                {
                    Console.WriteLine("*** ERROR: " + resp.errors);
                    return;
                }
                String x = resp.ToString();
                // select all of the Position nodes from the response.  See the workshop slides
                //   to understand how this code works.
                XPathNavigator navY = resp.xmlNav.Clone();
                XPathNavigator navX = resp.xmlNav.Clone();
                XPathNodeIterator nx = navX.Select("//ComponentStream[@name='X']//m:Position", MTCResponse.nsManager);

                while (nx.MoveNext())
                {
                    String xValue, yValue, timeStamp, seqNum;
                    XPathNodeIterator findY;

                    // grab the Timestamp from the next X position measurement...
                    timeStamp = nx.Current.GetAttribute("timestamp", String.Empty);
                    // find the matching Y
                    findY = navY.Select(String.Format("//ComponentStream[@name='Y']//m:Position[@timestamp='{0}']", timeStamp), MTCResponse.nsManager);
                    findY.MoveNext();
                    seqNum = nx.Current.GetAttribute("sequence", String.Empty);
                    xValue = nx.Current.Value;
                    yValue = findY.Current.Value;
                    // now print out all values
                    f.WriteLine("{0},{1},{2},{3}", seqNum, timeStamp, xValue, yValue);
                }
            }
            f.Close();
            Console.WriteLine("Complete, press return to finish", filename);
            Console.ReadLine();
        }

    }

}
