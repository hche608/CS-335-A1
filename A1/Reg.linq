<Query Kind="Program" />

void Main()
{
	string url ="http://services.odata.org/Northwind/Northwind.svc/Orders()?$orderby=OrderID desc&$select=OrderID,CustomerID,EmployeeID&$format=json";
	System.Text.RegularExpressions.Regex.Replace(url,@"\Orders\W+\w+\W+\w+\s+\w+\W+\w+=+\w+,\w+,\w+","goole");
}

// Define other methods and classes here
