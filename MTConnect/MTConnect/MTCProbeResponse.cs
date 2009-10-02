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
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace MTConnect
{
    public class MTCDevice
    {
        public String name;
        public double sampleRate;
        public String uuid;
        public String iso841class;
        public String serialNumber;
        public String manufacturer;
        public MTCDataItem[] dataItems;
    };


    public class MTCProbeResponse : MTCResponse
    {
        // public members
        public MTCDevice[] devices;
        // constructor: call base constructor, and extract device info from header
        public MTCProbeResponse(String loadPath, MTCAgent agt)
            : base(loadPath, agt)
        {
            if (this.responseDoc != null && this.agent != null)
            {
                this.ParseProbe();
            }
        }
        public MTCProbeResponse(StringReader rawXmlReader, MTCAgent agt)
            : base(rawXmlReader, agt)
        {
            if (this.responseDoc != null && this.agent != null)
            {
                this.ParseProbe();
            }
        }
        private void ParseProbe()
        {
            MTCAgent agt = this.agent;
            if (agt.cachedProbe == null)
            {
                // cache probe() response for looking up units etc.
                agt.cachedProbe = this.responseDoc.CreateNavigator();
                XPathNodeIterator n = agt.cachedProbe.Select("//Header");
                n.MoveNext();
                agt.instanceId = n.Current.GetAttribute("instanceId", String.Empty);
            }
            XPathNavigator nv = this.responseDoc.CreateNavigator();
            XPathNodeIterator ni = nv.Select("//Device");
            ArrayList devs = new ArrayList(1);  // the common case
            while (ni.MoveNext())
            {
                MTCDevice d = new MTCDevice();
                d.name = ni.Current.GetAttribute("name", String.Empty);
                try
                {
                    d.sampleRate = Convert.ToDouble(ni.Current.GetAttribute("sampleRate", String.Empty));
                }
                catch
                {
                    d.sampleRate = Double.NaN;
                }
                d.uuid = ni.Current.GetAttribute("uuid", String.Empty);
                d.iso841class = ni.Current.GetAttribute("iso841Class", String.Empty);
                // get the DataItems that are descendants of this Device.
                d.dataItems = GetDataItems(ni);
                // Description element is a child of Device. Grab it to get serial# and mfr info.
                nv = ni.Current.CreateNavigator();
                XPathNodeIterator n2 = nv.Select("//Description");
                n2.MoveNext();
                d.serialNumber = n2.Current.GetAttribute("serialNumber", String.Empty);
                d.manufacturer = n2.Current.GetAttribute("manufacturer", String.Empty);
                devs.Add(d);
            }
            this.devices = (MTCDevice[])devs.ToArray(typeof(MTCDevice));
        }
    
        private static MTCDataItem[] GetDataItems(XPathNodeIterator deviceNode)
        {
            ArrayList r = new ArrayList();
            XPathNavigator n1 = deviceNode.Current.CreateNavigator();
            String e = String.Empty;
            XPathNodeIterator i1 = n1.SelectDescendants("DataItem", e, false);
            while (i1.MoveNext())
            {
                MTCDataItem d = new MTCDataItem();
                d.name = i1.Current.GetAttribute("name", e);
                d.type = i1.Current.GetAttribute("type", e);
                d.subType = i1.Current.GetAttribute("subType", e);
                d.category = i1.Current.GetAttribute("category", e);
                d.units = MTCEvent.GetUnits(i1.Current.GetAttribute("units", e));
                d.id = i1.Current.GetAttribute("id", e);
                d.path = String.Format("//DataItem[@id='{0}']", d.id);

                r.Add(d);
            }

            return (MTCDataItem[])r.ToArray(typeof(MTCDataItem));
        }

    }

}
