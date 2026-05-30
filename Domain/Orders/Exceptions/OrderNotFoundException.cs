using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Orders.Exceptions;

public sealed class OrderNotFoundException : DomainException {
	public OrderNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "OrderNotFound",
			   $"Order #{id} not found.") { }
}
