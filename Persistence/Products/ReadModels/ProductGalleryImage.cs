namespace ColeccionaloYa.Persistence.Products.ReadModels;

public record ProductGalleryImage(
	int IdGallery,
	int IdProduct,
	int Order,
	string Url
);
