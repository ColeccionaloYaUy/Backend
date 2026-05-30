using ColeccionaloYa.Application.Tags;
using ColeccionaloYa.Domain.Tags;

namespace ColeccionaloYa.Application.Products.ProductTags;

public record ProductTagsDto(int IdProduct, List<TagDto> Tags) {
	public static ProductTagsDto From(int productId, IEnumerable<Tag> tags) =>
		new(productId, tags.Select(TagDto.From).ToList());
}
