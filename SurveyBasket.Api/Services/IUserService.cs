using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId);
	Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
