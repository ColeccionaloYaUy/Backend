namespace ColeccionaloYa.Domain.Addresses;

public class Address {
	public int Id { get; internal set; }
	public int ClientId { get; internal set; }
	public string Street { get; internal set; } = string.Empty;
	public string Number { get; internal set; } = string.Empty;
	public string City { get; internal set; } = string.Empty;
	public string Department { get; internal set; } = string.Empty;
	public string PostalCode { get; internal set; } = string.Empty;
	public AddressType Type { get; internal set; }
	public bool LogicalDelete { get; internal set; }

	internal Address() { }

	public static Address Create(
		int clientId,
		string street,
		string number,
		string city,
		string department,
		string postalCode,
		AddressType type
	) {
		return new Address {
			ClientId = clientId,
			Street = street.Trim(),
			Number = number.Trim(),
			City = city.Trim(),
			Department = department.Trim(),
			PostalCode = postalCode.Trim(),
			Type = type,
			LogicalDelete = false,
		};
	}

	public void Update(string street, string number, string city, string department, string postalCode, AddressType type) {
		Street = street.Trim();
		Number = number.Trim();
		City = city.Trim();
		Department = department.Trim();
		PostalCode = postalCode.Trim();
		Type = type;
	}

	public void AssignId(int id) {
		Id = id;
	}

	public void MarkDeleted() {
		LogicalDelete = true;
	}
}
