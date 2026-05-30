using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Stock.Exceptions;

public sealed class InvalidStockTypeException : DomainException {
	public InvalidStockTypeException(string type)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidStockType",
			   $"Stock movement type '{type}' is invalid; allowed values are 'entry', 'exit' or 'adjustment'.") { }
}
