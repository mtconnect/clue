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
using System.Xml;
using System.Xml.XPath;

namespace MTConnect
{
    public class MTCResponse
    {
        public XPathNavigator xmlNav;
        public XPathDocument responseDoc;
        public MTCAgent agent;  // agent from which we got this response
        // the following are parsed from the header
        public String sender;
        public DateTime creationTime;
        public Int64 bufferSize;
        public String instanceId;
        // indicate whether successful response received from Agent
        public bool isSuccess;
        public String errors;
        public static String nsUri;
        public static XmlNamespaceManager nsManager;
        static MTCResponse()
        {
            nsUri = "";
            nsManager = null;
        }
        public MTCResponse(StringReader xmlStringReader, MTCAgent agt)
        {
            this.agent = agt;
            this.errors = String.Empty;
            try
            {
                this.responseDoc = new XPathDocument(xmlStringReader);
            }
            catch (Exception ex)
            {
                this.responseDoc = null;
                this.isSuccess = false;
                this.errors += ex.Message;
            }
            this.ParseHeader();
        }
        public MTCResponse(String loadPath, MTCAgent agt)
        {
            this.agent = agt;
            this.errors = String.Empty;
            try
            {
                this.responseDoc = new XPathDocument(loadPath);
            }
            catch (Exception ex)
            {
                this.isSuccess = false;
                this.errors += ex.Message;
                return;
            }
            this.ParseHeader();
        }
        private void ParseHeader() 
        {
            String s;
            XPathNodeIterator n1;

            this.xmlNav = this.responseDoc.CreateNavigator();
            // if this is an MTConnectError document, something's wrong. Extract the
            // error message and error code, and set the failure flag.
            n1 = xmlNav.Select("//MTConnectError");
            if (n1.MoveNext()) {
                // error document
                this.isSuccess = false;
                n1 = xmlNav.Select("//Error");
                while (n1.MoveNext()) {
                    String errorCode = n1.Current.GetAttribute("errorCode", String.Empty);
                    if (errorCode != String.Empty) {
                        this.errors += String.Format("{0}: {1}\n", errorCode, n1.Current.Value);
                    } else {
                        this.errors += n1.Current.Value;
                    }
                }
                // no point in going further, since an MTConnectError document doesn't
                // have any other info in it besides the error info.
                return;
            }
            n1 = xmlNav.Select("//Header");
            n1.MoveNext();
            // parse header attributes
            String e = String.Empty;
            this.sender = n1.Current.GetAttribute("sender", e);
            this.instanceId = n1.Current.GetAttribute("instanceId", e);

            if ((s = n1.Current.GetAttribute("creationTime", e)) != e)
                if (!DateTime.TryParse(s, out this.creationTime))
                    this.creationTime = DateTime.Now;
            if ((s = n1.Current.GetAttribute("bufferSize", e)) != e)
                try { this.bufferSize = Convert.ToInt64(s); }
                catch { this.bufferSize = 0; }
            if (nsManager == null)
            {
                // first use: load up the namespace manager for resolution.
                // BUG: should do this after loading doc - check if xmlns: attribute is
                // present and if so whether its value matches current nsUri
                nsManager = new XmlNamespaceManager(xmlNav.NameTable);
                nsUri = "urn:mtconnect.com:MTConnectStreams:0.9";
                nsManager.AddNamespace("m", nsUri);
            }

            this.isSuccess = true;
        }
        public override String ToString() { return this.xmlNav.OuterXml; }
    }

}
