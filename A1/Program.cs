﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Xml;

namespace A1
{
    class Program
    {
        static void Main(string[] args)
        {
            string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1, loc_2, fname_or_url_2, xpath_2, kpath_2, spath_2;
            loc_1 = "/FILE-XML";
            fname_or_url_1 = "MyCustomers.xml";
            xpath_1 = "Customers/Customer";
            kpath_1 = "@CustomerID";
            spath_1 = "@CustomerID";

            loc_2 = "/FILE-XML";
            fname_or_url_2 = "MyOrders.xml";
            xpath_2 = "Orders/Order";
            kpath_2 = "@CID";
            spath_2 = "@OrderID";

            // read JSON directly from a file
            var json_data = string.Empty;
            try
            {
                json_data = File.ReadAllText("Orders.json");
                XDocument jsonDoc = JsonConvert.DeserializeXNode(json_data, "root");
                Console.WriteLine(jsonDoc);
                jsonDoc.Save("JsonFromLocal.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Fetching Json Data error: {0}\n\n", e);
            }
            
            try
            {
                XDocument xDocLeft;
                xDocLeft = XDocument.Load(fname_or_url_1);
                XElement xDocRoot = xDocLeft.Root;
                // LeftSeqs
                var result_LeftSeqs = new XElement("LeftSeq",
                    from feed in xDocRoot.XPathSelectElement(xpath_1)
                    orderby feed.Attribute(spath_1).Value ascending
                    select new XElement(xpath_1,
                            new XAttribute(kpath_1, feed.Attribute(kpath_1).Value), feed.Value));

                System.Console.Write(result_LeftSeqs);
                result_LeftSeqs.Save("_LeftSeq.xml");

                System.Console.Write("\n\n");

                // RightSeqs
                XDocument xDocRight;
                xDocRight = XDocument.Load(fname_or_url_2);

                var result_RightSeqs = new XElement("RightSeq",
                    from feed in xDocRight.Descendants(xpath_2)
                    orderby feed.Attribute(kpath_2).Value ascending

                    select new XElement(xpath_2,
                            new XAttribute(kpath_2, feed.Attribute(kpath_2).Value),
                            new XAttribute(spath_2, feed.Attribute(spath_2).Value), feed.Value)
                            );

                System.Console.Write(result_RightSeqs);
                result_RightSeqs.Save("_RightSeq.xml");

                System.Console.Write("\n\n");

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


                //try
                //{
                //    Console.WriteLine("WEB-JSON --> XML:\n");
                //    string url = "http://services.odata.org/Northwind/Northwind.svc/Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID&$format=json";
                //    using (var w = new WebClient())
                //    {
                //        var json_data = string.Empty;

                //        try
                //        {
                //            json_data = w.DownloadString(url);
                //            XDocument jsonDoc = JsonConvert.DeserializeXNode(json_data, "root");
                //            Console.WriteLine(jsonDoc);
                //            jsonDoc.Save("JsonFromWeb.xml");
                //        }
                //        catch (Exception e)
                //        {
                //            Console.WriteLine("Fetching Json Data error: {0}\n\n", e);
                //        }
                //    }
                //    //string json = "MyOrdersExtended.json";
                //    //XDocument jsonDoc;
                //    //jsonDoc = JsonConvert.DeserializeXNode(json, "root");
                //    //Console.WriteLine(jsonDoc);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("Open Json file error: {0}\n\n", e);
                //}

                Console.ReadKey();

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("XML does not exist. {0}", e);
                Console.ReadKey();
            }
        }
    }
}
