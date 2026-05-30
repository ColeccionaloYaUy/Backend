using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Stock.Exceptions;

public sealed class InvalidStockQuantityException : DomainException {
	public InvalidStockQuantityException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidStockQuantity",
			   "Stock movement quantity cannot be zero.") { }
}
