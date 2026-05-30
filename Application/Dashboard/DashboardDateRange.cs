namespace ColeccionaloYa.Application.Dashboard;

public static class DashboardDateRange {
	public static (DateOnly From, DateOnly To) Resolve(DateOnly? from, DateOnly? to) {
		var effectiveTo = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
		var effectiveFrom = from ?? effectiveTo.AddDays(-30);
		return (effectiveFrom, effectiveTo);
	}
}
