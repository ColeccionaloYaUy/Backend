using System.ComponentModel.DataAnnotations;
using ColeccionaloYa.Domain.Addresses;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Addresses;

public class AddressRequest {
	[Required]
	[MaxLength(150)]
	public string Street { get; set; } = string.Empty;

	[Required]
	[MaxLength(20)]
	public string Number { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string City { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string Department { get; set; } = string.Empty;

	[Required]
	[MaxLength(20)]
	public string PostalCode { get; set; } = string.Empty;

	[Required]
	public AddressType Type { get; set; }
}
