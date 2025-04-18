﻿using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;

namespace SurveyBasket.Api.Services;

public class NotificationServices(
	ApplicationDbContext db,
	UserManager<ApplicationUser> userManager,
	IHttpContextAccessor httpContextAccessor,
	IEmailSender emailSender) : INotificationServices
{
	private readonly ApplicationDbContext _db = db;
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
	private readonly IEmailSender _emailSender = emailSender;

	public async Task SendNewPollsNotification(int? pollId = null)
	{
		IEnumerable<Poll> polls = [];

		if (pollId.HasValue)
		{
			var poll = await _db.Polls.SingleOrDefaultAsync(x => x.Id == pollId && x.IsPublished);

			polls = [poll!];
		}
		else
		{
			polls = await _db.Polls.
				Where(x => x.IsPublished
				&& x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
				.AsNoTracking()
				.ToListAsync();
		}

		var users = await _userManager.GetUsersInRoleAsync(DefaultRoles.Member.Name);

		var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

		foreach (var poll in polls)
		{
			foreach (var user in users)
			{
				var placeholderValues = new Dictionary<string, string>
				{
					{ "{{name}}", user.FirstName },
					{ "{{pollTill}}", poll.Title },
					{ "{{endDate}}", poll.EndsAt.ToString() },
					{ "{{url}}", $"{origin}/polls/start/{poll.Id}" } ,

				};
				var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeholderValues);

				await _emailSender.SendEmailAsync(user.Email!, $"📣 Survey Basket: New Poll - {poll.Title}", body);
			}
		}
	}
}
