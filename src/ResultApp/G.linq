<Query Kind="Expression" />


[HttpPost, Route("api/banktransfers/")]
public IActionResult MakeDeposit([FromBody] BookTransfer request)
   => 
   		Handle(request)
			.Map(t => validate(t))
				.Bind(Validate)
				.Bind(Save)
				.Match(
	  				Invalid: BadRequest,
	  				Valid: result => result.Match(
		 				Exception: OnFaulted,
		 				Success: _ => Ok()));



IActionResult OnFaulted(Exception ex)
{
	logger.LogError(ex.Message);
	return StatusCode(500, Errors.UnexpectedError);
}

Validation<Exceptional<Unit>> Handle(Transfer request)
   => Validate(request)
	  

Validation<BookTransfer> Validate(BookTransfer cmd)
   => ValidateBic(cmd).Bind(ValidateDate);
	  
   

Either<Error, BookTransfer> ValidateAccounnt(Transfer request)
{
	if (request has no money)
		return Errors.Invalid;
	else return request;
}

Either<Error, BookTransfer> ValidateDate(BookTransfer request)
{
	if (request.Date.Date <= now.Date)
		return Errors.TransferDateIsPast;
	else return request;
}

Either<Error, Unit> Save(BookTransfer request)
{ throw new NotImplementedException(); }