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
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml.XPath;

namespace MTConnect
{
    public class MTCAgent
    {
        private Uri agentUri;
        private UriBuilder lastUri;
        public bool squashExceptions;
        public enum debugLevel { debugNone, debugErrors, debugWarnings, debugInfo };

        // public members
        public debugLevel debugging;
        public String instanceId;  // instanceID of this Agent
        public String version;  // MTConnect protocol version of the response
        public Int32 nextSequence;  // next sequence number; updated on each Sample or Current
        public XPathNavigator cachedProbe; // navigator on root of first Probe() response

        // constructor: takes a base URL at which to contact the MTCAgent
        public MTCAgent(String url)
        {
            this.agentUri = new Uri(url);
            this.lastUri = new UriBuilder(url);   // no error checking for now.
            this.nextSequence = 0;
            this.cachedProbe = null;
            this.instanceId = String.Empty;
            this.squashExceptions = false;
            this.debugging = debugLevel.debugNone;
        }
        private void Log(debugLevel lvl, String msg)
        {
            if (this.debugging != debugLevel.debugNone)
            {
                Console.WriteLine(msg);
            }
        }
        private String BuildUri(String path, String query)
        {
            // prepend agent host and port
            this.lastUri = new UriBuilder(agentUri.ToString());
            if (!this.lastUri.Path.EndsWith("/")) { this.lastUri.Path += "/"; }
            this.lastUri.Path += path;
            if (query != String.Empty)
            {
                this.lastUri.Query = query;
            }
            return this.lastUri.ToString();
        }
        /// <summary>
        /// The Probe, Sample and Current commands are just wrappers 
        /// that construct the correct XPath expression from their arguments,
        /// then they call OpenXmlStream, which returns an XmlReader object. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <param name="maxSamples"></param>
        /// <returns></returns>
        public MTCProbeResponse Probe()
        {
            MTCProbeResponse r = new MTCProbeResponse(BuildUri("probe", String.Empty), this);
            return r;
        }
        public MTCSampleResponse Sample(String path, Int32 maxSamples, Int32 start)
        {
            String query = String.Format("from={0}&count={1}&path={2}",
                start, maxSamples, path);
            return new MTCSampleResponse(BuildUri("sample", query), this);
        }
        public MTCSampleResponse Sample(String path, Int32 start)
        {
            String query = String.Format("from={0}&path={1}", start, path);
            return new MTCSampleResponse(BuildUri("sample", query), this);
        }
        public MTCSampleResponse Current()
        {
            MTCSampleResponse r = new MTCSampleResponse(BuildUri("current", String.Empty), this);
            return r;
        }
        public MTCSampleResponse Current(String path)
        {
            MTCSampleResponse r =
                new MTCSampleResponse(BuildUri("current", String.Format("path={0}", path)), this);
            return r;
        }
    }
}
