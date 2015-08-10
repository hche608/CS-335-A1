<Query Kind="Program">
  <Reference Relative="Newtonsoft.Json.dll">C:\Users\hche608\Source\Repos\cs-335\A1\Newtonsoft.Json.dll</Reference>
</Query>

void Main()
{
            string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1, loc_2, fname_or_url_2, xpath_2, kpath_2, spath_2;

            loc_1 = "/FILE-XML";
            fname_or_url_1 = "MyCustomers.xml";
            xpath_1 = "Customers/Customer";
            kpath_1 = "@CustomerID";
            spath_1 = "@CustomerID";
            XDocument xDocLeft = new XDocument();

            loc_2 = "/FILE-XML";
            fname_or_url_2 = "MyOrders.xml";
            xpath_2 = "Orders/Order";
            kpath_2 = "@CID";
            spath_2 = "@OrderID";
            XDocument xDocRight = new XDocument();

			
            Func<String, XDocument> loadxml = delegate (String s)
            {
                //XDocument result = XDocument.Load(s);
                return XDocument.Load(s);
            };

            Func<String, XDocument> loadjson = delegate (String s)
            {
                var json_data = string.Empty;
                json_data = File.ReadAllText(s);
                //XDocument result = Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
                return Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data, "root");
            };


            Action<String, String, String, List<XElement>> getUrlJson = null;
            getUrlJson = delegate (String s, String o, String f, List<XElement> ns)
            {
                var w = new System.Net.WebClient();
                XDocument xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(s + o + f), "root");
                if (xml.XPathSelectElement("//odata.nextLink") != null)
                {
                    o = xml.XPathSelectElement("//odata.nextLink").Value;
                    xml.XPathSelectElement("//odata.nextLink").Remove();

                    ns.AddRange(xml.XPathSelectElements("root/value"));
                    getUrlJson(s, o, f, ns);
                } else 
                ns.AddRange(xml.XPathSelectElements("root/value"));
            };


            Func<String, XDocument> loadurl = delegate (String s)
            {
                string server = "http://services.odata.org/Northwind/Northwind.svc/";
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
                        (v == null ? String.Empty : v.XPathSelectElement(spath).Value)), StringComparer.Ordinal)));
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
			xDocLeft.XPathSelectElements("LeftSeq/*").Dump("Left");
            // RightSeqs
            xDocRight = orderByKey("RightSeq", xpath_2, spath_2, xDocRight);
			//xDocRight.XPathSelectElements("RightSeq/*").Dump("Right");
			// InnerJoin

            XDocument result_InnerJoin = new XDocument(new XElement("InnerJoin",
                from lSide in xDocLeft.XPathSelectElements("LeftSeq/*")
                join rSide in xDocRight.XPathSelectElements("RightSeq/*")
                on (string)(kpath_1.Contains('@') ? (lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)) == null ? String.Empty : lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)).Value) :
                (lSide == null ? String.Empty : lSide.XPathSelectElement(kpath_1).Value))
                equals
                (kpath_2.Contains('@') ? (rSide.Attribute(kpath_2.Substring(1, kpath_2.Length - 1)) == null ? String.Empty : rSide.Attribute(kpath_2.Substring(1, kpath_2.Length - 1)).Value) :
                (rSide == null ? String.Empty : rSide.XPathSelectElement(kpath_2).Value))
                select new XElement("Join", lSide, rSide)));
			result_InnerJoin.XPathSelectElements("InnerJoin/Join/*").Dump("InnerJoin");
			//result_InnerJoin.Save("_InnerJoin.xml");			
			
			var groupJoin =
                xDocLeft.XPathSelectElements("LeftSeq/*").GroupJoin(result_InnerJoin.XPathSelectElements("InnerJoin/*")
                .GroupBy(el => el.XPathSelectElement(xpath_2.Substring(xpath_2.IndexOf('/') + 1,xpath_2.Length - xpath_2.IndexOf('/') - 1)).Attribute("CID").Value),
                lSide => (kpath_1.Contains('@') ? (lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)) == null ? String.Empty : lSide.Attribute(kpath_1.Substring(1, kpath_1.Length - 1)).Value) :
                (lSide == null ? String.Empty : lSide.XPathSelectElement(kpath_1).Value)),
                grpB => grpB.Key,
                (lSide,grpB) => new { lSide, grpB });
			groupJoin.Dump("GroupJoin");	
			
}

// Define other methods and classes here