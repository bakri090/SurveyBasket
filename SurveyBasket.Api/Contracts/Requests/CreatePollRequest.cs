using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Api.Contracts.Requests
{
	public record CreatePollRequest(
		string Title,string Description);
}
