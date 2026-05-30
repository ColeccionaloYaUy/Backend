using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Stock.Exceptions;

public sealed class InvalidStockSignException : DomainException {
	public InvalidStockSignException(StockMovementType type)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidStockSign",
			   $"The quantity sign does not match the movement type '{type.ToString().ToLowerInvariant()}'.") { }
}
