using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Addresses.Exceptions;

public sealed class AddressAccessDeniedException : DomainException {
	public AddressAccessDeniedException()
		: base(HttpStatusCode.Forbidden,
			   "AddressAccessDenied",
			   "You do not have permission to access this address.") { }
}
