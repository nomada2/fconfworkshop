<Query Kind="Program">
  <Reference Relative="ResultApp\bin\Debug\ResultApp.exe">C:\Temp\ResultApp\ResultApp\bin\Debug\ResultApp.exe</Reference>
  <Namespace>ResultApp</Namespace>
  <Namespace>ResultApp.Functional.Result</Namespace>
</Query>

Result<Exception, string> FormatZipCode(string zipCode)
{
	var regex = new Regex(@"^\d{5}[-\s]*\d{4}?$");
	if (string.IsNullOrWhiteSpace(zipCode) || !regex.IsMatch(zipCode))
		return new Exception("Zip code bad format");
	return Regex.Replace($"{zipCode.Substring(0, 5)}-{zipCode.Substring(5)}", @"\s+", "");
}

Result<Exception, string> toState(string zipCode)
{
	if (string.IsNullOrWhiteSpace(zipCode)) return new Exception("Invalid State");
	return "MD";
}

void Main()
{
	Result<Exception, string> zipCode = FormatZipCode("20745    0001");

	Result<Exception, string> badZipCode = FormatZipCode("Invalid");

	zipCode.Match(
		(ex) => ex.Message.Dump(),
		z => z.Dump()
		);

	badZipCode.Match(
			(ex) => ex.Message.Dump(),
			z => z.Dump()
	);

	var state = zipCode.Map(z => toState(z));

	state.Dump();
}