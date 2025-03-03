﻿using Microsoft.EntityFrameworkCore;

namespace SurveyBasket.Api.Service
{
    public class PollService(ApplicationDbContext db) : IPollService
    {
        private readonly ApplicationDbContext _db = db;
       
        public async Task<IEnumerable<Result<PollResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var polls = await _db.Polls.AsNoTracking().ToListAsync(cancellationToken);
			var pollsResponse = polls.Select(x => Result.Success(x.Adapt<PollResponse>()));
			return pollsResponse;
		}
        
        public async Task<Result<PollResponse>> GetAsync(int id,CancellationToken cancellationToken = default)
		{
			var pool = await _db.Polls.FindAsync(id,cancellationToken);
			return pool is not null 
				? Result.Success(pool.Adapt<PollResponse>())
				: Result.Failure<PollResponse>(PollError.PollNotFound);
		}

		public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
		{
			var isExistingTitle = await _db.Polls.AnyAsync(x => x.Title == request.Title, cancellationToken);
				if (isExistingTitle)
				return Result.Failure<PollResponse>(PollError.DuplicatedPollTitle);

			var poll = request.Adapt<Poll>();

			await _db.AddAsync(poll, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);

			return Result.Success(poll.Adapt<PollResponse>());
		}

		public async Task<Result> UpdateAsync(int id, PollRequest request,CancellationToken cancellationToken = default)
		{
			var isExisted = await _db.Polls.AnyAsync(x => x.Title == request.Title && x.Id != id, cancellationToken);

			if (isExisted)
				return Result.Failure<PollResponse>(PollError.DuplicatedPollTitle);

			var currentPoll = await _db.Polls.FindAsync(id, cancellationToken);

			if (currentPoll is null)
				return Result.Failure(PollError.PollNotFound);

			currentPoll.Title = request.Title;
			currentPoll.Summary = request.Summary;
			currentPoll.StartsAt = request.StartsAt;
			currentPoll.EndsAt = request.EndsAt;

			await _db.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}

		public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			var poll = await _db.Polls.FindAsync(id, cancellationToken); 
			
			if (poll is null)
				return Result.Failure(PollError.PollNotFound); 

			_db.Remove(poll);
			
			await _db.SaveChangesAsync(cancellationToken);

			return Result.Success();
		}
		public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
		{
			var poll = await _db.Polls.FindAsync(id);

			if (poll is null)
				return Result.Failure(PollError.PollNotFound);

			poll.IsPublished = !poll.IsPublished;

			await _db.SaveChangesAsync(cancellationToken);

			return Result.Success();
		}

	}
	}
