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

	var test = leftXDoc.XPathSelectElements(xpath_1).OrderBy( el => (String)el.Attribute("CustomerID").Value, StringComparer.Ordinal);
	test.Dump("After Sort");
	
	
	string loc_2, fname_or_url_2, xpath_2, kpath_2, spath_2;
	loc_2 = "/FILE-XML";
    fname_or_url_2 = "MyOrders.xml";
    xpath_2 = "Orders/Order";
    kpath_2 = "@CID";
    spath_2 = "@OrderID";
	XDocument rightXDoc = XDocument.Load(fname_or_url_2);
	rightXDoc.XPathSelectElements("Orders/Order/@CID").FirstOrDefault().Value.Dump("rightXDoc");

	var test2 = rightXDoc.XPathSelectElements(xpath_2).OrderBy( el => (String)el.Attribute("OrderID").Value, StringComparer.Ordinal);
	test2.Dump("After Sort 2");
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
	result_LeftSeqs = new XElement("LeftSeq", nodes);
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
	result_LeftSeqs.Dump("XML2");
	*/
}

// Define other methods and classes here