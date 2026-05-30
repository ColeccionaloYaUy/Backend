namespace ColeccionaloYa.Persistence.Products.ReadModels;

public record ProductStockInfo(
	int CurrentStock,
	DateTime? LastMovementDate
);
