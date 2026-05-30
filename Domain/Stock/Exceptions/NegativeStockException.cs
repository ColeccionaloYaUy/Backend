using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Stock.Exceptions;

public sealed class NegativeStockException : DomainException {
	public NegativeStockException(int productId)
		: base(HttpStatusCode.Conflict,
			   "NegativeStock",
			   $"The movement would result in negative stock for product #{productId}.") { }
}
