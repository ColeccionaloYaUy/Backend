namespace API.Persistence.Shared;

public record PagedData<T>(List<T> Items, int TotalCount, int EffectivePage);
