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
    class Part1
    {
        string loc_1, fname_or_url_1, xpath_1, kpath_1, spath_1;
        loc_1 = "/FILE-XML";
        fname_or_url_1 = "MyCustomers.xml";
        xpath_1 = "Customers/Customer";
        kpath_1 = "@CustomerID";
        spath_1 = "@CustomerID";
	    XDocument leftXDoc = XDocument.Load(fname_or_url_1);
        leftXDoc.XPathSelectElements(xpath_1).ToList().Dump("leftXDoc");
        //leftXDoc.XPathSelectElements("Customers/Customer/@CustomerID").Dump("What I Got");
        var test = leftXDoc.XPathSelectElements(xpath_1).ToList().OrderBy(s => (String)s.XPathSelectElements("Customers/Customer/@CustomerID").Value, StringComparer.Ordinal);
        test.Dump("Test");
    }
}
