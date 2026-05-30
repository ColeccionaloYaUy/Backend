namespace ColeccionaloYa.Domain.Genres;

public class Genre {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;

	internal Genre() { }

	public static Genre Create(string name) {
		return new Genre {
			Name = name.Trim(),
		};
	}

	public void Update(string name) {
		Name = name.Trim();
	}

	public void AssignId(int id) {
		Id = id;
	}
}
