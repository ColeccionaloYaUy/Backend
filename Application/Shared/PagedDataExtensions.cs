using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Application.Shared;

public static class PagedDataExtensions {
	public static PagedResult<TDto> ToPagedResult<TDomain, TDto>(
		this PagedData<TDomain> data, int pageSize, Func<TDomain, TDto> map) {
		return new PagedResult<TDto>(data.Items.Select(map).ToList(), data.EffectivePage, pageSize, data.TotalCount);
	}
}
