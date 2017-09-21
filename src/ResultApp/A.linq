<Query Kind="Program" />

static string FormatZipCode(string zipCode)
{
	if (string.IsNullOrWhiteSpace(zipCode))
	{
		// ??
	}
	var regex = new Regex(@"^\d{5}[-\s]*\d{4}?$");
	
	if (regex.IsMatch(zipCode))
		return Regex.Replace($"{zipCode.Substring(0, 5)}-{zipCode.Substring(5)}",  @"\s+", "");
	else
		return null;
}

static void Main()
{
	string zipCode = FormatZipCode("20745    0000");	
	zipCode.Dump();
}