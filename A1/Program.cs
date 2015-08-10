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
                string server = @"http://services.odata.org/Northwind/Northwind.svc/";
                string order = "Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID";
                string format = "&$format=json";
                List<XElement> nodes = new List<XElement>();
                getUrlJson(server, order, format, nodes);
                XDocument result = new XDocument(new XElement("root", nodes));
                return result;
            };

            Func<String, String, String, XDocument, XDocument> orderByKey = delegate (String title, String xpath, String spath, XDocument xDoc)
            {
                XDocument result = new XDocument(new XElement(title, xDoc.XPathSelectElements(xpath).OrderBy(
                    v => (spath.Contains("@") ?
                        (v.Attribute(spath.Substring(1, spath.Length - 1)) == null ? String.Empty : v.Attribute(spath.Substring(1, spath.Length - 1)).Value) :
                        (v.Value == null ? String.Empty : v.XPathSelectElement(spath).Value)), StringComparer.Ordinal)));
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
                on (string)(kpath_1.Contains('@') ? (lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)) == null ? String.Empty : lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)).Value) :
                (lSide == null ? String.Empty : lSide.XPathSelectElement(kpath_1).Value))
                equals
                (kpath_2.Contains('@') ? (rSide.Attribute(kpath_2.Substring(1, kpath_2.Length - 1)) == null ? String.Empty : rSide.Attribute(kpath_2.Substring(1, kpath_2.Length - 1)).Value) :
                (rSide == null ? String.Empty : rSide.XPathSelectElement(kpath_2).Value))
                select new XElement("Join", lSide, rSide)));
            result_InnerJoin.Save("_InnerJoin.xml");
            // GroupJoin
            var result_GroupBy =
                result_InnerJoin.XPathSelectElements("InnerJoin/*")
                .GroupBy(el => el.XPathSelectElement("Order").Attribute("CID").Value)
                .GroupJoin(xDocLeft.XPathSelectElements("LeftSeq/*"),
                grpB => grpB.Key.DefaultIfEmpty(),
                lSide => lSide.Attribute("CustomerID").Value,
                (grpB, lSide) => new { lSide, grpB }
                );

            var groupJoin =
                xDocLeft.XPathSelectElements("LeftSeq/*").GroupJoin(result_InnerJoin.XPathSelectElements("InnerJoin/*")
                .GroupBy(el => el.XPathSelectElement(xpath_2.Substring(xpath_2.IndexOf('/') + 1, xpath_2.Length - xpath_2.IndexOf('/') - 1)).Attribute("CID").Value),
                lSide => (kpath_1.Contains('@') ? (lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)) == null ? String.Empty : lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)).Value) :
                (lSide == null ? String.Empty : lSide.XPathSelectElement(kpath_1).Value)),
                grpB => grpB.Key,
                (lSide, grpB) => new { lSide, grpB });

        }
    }
}
