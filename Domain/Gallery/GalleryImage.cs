namespace ColeccionaloYa.Domain.Gallery;

public class GalleryImage {
	public int Id { get; internal set; }
	public int ProductId { get; internal set; }
	public int Order { get; internal set; }
	public string Url { get; internal set; } = string.Empty;

	internal GalleryImage() { }

	public static GalleryImage Create(int productId, int order, string url) {
		return new GalleryImage {
			ProductId = productId,
			Order = order,
			Url = url.Trim(),
		};
	}

	public void AssignId(int id) {
		Id = id;
	}
}
