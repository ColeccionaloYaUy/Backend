namespace ColeccionaloYa.Domain.Tags;

public class Tag {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public bool IsFranchise { get; internal set; }

	internal Tag() { }

	public static Tag Create(string name, bool isFranchise) {
		return new Tag {
			Name = name.Trim(),
			IsFranchise = isFranchise,
		};
	}

	public void Update(string name, bool isFranchise) {
		Name = name.Trim();
		IsFranchise = isFranchise;
	}

	public void AssignId(int id) {
		Id = id;
	}
}
