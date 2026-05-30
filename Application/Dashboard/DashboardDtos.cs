using System.Globalization;
using ColeccionaloYa.Persistence.Dashboard.ReadModels;

namespace ColeccionaloYa.Application.Dashboard;

public record DashboardSummaryDto(
	decimal TotalSales,
	int OrdersCount,
	int NewClients,
	decimal AvgTicket,
	int TotalUnitsSold,
	int ActiveClients,
	DateOnly DateFrom,
	DateOnly DateTo
);

public record SalesSeriesPointDto(DateOnly Date, string Label, decimal Total, decimal Subtotal, decimal Taxes, int Orders, int Units);

public record SalesTotalsDto(decimal Total, int Orders, int Units, decimal AvgPerBucket);

public record SalesSeriesDto(string Period, List<SalesSeriesPointDto> Series, SalesTotalsDto Totals);

public record TopProductDto(int IdProduct, string Name, string Type, string? CoverUrl, decimal Price, int QuantitySold, decimal Revenue, int OrdersCount, int CurrentStock) {
	public static TopProductDto From(TopProductRow r) =>
		new(r.IdProduct, r.Name, r.Type, r.CoverUrl, r.Price, r.QuantitySold, r.Revenue, r.OrdersCount, r.CurrentStock);
}

public record TopProductsDto(List<TopProductDto> Data);

public record TopClientDto(int IdClient, string Name, string Lastname, string Email, decimal TotalSpent, int OrdersCount, decimal AvgTicket, int UnitsPurchased, DateOnly? LastOrderDate, DateOnly ClientSince) {
	public static TopClientDto From(TopClientRow r) =>
		new(r.IdClient, r.Name, r.Lastname, r.Email, r.TotalSpent, r.OrdersCount,
			r.OrdersCount == 0 ? 0 : Math.Round(r.TotalSpent / r.OrdersCount, 2), r.UnitsPurchased, r.LastOrderDate, r.ClientSince);
}

public record TopClientsDto(List<TopClientDto> Data);

public record OrdersByStatusItemDto(string Status, int Count, decimal TotalValue, decimal Percentage);

public record OrdersByStatusDto(List<OrdersByStatusItemDto> Data, int TotalOrders);

public record StockAlertDto(int IdProduct, string Name, string Type, string? CoverUrl, int CurrentStock, int Threshold, decimal AvgDailySales, int? DaysUntilStockout, string Severity);

public record StockAlertsDto(List<StockAlertDto> Data, int TotalAlerts);

public record CatalogSummaryDto(
	int TotalProducts, int TotalBooks, int TotalPacks, int TotalObjects, int TotalComics, int TotalMangas,
	int FeaturedProducts, int ProductsWithDiscount, int ProductsWithoutImage, int TotalAuthors, int TotalGenres,
	int TotalFranchises, int TotalTags, decimal AvgPrice
) {
	public static CatalogSummaryDto From(CatalogSummaryRow r) =>
		new(r.TotalProducts, r.TotalBooks, r.TotalPacks, r.TotalObjects, r.TotalComics, r.TotalMangas,
			r.FeaturedProducts, r.ProductsWithDiscount, r.ProductsWithoutImage, r.TotalAuthors, r.TotalGenres,
			r.TotalFranchises, r.TotalTags, Math.Round(r.AvgPrice, 2));
}

public record RealtimeLastOrderDto(int IdOrder, string ClientName, decimal Total, DateOnly CreationDate);

public record RealtimeSummaryDto(
	int OrdersLast24h, decimal RevenueLast24h, int NewClientsLast24h, int ActiveCartsNow,
	int PendingOrders, int ProcessingOrders, int ShippedOrders, int PendingPayments, int StockAlerts,
	RealtimeLastOrderDto? LastOrder
);

public static class SalesLabel {
	public static string For(string period, DateOnly date) => period switch {
		"week" => $"Sem {ISOWeek(date)} {date.Year}",
		"month" => date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
		_ => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
	};

	private static int ISOWeek(DateOnly date) =>
		System.Globalization.ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));
}
