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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace MTConnect
{
    public class MTCEvent
    {
        //public String path;  // path to component, starting with Device name
        public String componentId;
        public String componentType; // eg "Linear"
        public String componentName;    // eg "Y"
        public Int32 sequence; // sequence number
        public ValueType valueType;
        public ValueSubtype valueSubtype;
        public SamplingType samplingType;
        public Units units;
        public String deviceUuid;
        public String lane;
        public DateTime timestamp;
        public String dataItemId;
        public String value;
        public bool hasParseErrors;
        public String parseErrors;

        public enum ValueType
        {
            UNKNOWN = 0,
            ACCELERATION,
            ALARM,
            AMPERAGE,
            ANGLE,
            ANGULAR_ACCELERATION,
            ANGULAR_VELOCITY,
            BLOCK,
            CODE,
            DIRECTION,
            EXECUTION,
            PATH_FEEDRATE,
            AXIS_FEEDRATE,
            LINE,
            CONTROLLER_MODE,
            LOAD,
            OTHER,
            POSITION,
            POWER_STATUS,
            PRESSURE,
            PROGRAM,
            SPINDLE_SPEED,
            STATUS,
            TEMPERATURE,
            TICK,
            TOOL,
            TRANSFER,
            VELOCITY,
            VIBRATION,
            VOLTAGE,
            WATTS
        }
        public enum ValueSubtype
        {
            UNKNOWN = 0,
            ACTUAL,
            COMMANDED,
            DISTANCE,
            MAXIMUM,
            MAXOVERRIDE,
            MINIMUM,
            OTHER,
            OVERRIDE,
            TARGET,
            NONE       // not all value types have a subtype
        }
        public enum SamplingType
        {
            UNKNOWN = 0, Discrete, Continuous, State
        }
        public enum Units
        {
            UNKNOWN = 0,
            STATUS,
            MILLIMETER,
            DEGREE,
            MILLIMETER_SECOND,
            DEGREE_SECOND,
            MILLIMETER_SECOND_2,
            DEGREE_SECOND_2,
            LITER,
            KILOGRAM,
            NEWTON,
            CELSIUS,
            REVOLUTION_MINUTE,
            VOLT,
            AMPERE,
            WATT,
            TICK,
            PERCENT
        }
        public MTCEvent()
        {
            this.valueType = ValueType.UNKNOWN;
            this.valueSubtype = ValueSubtype.UNKNOWN;
            this.units = Units.UNKNOWN;
            this.hasParseErrors = false;
        }
        public Double ValueAsDouble()
        {
            Double result = Double.NaN;
            try
            {
                result = Convert.ToDouble(this.value);
            }
            catch
            {
                this.hasParseErrors = true;
                this.parseErrors += String.Format("Can't convert value '{0}' to Double", this.value);
            }
            return result;
        }
        public Int32 ValueAsInt32()
        {
            Int32 result = 0;
            try
            {
                result = Convert.ToInt32(this.value);
            }
            catch
            {
                this.hasParseErrors = true;
                this.parseErrors += String.Format("Can't convert value '{0}' to Int32", this.value);
            }
            return result;
        }
        public static Units GetUnits(String unitsString)
        {
            foreach (Units u in Enum.GetValues(typeof(Units)))
            {
                if (CompareAlphaOnly(unitsString, u.ToString()) == 0)
                {
                    return u;
                }
            }
            return Units.UNKNOWN;
        }
        public static ValueType GetValueType(String valueTypeString)
        {
            foreach (ValueType v in Enum.GetValues(typeof(ValueType)))
            {
                if (CompareAlphaOnly(valueTypeString, v.ToString()) == 0)
                {
                    return v;
                }
            }
            return ValueType.UNKNOWN;
        }
        public static ValueSubtype GetValueSubtype(String valueSubtypeString)
        {
            if (valueSubtypeString.Length == 0)
                return ValueSubtype.NONE;
            foreach (ValueSubtype s in Enum.GetValues(typeof(ValueSubtype)))
            {
                if (CompareAlphaOnly(valueSubtypeString, s.ToString()) == 0)
                    return s;
            }
            return ValueSubtype.UNKNOWN;
        }        
        private static int CompareAlphaOnly(String a, String b)
        {
            String a1 = a.Replace("/", "").Replace("^", "").Replace("_", "");
            String b1 = b.Replace("/", "").Replace("^", "").Replace("_", "");
            return String.Compare(a1, b1, true);
        }
        public bool ExtractEvent(String measType,
            XPathNavigator eventOrSample, XPathNavigator componentStream,
            XPathNavigator probeInfo)
        {
            String e = String.Empty;

            // get common stuff from componentStream node. 
            // PERF: This should probably be
            // refactored so that we only do GetAttribute when processing the first
            // event/sample inside a Component node, and just re-use the same string
            // constant thereafter.
            //this.path = componentStream.GetAttribute("path", e);
            this.componentId = componentStream.GetAttribute("componentId", e);
            this.componentName = componentStream.GetAttribute("name", e);
            this.componentType = componentStream.GetAttribute("component", e);
            this.deviceUuid = componentStream.GetAttribute("uuid", e);
            this.lane = componentStream.GetAttribute("lane", e);
            // get event-specific values
            this.sequence = Int32.Parse(eventOrSample.GetAttribute("sequence", e));
            this.timestamp = DateTime.Parse(eventOrSample.GetAttribute("timestamp", e));
            this.dataItemId = eventOrSample.GetAttribute("dataItemId", e);
            this.value = eventOrSample.Value;
            // convert element type to correct Event.ValueType enum
            if ((this.valueType = GetValueType(eventOrSample.LocalName)) == ValueType.UNKNOWN)
            {
                this.hasParseErrors = true;
                this.parseErrors += String.Format("Event type '{0}' unknown\n", eventOrSample.LocalName);
            }
            // convert subtype, if present, to correct ValueSubType enum. OK for it to be absent.
            this.valueSubtype = GetValueSubtype(eventOrSample.GetAttribute("subType", e));
            // get units, if measurement is a Sample (vs. an Event).
            // If probeInfo is null, then no probe has been
            // done so we don't know the units. Leave set to UNKNOWN, and set the hasParseErrors
            // flag after adding an appropriate message to the parseErrors field.  If probeInfo
            // is not null, it's an XPathNavigator to which we can directly pass the Path attribute
            // of this sample or measurement, and get the units out.
            if (measType[0] == 'S' || measType[0] == 's')
            {
                if (probeInfo is XPathNavigator)
                    this.ExtractUnits(probeInfo);
                else
                {
                    // there's no probe info available to get units
                    this.hasParseErrors = true;
                    this.parseErrors += "No Probe info available to get units";

                }
            }
            return !(this.hasParseErrors);
        }
        private void ExtractUnits(XPathNavigator probeInfo)
        {
            String pathToDataItem = String.Format(
                  "//DataItem[@id='{0}']", this.dataItemId);
            //String pathToDataItem = String.Format(
            //            "//Devices{0}//{1}[@name='{2}']//DataItem[@type='{3}'][@units]",
             //           this.path, this.componentType, this.componentName,
              //          this.valueType.ToString());
            //if (this.valueSubtype != ValueSubtype.UNKNOWN)
            //    pathToDataItem += String.Format("[@subType='{0}']", this.valueSubtype.ToString());
            XPathNodeIterator n = probeInfo.Select(pathToDataItem);
            n.MoveNext();
            // get Units attribute and we're done. 
            String units;
            this.units = Units.UNKNOWN;
            if ((units = n.Current.GetAttribute("units", String.Empty)) == String.Empty)
            {
                this.hasParseErrors = true;
                this.parseErrors += "No Units found in:\n" + n.Current.OuterXml + "\n";
            }
            else
            {
                if ((this.units = GetUnits(units)) == Units.UNKNOWN)
                {
                    this.hasParseErrors = true;
                    this.parseErrors += "Can't recognize unit name " + units + "\n";
                }
            }
        }

    }
}