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
            var xDocLeft = new XDocument();

            loc_2 = args[5];
            fname_or_url_2 = args[6];
            xpath_2 = args[7];
            kpath_2 = args[8];
            spath_2 = args[9];
            var xDocRight = new XDocument();
            /*
            loc_1 = "/FILE-XML";
            fname_or_url_1 = "MyCustomers.xml";
            xpath_1 = "Customers/Customer";
            kpath_1 = "@CustomerID";
            spath_1 = "@CustomerID";
            var xDocLeft = new XDocument();

            loc_2 = "/FILE-JSON";
            fname_or_url_2 = "Orders.json";
            xpath_2 = "root/value";
            kpath_2 = "OrderID";
            spath_2 = "OrderID";
            var xDocRight = new XDocument();
            */
            switch (loc_1)
            {
                case "/FILE-JSON":
                    // FILE-JSON ==> XML            
                    try
                    {
                        var json_data = string.Empty;
                        json_data = File.ReadAllText(fname_or_url_1);
                        xDocLeft = JsonConvert.DeserializeXNode(json_data, "root");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Loading Json Data error: {0}\n\n", e);
                    }
                    break;
                case "/URL-JSON":
                    //xDocLeft = XDocument.Load(fname_or_url_1);


                    try
                    {
                        Console.WriteLine("WEB-JSON --> XML:\n");
                        string server = "http://services.odata.org/Northwind/Northwind.svc/";
                        string order = "Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID";
                        string format = "&$format=json";
                        string url = server + order + format;

                        var combinedxml = new XDocument();

                        using (var w = new System.Net.WebClient())
                        {
                            var json_data = string.Empty;
                            try
                            {
                                json_data = w.DownloadString(url);
                                XDocument xml1 = Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
                                combinedxml.Root.Add(xml1.Descendants("root"));
                            nextLine:
                                if (xml1.XPathSelectElement("//odata.nextLink") != null)
                                {
                                    url = server + xml1.XPathSelectElement("//odata.nextLink").Value + format;
                                    xml1.XPathSelectElement("//odata.nextLink").Remove();
                                    json_data = w.DownloadString(url);
                                    XDocument xml2 = Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
                                    xml1 = xml1.Descendants("root").Union(xml2.Descendants("root"));
                                    var xmlcombined = xml1.Descendants("root").Union(xml2.Descendants("root"));

                                    goto nextLine;
                                }

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Fetching Json Data error: {0}\n\n", e);
                            }
                        }

                        //string json = "MyOrdersExtended.json";
                        //XDocument jsonDoc;
                        //jsonDoc = JsonConvert.DeserializeXNode(json, "root");
                        //Console.WriteLine(jsonDoc);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Open Json file error: {0}\n\n", e);
                    }

                    Console.ReadKey();

                    break;
                default:
                    xDocLeft = XDocument.Load(fname_or_url_1);
                    break;
            }

            switch (loc_2)
            {
                case "/FILE-JSON":
                    try
                    {
                        var json_data = string.Empty;
                        json_data = File.ReadAllText(fname_or_url_2);
                        xDocRight = (XDocument)JsonConvert.DeserializeXNode(json_data, "root");
                        xDocRight.Save("JsonToXML.xml");
                        Console.WriteLine("Loaded a Json file.\n");
                        Console.WriteLine(xDocRight.XPathSelectElement("root/value").Name + "\n\n");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Loading Json Data error: {0}\n\n", e);
                    }
                    break;
                case "/URL-JSON":
                    //xDocRight = XDocument.Load(fname_or_url_2);
                    break;
                default:
                    xDocRight = XDocument.Load(fname_or_url_2);
                    break;
            }


            // LeftSeqs
            //var xDoc = XDocument.Load(fname_or_url_1);
            IOrderedEnumerable<XElement> nodes;
            switch (spath_1.Substring(0, 1))
            {
                case "@":
                    nodes = from feed in xDocLeft.XPathSelectElements(xpath_1)
                            orderby feed.Attribute(spath_1.Substring(1, spath_1.Length - 1)).Value
                            select feed;
                    break;
                default:
                    nodes = from feed in xDocLeft.XPathSelectElements(xpath_1)
                            orderby feed.Value
                            select feed;
                    break;
            }
            var result_LeftSeqs = new XElement("LeftSeq", nodes);

            //System.Console.Write(result_LeftSeqs);
            result_LeftSeqs.Save("_LeftSeq.xml");

            // RightSeqs
            //xDoc = XDocument.Load(fname_or_url_2);
            //IOrderedEnumerable<XElement> nodes;
            switch (spath_2.Substring(0, 1))
            {
                case "@":
                    nodes = from feed in xDocRight.XPathSelectElements(xpath_2)
                            orderby feed.Attribute(spath_2.Substring(1, spath_2.Length - 1)).Value
                            select feed;
                    break;
                default:
                    nodes = from feed in xDocRight.XPathSelectElements(xpath_2)
                            orderby feed.Value
                            select feed;
                    break;
            }
            var result_RightSeqs = new XElement("RightSeq", nodes);

            //System.Console.Write(result_LeftSeqs);
            result_RightSeqs.Save("_RightSeq.xml");

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
