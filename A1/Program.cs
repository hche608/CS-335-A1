﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace A1
{
    class Program
    {
        static void Main(string[] args)
        {
            string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1, loc_2, fname_or_url_2, xpath_2, kpath_2, spath_2;
            loc_1 = "FILE-XML";
            fname_or_url_1 = "MyCustomers.xml";
            xpath_1 = "Customer";
            kpath_1 = "CustomerID";
            spath_1 = "CustomerID";

            loc_2 = "FILE-XML";
            fname_or_url_2 = "MyOrders.xml";
            xpath_2 = "Order";
            kpath_2 = "OrderID";
            spath_2 = "CID";
            try
            {
                XDocument xDocLeft;
                xDocLeft = XDocument.Load(fname_or_url_1);
                //xDoc = XDocument.Parse(URL);

                // LeftSeqs
                var result_LeftSeqs = new XElement("LeftSeq",
                    from feed in xDocLeft.Descendants(xpath_1)
                    orderby feed.Attribute(kpath_1).Value ascending//, feed.Value ascending
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
                var result_LeftOuterJoin = new XElement("LeftOuterJoin",
                    from customer in xDocLeft.Descendants(xpath_1)
                    join order in xDocRight.Descendants(xpath_2)
                    on (string)customer.Attribute(spath_1)
                    equals
                    (string)order.Attribute(spath_2)

                    //orderby customer.Attribute(spath_1).Value ascending, order.Attribute(kpath_2).Value ascending
                    into lotJoin
                    from whatever in lotJoin.DefaultIfEmpty()
                    select new XElement("Join",
                        new XElement(xpath_1,
                            new XAttribute(kpath_1, customer.Attribute(kpath_1).Value), customer.Value),
                        new XElement(xpath_2,
                            new XAttribute(kpath_2, lotJoin.Attribute(kpath_2).Value),
                            new XAttribute(spath_2, lotJoin.Attribute(spath_2).Value), lotJoin.Value)));

                System.Console.Write(result_InnerJoin);
                result_InnerJoin.Save("_InnerJoin.xml");

                System.Console.Write("\n\n");

                //// LeftSeqs
                //var result_LeftSeqs = from feed in xDocLeft.Descendants(xpath_1)
                //             orderby feed.Attribute(kpath_1).Value
                //             select new
                //             {
                //                 xpath = feed.Value,
                //                 kpath = feed.Attribute(kpath_1).Value,
                //                 spath = feed.Attribute(spath_1).Value
                //             };

                //foreach (var detail in result_LeftSeqs)
                //{
                //    Console.WriteLine("xpath: {0}, kpath: {1}, spath: {2}", detail.xpath, detail.kpath, detail.spath);
                //}

                //Console.WriteLine();

                //var xml = new XElement("LeftSeq", result_LeftSeqs.Select(x => new XElement(xpath_1,
                //    new XAttribute(kpath_1, x.kpath), x.xpath)));

                //System.Console.Write(xml);
                //xml.Save("_LeftSeq.xml");

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
