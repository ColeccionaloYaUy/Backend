namespace ColeccionaloYa.Domain.Roles;

public class Role {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public string? Description { get; internal set; }

	internal Role() { }

	public static Role Create(string name, string? description) {
		return new Role {
			Name = name.Trim(),
			Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
		};
	}

	public void Update(string name, string? description) {
		Name = name.Trim();
		Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
	}

	public void AssignId(int id) {
		Id = id;
	}
}
