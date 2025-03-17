namespace SurveyBasket.Api.Abstractions.Consts;

public static  class RegexPatterns
{
	public static string Password = "(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}";
}
