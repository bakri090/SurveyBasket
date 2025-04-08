namespace SurveyBasket.Api.Services;

public interface INotificationServices
{
	Task SendNewPollsNotification(int? pollId = null);
}
