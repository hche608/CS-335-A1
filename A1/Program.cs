using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace A1
{
    class Program
    {

        static void Main(string[] args)
        {

            string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1, loc_2, fname_or_url_2, xpath_2, kpath_2, spath_2;

            loc_1 = args[0];
            fname_or_url_1 = args[1];
            xpath_1 = args[2];
            kpath_1 = args[3];
            spath_1 = args[4];
            XDocument xDocLeft = new XDocument();

            loc_2 = args[5];
            fname_or_url_2 = args[6];
            xpath_2 = args[7];
            kpath_2 = args[8];
            spath_2 = args[9];
            XDocument xDocRight = new XDocument();

            Func<String, XDocument> loadxml = delegate (String s)
            {
                return XDocument.Load(s);
            };

            Func<String, XDocument> loadjson = delegate (String s)
            {
                var json_data = string.Empty;
                json_data = File.ReadAllText(s);
                return Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
            };

            Action<String, String, String, List<XElement>> getUrlJson = null;
            getUrlJson = delegate (String s, String o, String f, List<XElement> nodes)
            {
                var w = new System.Net.WebClient();
                XDocument xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(s + o + f), "root");
                if (xml.XPathSelectElement("//odata.nextLink") != null)
                {
                    o = xml.XPathSelectElement("//odata.nextLink").Value;
                    xml.XPathSelectElement("//odata.nextLink").Remove();
                    nodes.AddRange(xml.XPathSelectElements("root/value"));
                    getUrlJson(s, o, f, nodes);
                }
                else
                    nodes.AddRange(xml.XPathSelectElements("root/value"));
            };

            Func<String, XDocument> loadurl = delegate (String s)
            {
                string server = Regex.Match(s, @"(https?)(\w+|\W+)+?svc\/").Value;
                string format = Regex.Match(s, @"\&\$format=\w+").Value;
                string order = s.Replace(server, String.Empty).Replace(format, String.Empty);
                List<XElement> nodes = new List<XElement>();
                getUrlJson(server, order, format, nodes);
                XDocument result = new XDocument(new XElement("root", nodes));
                return result;
            };

            Func<XElement, String, String> selector = delegate(XElement x, String path)
            {
                var y = x.XPathEvaluate(path) as IEnumerable<object>;
                if (y == null) return null;
                dynamic z = y.FirstOrDefault();
                if (z == null) return null;
                return z.Value; // expected y : XElement or XAttribute
            };

            Func<String, String, String, XDocument, XDocument> orderByKey = delegate (String title, String xpath, String spath, XDocument xDoc)
            {
                XDocument result = new XDocument(new XElement(title, xDoc.XPathSelectElements(xpath).OrderBy(v => selector(v, spath), StringComparer.Ordinal)));
                result.Save("_" + title + ".xml");
                return result;
            };

            if (loc_1 == "/FILE-JSON")
                xDocLeft = loadjson(fname_or_url_1);
            if (loc_2 == "/FILE-JSON")
                xDocRight = loadjson(fname_or_url_2);
            if (loc_1 == "/URL-JSON")
                xDocLeft = loadurl(fname_or_url_1);
            if (loc_2 == "/URL-JSON")
                xDocRight = loadurl(fname_or_url_2);
            if (loc_1 == "/FILE-XML")
                xDocLeft = loadxml(fname_or_url_1);
            if (loc_2 == "/FILE-XML")
                xDocRight = loadxml(fname_or_url_2);

            // LeftSeqs
            xDocLeft = orderByKey("LeftSeq", xpath_1, spath_1, xDocLeft);
            // RightSeqs
            xDocRight = orderByKey("RightSeq", xpath_2, spath_2, xDocRight);
            // Inner Join
            XDocument result_InnerJoin = new XDocument(new XElement("InnerJoin",
                from lSide in xDocLeft.XPathSelectElements("LeftSeq/*")
                join rSide in xDocRight.XPathSelectElements("RightSeq/*")
                on selector(lSide,kpath_1) equals selector(rSide, kpath_2)
                select new XElement("Join", lSide, rSide)));
            //result_InnerJoin.Save("_InnerJoin.xml");
            // GroupJoin
            var groupJoin =
                    xDocLeft.XPathSelectElements("LeftSeq/*").GroupJoin(result_InnerJoin.XPathSelectElements("InnerJoin/*")
                    .GroupBy(el => selector(el.XPathSelectElement(xpath_2.Replace(Regex.Match(xpath_2, @"\w+\/").Value, String.Empty)), kpath_2)),
                    lSide => (kpath_1.Contains('@') ? (lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)) == null ? String.Empty : lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)).Value) :
                    (lSide == null ? String.Empty : lSide.XPathSelectElement(kpath_1).Value)),
                    grpB => grpB.Key,
                    (lSide, grpB) => new XDocument(new XElement("GroupJoin", new XElement("Join", lSide, new XElement("Group", new XAttribute("Count", c => c.), grpB)))));

            
        }
    }
}
