namespace API.Application.Shared;

public interface IPagedQuery {
	int Page { get; }
	int PageSize { get; }
}
