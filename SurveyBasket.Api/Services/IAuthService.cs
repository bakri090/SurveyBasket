namespace SurveyBasket.Api.Services;

public interface IAuthServices
{
	Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
	Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
	Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
	Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
	Task<Result> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default);
	Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request);
	Task<Result> ResetPasswordRequestAsync(ResetPasswordRequest request);
	Task<Result> SendResetPasswordCodeAsync(string email);
}
