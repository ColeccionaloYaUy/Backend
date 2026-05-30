namespace ColeccionaloYa.Domain.Authors;

public class Author {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;

	internal Author() { }

	public static Author Create(string name) {
		return new Author {
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
