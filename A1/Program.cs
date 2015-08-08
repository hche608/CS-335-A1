using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Xml.XPath;

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

            Func<String, XDocument> loadxml = delegate(String s)
            {
                XDocument result = XDocument.Load(s);
                return result;
            };

            Func<String, XDocument> loadjson = delegate(String s)
            {
                var json_data = string.Empty;
                json_data = File.ReadAllText(s);
                XDocument result = Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
                return result;
            };

            Func<String, XDocument> loadurl = delegate(String s)
            {
                string server = @"http://services.odata.org/Northwind/Northwind.svc/";
                string order = "Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID";
                string format = "&$format=json";
                string url = server + order + format;

                List<XElement> nodes = new List<XElement>();
                var w = new System.Net.WebClient();
                XDocument xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(url), "root");

            nextLine:
                if (xml.XPathSelectElement("//odata.nextLink") != null)
                {
                    url = server + xml.XPathSelectElement("//odata.nextLink").Value + format;
                    xml.XPathSelectElement("//odata.nextLink").Remove();
                    xml.XPathSelectElement("//odata.metadata").Remove();
                    nodes.AddRange(xml.XPathSelectElements("root/value"));
                    xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(url), "root");
                    goto nextLine;
                }
                nodes.AddRange(xml.XPathSelectElements("root/value"));
                XDocument result = new XDocument(new XElement("root", nodes));
                return result;
            };

            Func<String, String, String, XDocument, XDocument> orderByKey = delegate(String title, String xpath, String key, XDocument xDoc)
            {
                XDocument result = new XDocument();
                if (key.Substring(0, 1) == "@")
                    result = new XDocument(new XElement(title, xDoc.XPathSelectElements(xpath).OrderBy(atr => (String)atr.Attribute(key.Substring(1, key.Length - 1)).Value, StringComparer.Ordinal)));
                else
                    result = new XDocument(new XElement(title, xDoc.XPathSelectElements(xpath).OrderBy(el => (String)el.XPathSelectElement(key), StringComparer.Ordinal)));
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

            /*
            try
            {

                // Inner Join
                var result_InnerJoin = new XElement("InnerJoin",
                    from customer in xDocLeft.Descendants(xpath_1)
                    join order in xDocRight.Descendants(xpath_2)
                    on (string)customer.Attribute(spath_1)
                    equals
                    (string)order.Attribute(spath_2)
                    orderby customer.Attribute(spath_1).Value ascending, order.Attribute(kpath_2).Value ascending
                    select new XElement("Join",
                        new XElement(xpath_1,
                            new XAttribute(kpath_1, customer.Attribute(kpath_1).Value), customer.Value),
                        new XElement(xpath_2,
                            new XAttribute(kpath_2, order.Attribute(kpath_2).Value),
                            new XAttribute(spath_2, order.Attribute(spath_2).Value), order.Value)));

                System.Console.Write(result_InnerJoin);
                result_InnerJoin.Save("_InnerJoin.xml");

                System.Console.Write("\n\n");

                // LeftOuterJoin
                var result_LeftOuterJoin = 
                    new XElement("LeftOuterJoin",
                    from customer in xDocLeft.Descendants(xpath_1)
                    join order in xDocRight.Descendants(xpath_2)
                    on (string)customer.Attribute(spath_1)
                    equals
                    (string)order.Attribute(spath_2)
                    into lotJoin                    
                    from feed in lotJoin.DefaultIfEmpty()
                    orderby customer.Attribute(spath_1).Value ascending, feed == null ? String.Empty : feed.Attribute(kpath_2).Value ascending
                    select new XElement("Join",
                        new XElement(xpath_1,
                            new XAttribute(kpath_1, customer.Attribute(kpath_1).Value), customer.Value),
                            new XElement(xpath_2,
                                new XAttribute(kpath_2, feed == null ? String.Empty : feed.Attribute(kpath_2).Value),
                                new XAttribute(spath_2, feed == null ? String.Empty : feed.Attribute(spath_2).Value), feed == null ? String.Empty : feed.Value)));
                result_LeftOuterJoin.Descendants("Join").Elements(xpath_2).Where(x => x.Value == "").Remove();
                System.Console.Write(result_LeftOuterJoin);

                result_LeftOuterJoin.Save("_LeftOuterJoin.xml");
                System.Console.Write("\n\n");

                // GroupJoin
                var result_GroupJoin = new XElement("GroupJoin",
                                        //from customer in xDocLeft.Descendants(xpath_1)
                                        from order in xDocRight.Descendants(xpath_2)
                                        //on (string)customer.Attribute(spath_1)
                                        //equals
                                        //(string)order.Attribute(spath_2)
                                        //orderby customer.Attribute(spath_1).Value ascending, order == null ? String.Empty : order.Attribute(kpath_2).Value ascending
                                        group order by order.Attribute("CID").Value
                                        into grped_order
                                        join customer in xDocLeft.Descendants(xpath_1)
                                        on (string)grped_order.Key
                                        equals
                                        (string)customer.Attribute(spath_2)
                                        into gp
                                        orderby grped_order.Key
                                        select new XElement("Join",
                                                    new XElement(xpath_1, 
                                                        new XAttribute(kpath_1, grped_order.Key), ""),
                                                    new XElement("Group", 
                                                        new XAttribute("Count", grped_order.Count()), grped_order.DescendantsAndSelf())  
                                            ));
          
                System.Console.Write(result_GroupJoin);
                result_GroupJoin.Save("_GroupJoin.xml");

                System.Console.Write("\n\n");
         
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("XML does not exist. {0}", e);
                Console.ReadKey();
            }*/
        }
    }
}
