<Query Kind="Program" />


static void Main()
{
	ZipCode zipCode = ZipCode.FormatZipCode("20745    0001");
	
	zipCode.Match(
		() => "Error".Dump(),
		(z) => z.Dump()
		);
}


class ZipCode : IsValidType<string>
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

interface IsValidType<T>
{
	bool IsValid { get; }
	T Value { get; }
}

static class ValidTypeExt
{
	public static TR Match<T, TR>(this IsValidType<T> @this, Func<TR> Invalid, Func<T, TR> valid)
	   => @this.IsValid ? valid(@this.Value) : Invalid();
}