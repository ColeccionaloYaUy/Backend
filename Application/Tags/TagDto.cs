using ColeccionaloYa.Domain.Tags;

namespace ColeccionaloYa.Application.Tags;

public record TagDto(int Id, string Name, bool IsFranchise) {
	public static TagDto From(Tag tag) =>
		new(tag.Id, tag.Name, tag.IsFranchise);
}
