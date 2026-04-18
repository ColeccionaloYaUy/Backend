using API.Persistence.Shared;

namespace API.Application.Shared;

public static class PagedDataExtensions {
	public static PagedResult<TDto> ToPagedResult<TDomain, TDto>(
		this PagedData<TDomain> data, int pageSize, Func<TDomain, TDto> map) {
		return new PagedResult<TDto>(data.Items.Select(map).ToList(), data.EffectivePage, pageSize, data.TotalCount);
	}
}
