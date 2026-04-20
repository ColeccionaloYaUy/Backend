namespace ColeccionaloYa.Domain.Clients;

public class Client {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public string Lastname { get; internal set; } = string.Empty;
	public string Email { get; internal set; } = string.Empty;
	public string? Phone { get; internal set; }
	public string PasswordHash { get; internal set; } = string.Empty;
	public int? AddressDeliveryId { get; internal set; }
	public int? AddressOrderId { get; internal set; }
	public int RoleId { get; internal set; }
	public string RoleName { get; internal set; } = string.Empty;
	public bool Active { get; internal set; }
	public DateTime CreationDate { get; internal set; }
	public bool LogicalDelete { get; internal set; }

	internal Client() { }

	public static Client Register(string name, string lastname, string email, string passwordHash, int roleId, string roleName) {
		return new Client {
			Name = name,
			Lastname = lastname,
			Email = email,
			PasswordHash = passwordHash,
			RoleId = roleId,
			RoleName = roleName,
			Active = true,
			CreationDate = DateTime.UtcNow,
			LogicalDelete = false,
		};
	}

	public void AssignId(int id) {
		Id = id;
	}

	public void ChangePasswordHash(string newHash) {
		PasswordHash = newHash;
	}
}
