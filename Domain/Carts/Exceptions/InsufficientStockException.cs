using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Carts.Exceptions;

public sealed class InsufficientStockException : DomainException {
	public InsufficientStockException(int productId)
		: base(HttpStatusCode.Conflict,
			   "InsufficientStock",
			   $"Insufficient stock for product #{productId}.") { }
}
