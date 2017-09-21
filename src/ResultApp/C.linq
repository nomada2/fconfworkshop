<Query Kind="Program" />

static void Main()
{
	ZipCode zipCode = ZipCode.FormatZipCode("20745    0001");
	
	//ZipCode zipCode = ZipCode.FormatZipCode("Invalid");
	if (zipCode.IsValid)
		zipCode.Dump();
	else
		"Error".Dump();
}


class ZipCode
{
	public bool IsValid { get; }
	public string Value { get; }
	
	private ZipCode(string zipCode)
	{
		var regex = new Regex(@"^\d{5}[-\s]*\d{4}?$");
		IsValid = !string.IsNullOrWhiteSpace(zipCode) && regex.IsMatch(zipCode);
		
		if (IsValid)
			Value = Regex.Replace($"{zipCode.Substring(0, 5)}-{zipCode.Substring(5)}", @"\s+", "");
		else Value = null;
	}

	public static ZipCode FormatZipCode(string zipCode) => new ZipCode(zipCode);
}