﻿using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SurveyBasket.Api.Services;

public class CacheServices(IDistributedCache distributedCache, ILogger<CacheServices> logger) : ICacheServices
{
	private readonly IDistributedCache _distributedCache = distributedCache;
	private readonly ILogger<CacheServices> _logger = logger;

	public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default) where T : class
	{
		_logger.LogInformation("Get cache with key: {key}", cacheKey);

		var cacheValue = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
		return string.IsNullOrEmpty(cacheValue) ? null : JsonSerializer.Deserialize<T>(cacheValue);
	}
	public async Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default) where T : class
	{
		_logger.LogInformation("Set cache with key: {key}", cacheKey);
		await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(value), cancellationToken);
	}

	public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Remove cache with key: {key}", cacheKey);
		await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
	}

}
