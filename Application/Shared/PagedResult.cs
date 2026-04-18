namespace API.Application.Shared;

public record PagedResult<T>(
	List<T> Items,
	int Page,
	int PageSize,
	int TotalCount) {
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
