namespace SurveyBasket.Api.Services;

public interface ICacheServices
{
	Task<T?> GetAsync<T>(string cacheKey,CancellationToken cancellationToken = default) where T :class;
	Task SetAsync<T>(string cacheKey,T value,CancellationToken cancellationToken = default) where T :class;
	Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
}
