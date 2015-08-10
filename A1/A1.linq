<Query Kind="Program">
  <Reference Relative="Newtonsoft.Json.dll">C:\Users\hche608\Source\Repos\cs-335\A1\Newtonsoft.Json.dll</Reference>
</Query>

void Main()
{
	string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1;
	loc_1 = "/FILE-XML";
    fname_or_url_1 = "MyCustomers.xml";
    xpath_1 = "Customers/Customer";
    kpath_1 = "@CustomerID";
    spath_1 = "@CustomerID";
	XDocument leftXDoc = XDocument.Load(fname_or_url_1);
	
	leftXDoc.XPathSelectElements(xpath_1).Dump("leftXDoc");
	leftXDoc.XPathSelectElements("Customers/Customer/@CustomerID").Dump("What I Got");
	var test = leftXDoc.XPathSelectElements(xpath_1).OrderBy( s => (String)s, StringComparer.Ordinal);
	test.Dump("Test");
	var result_LeftSeqs = new XElement("LeftSeq", from feed in leftXDoc.XPathSelectElements(xpath_1)
                            orderby feed.Attribute(spath_1.Substring(1, spath_1.Length - 1)).Value
                            select feed);
	result_LeftSeqs.XPathSelectElements("/Customer").Dump("leftXDoc After Sort");
	
	Console.WriteLine("WEB-JSON --> XML:\n");
	string server = "http://services.odata.org/Northwind/Northwind.svc/";
	string order = "Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID";
	string format = "&$format=json";
	string url = server + order + format;
	//var xmlcombined = XDocument();

	using (var w = new System.Net.WebClient())
	{
		
		try
		{
			XDocument xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(url), "root");
			//xmlcombined.Root.Add(xml1.Descendants("root"));

					
			nextLine:
			if(xml.XPathSelectElement("//odata.nextLink") != null){
				//xml.XPathSelectElement("//odata.nextLink").Value.Dump("NextLink");
				url = server + xml.XPathSelectElement("//odata.nextLink").Value + format;
				xml.XPathSelectElement("//odata.nextLink").Remove();
				xml.XPathSelectElement("//odata.metadata").Remove();
				xml.XPathSelectElements("root/value").Dump("Test");
				
				xml = Newtonsoft.Json.JsonConvert.DeserializeXNode(w.DownloadString(url), "root");
				

				goto nextLine;
			}

		}
			catch (Exception e)
		{
			Console.WriteLine("Fetching Json Data error: {0}\n\n", e);
		}
	}
	
	
	
	
	/*
	var xDoc = XDocument.Load(fname_or_url_1);
	xDoc.XPathSelectElements(xpath_1).Dump("XML");
	IOrderedEnumerable<XElement> nodes;
	switch (spath_1.Substring(0, 1))
	{
	case "@":
		nodes = from feed in xDoc.XPathSelectElements(xpath_1)
				orderby feed.Attribute(spath_1.Substring(1, spath_1.Length - 1)).Value
				select feed;
		break;
	default:
		nodes = from feed in xDoc.XPathSelectElements(xpath_1)
				orderby feed.XPathSelectElements("" + xpath_1 + "/" +spath_1).FirstOrDefault().Value
				select feed;
		break;
	}
	var result_LeftSeqs = new XElement("LeftSeq", nodes);
	result_LeftSeqs.Dump("XML");
	
	//**********************************
	fname_or_url_1 = "Orders.json";
    xpath_1 = "root/value";
    kpath_1 = "OrderID";
    spath_1 = "OrderID";
	
	var json_data = string.Empty;
    json_data = File.ReadAllText(fname_or_url_1);
    xDoc = Newtonsoft.Json.JsonConvert.DeserializeXNode(json_data,"root");
	xDoc.Dump("Loaded JSON");
	xDoc.XPathSelectElements("/root/value[OrderID[1]]").Dump("JSON");
	//IOrderedEnumerable<XElement> nodes;
	switch (spath_1.Substring(0, 1))
	{
	case "@":
		nodes = from feed in xDoc.XPathSelectElements(xpath_1)
				orderby feed.Attribute(spath_1.Substring(1, spath_1.Length - 1)).Value
				select feed;
		break;
	default:
		nodes = from feed in xDoc.XPathSelectElements(xpath_1)
				orderby feed.XPathSelectElement("//OrderID").Value descending
				select feed;
		break;
	}
	result_LeftSeqs = new XElement("LeftSeq", nodes);
	result_LeftSeqs.Dump("XML2");*/
	
}

// Define other methods and classes here