namespace ColeccionaloYa.Persistence.Dashboard.ReadModels;

public record SummaryTotals(decimal TotalSales, int OrdersCount, int NewClients, int TotalUnitsSold, int ActiveClients);

public record SalesBucket(DateOnly Date, decimal Total, decimal Subtotal, decimal Taxes, int Orders, int Units);

public record TopProductRow(int IdProduct, string Name, string Type, string? CoverUrl, decimal Price, int QuantitySold, decimal Revenue, int OrdersCount, int CurrentStock);

public record TopClientRow(int IdClient, string Name, string Lastname, string Email, decimal TotalSpent, int OrdersCount, int UnitsPurchased, DateOnly? LastOrderDate, DateOnly ClientSince);

public record OrdersByStatusRow(string Status, int Count, decimal TotalValue);

public record StockAlertRow(int IdProduct, string Name, string Type, string? CoverUrl, int CurrentStock, decimal AvgDailySales);

public record CatalogSummaryRow(
	int TotalProducts, int TotalBooks, int TotalPacks, int TotalObjects, int TotalComics, int TotalMangas,
	int FeaturedProducts, int ProductsWithDiscount, int ProductsWithoutImage, int TotalAuthors, int TotalGenres,
	int TotalFranchises, int TotalTags, decimal AvgPrice);

public record RealtimeSummaryRow(
	int OrdersLast24h, decimal RevenueLast24h, int NewClientsLast24h, int ActiveCartsNow,
	int PendingOrders, int ProcessingOrders, int ShippedOrders, int PendingPayments, int StockAlerts);

public record RealtimeLastOrder(int IdOrder, string ClientName, decimal Total, DateOnly CreationDate);
