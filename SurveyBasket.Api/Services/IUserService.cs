using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services;

public interface IUserServices
{
	Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Result<UserResponse>> GetAsync(string id);
	Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
	Task<Result> AddPasswordToUserAsync(AddPasswordToUserRequest request, CancellationToken cancellationToken = default);
	Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId);
	Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
	Task<Result> TogglePublishStatusAsync(string id, CancellationToken cancellationToken);
	Task<Result> UserUnlock(string id, CancellationToken cancellationToken);
}
