using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Addresses.Exceptions;

public sealed class AddressNotFoundException : DomainException {
	public AddressNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "AddressNotFound",
			   $"Address #{id} not found.") { }
}
