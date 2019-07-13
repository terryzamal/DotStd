﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotStd
{
    public static class HTMLUtil
    {
        // Helper functions for formatting HTML.
        // compliment Encode, Decode
        // In some ways HTML can be treated like XML except for some exceptions. (some non closed tags, literals, some encoding)
        // Use HttpUtility.HtmlEncode() to encode a string to proper HTML. Though FormUrlEncodedContent is for URL args.

        public const string kNBSP = "&nbsp;";   // HTML non breaking space

        public const string kCommentOpen = "<!--";
        public const string kCommentClose = "-->";

        public static string DecodeEntities2( string src)
        {
            // replace Non standard entities with chars. HTML is not the same as XML.
            // Replacing "&nbsp;" with "&#160;" since "&nbsp;" is not XML standard
            // NOTE: XmlReader is vulnerable to attacks from entity creation. XSS. OK in .Net 4.0+ but use DtdProcessing.Prohibit.

            src = src.Replace(kNBSP, " ");
            src = src.Replace("&eacute;", "é");
            src = src.Replace("&trade;", "™");
            
            return src;
        }

        public static string GetOptStr(string value, string desc, bool selected = false)
        {
            // for <select>
            // construct HTML "<option value='1'>Main</option>");
            // like TupleKeyValue
            return string.Concat("<option ", selected ? "selected " : string.Empty, "value='", value, "'>", desc, "</option>");
        }

        public static string GetOptStr(int value, string desc, bool selected = false)
        {
            // construct HTML 
            // like TupleIdValue
            return GetOptStr(value.ToString(), desc, selected);
        }
 
        public static string GetOptStr(Enum n, bool selected = false)
        {
            // construct HTML 
            // Get a value from enum to populate a HTML select list option.
            return GetOptStr(n.ToInt(), n.ToDescription(), selected);
        }

        public static bool IsCommentLine(string s)
        {
            s = s.Trim();
            return s.StartsWith(kCommentOpen) && s.EndsWith(kCommentClose);
        }

        public static string GetList(IEnumerable<string> a)
        {
            if (!a.Any())
                return "";
            var sb = new StringBuilder();
            sb.Append("<ul>");
            foreach (var s in a)
            {
                sb.Append("<li>");
                sb.Append(s);
                sb.Append("</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        public static string Post(string url, string prams)
        {
            // prams = already encoded params. e.g. "f=1&b=2"
            // Similar to WebClient.UploadValues()
            // https://stackoverflow.com/questions/4088625/net-simplest-way-to-send-post-with-data-and-read-response

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            // req.Proxy = new System.Net.WebProxy(ProxyString, true);
            //Add these, as we're doing a POST
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            //We need to count how many bytes we're sending. 
            //Post'ed Faked Forms should be name=value&
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(prams);
            req.ContentLength = bytes.Length;

            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();

            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null)
                return null;
            var sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
    }
}
