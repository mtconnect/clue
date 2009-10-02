using System;
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


using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace MTConnect
{
    public struct MTCDataItem
    {
        public String name;
        public String type;
        public String subType;
        public String category;
        public String id;
        public MTCEvent.Units units;
        public String path;
    }

    public class MTCSampleResponse : MTCResponse
    {
        /// <summary>
        /// MTCSampleResponse encapsulates the functionality for getting and parsing the XML document
        /// returned by a Current or Sample request to an Agent.
        /// Default constructor (which should not be called explicitly under
        /// normal circumstances) just sets up the instance variables to indicate
        /// we haven't got a valid XML response doc yet.
        /// </summary>
        public MTCSampleResponse(String loadPath, MTCAgent agt)
            : base(loadPath, agt)
        {
            this.ParseSampleHeaders();
        }
        public MTCSampleResponse(StringReader rawXmlReader, MTCAgent agt)
            : base(rawXmlReader, agt)
        {
            this.ParseSampleHeaders();
        }
        private void ParseSampleHeaders()
        {
            if (xmlNav is XPathNavigator)
            {
                // update nextSequence info from Header node
                XPathNodeIterator n2 = xmlNav.Select("//Header");
                n2.MoveNext();
                try
                {
                    this.agent.nextSequence =
                        Convert.ToInt32(n2.Current.GetAttribute("nextSequence", String.Empty));
                }
                catch
                {
                    this.agent.nextSequence = 0;
                }
            }
            else
            {
                this.isSuccess = false;
                this.errors += "XML navigator not initialized";
            }
        }
        public MTCEvent[] FindByXpath(String xPathPredicate)
        {
            return FindByComponentAndName(String.Empty, String.Empty, xPathPredicate);
        }
        public MTCEvent[] FindByComponentAndName(String component, String name)
        {
            return FindByComponentAndName(component, name, null);
        }
        public MTCEvent[] FindByComponentId(String id)
        {
            return FindByComponentAndName(String.Empty, String.Empty, 
                                            String.Format("//ComponentStream[@componentId=\"{0}\"]", id));
        }
        public MTCEvent[] FindByComponentAndName(String component, String name, String xPathPredicate)
        {
            ArrayList evts = new ArrayList();
            String thePath;
            XPathNodeIterator componentStreams;

            // if the response object doesn't have a valid navigator, don't even try.
            if (!(xmlNav is XPathNavigator))
            {
                return new MTCEvent[0];
            }
            // Constrain path selection via component or name ONLY if nonempty are given for them.
            if (xPathPredicate == String.Empty || xPathPredicate == null)
            {
                thePath = String.Format("//ComponentStream{0}{1}",
                    (component != null && component.Length > 0 ? String.Format("[@component=\"{0}\"]", component) : String.Empty),
                    (name != null && name.Length > 0 ? String.Format("[@name=\"{0}\"]", name) : String.Empty));
            }
            else
            {
                thePath = xPathPredicate;
            }
            componentStreams = this.xmlNav.Select(thePath, MTCResponse.nsManager);

            // the node iterator returned a bunch of <ComponentStream> nodes.  each one will have either a Samples or Events node
            // as its child, and that node will have zero(?) or more children each of which is one of the defined Event Types 
            // or Sample Types.  Walk each of these and construct the corresponding MTCEvent structure.

            while (componentStreams.MoveNext())
            {
                XPathNavigator samplesNav = componentStreams.Current.Clone();
                bool ignoreCase = true;
                samplesNav.MoveToFirstChild();  // a Samples or Events node
                if (samplesNav.NodeType != XPathNodeType.Element
                    || (String.Compare(samplesNav.Name, "Events", ignoreCase) != 0
                        && String.Compare(samplesNav.Name, "Samples", ignoreCase) != 0))
                {

                    throw new Exception("Not a Sample or Events node!");
                }
                XPathNodeIterator samples = samplesNav.SelectChildren(XPathNodeType.Element);
                while (samples.MoveNext())
                {
                    MTCEvent e = new MTCEvent();
                    e.ExtractEvent(samplesNav.Name,
                        samples.Current, componentStreams.Current, this.agent.cachedProbe);
                    evts.Add(e);
                }
            }
            return (MTCEvent[])evts.ToArray(typeof(MTCEvent));
        }
    }
}
