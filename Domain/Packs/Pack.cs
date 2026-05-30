namespace ColeccionaloYa.Domain.Packs;

public class Pack {
	public int ProductId { get; internal set; }
	public int ProductCount { get; internal set; }
	public DateTime CreationDate { get; internal set; }

	internal Pack() { }

	public static Pack Create(int productId, int productCount) {
		return new Pack {
			ProductId = productId,
			ProductCount = productCount,
			CreationDate = DateTime.UtcNow,
		};
	}
}
