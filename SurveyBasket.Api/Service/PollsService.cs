
using Microsoft.EntityFrameworkCore;

namespace SurveyBasket.Api.Service
{
    public class PollsService(ApplicationDbContext db) : IPollService
    {
        private readonly ApplicationDbContext _db = db;
       
        public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
			await _db.Polls.AsNoTracking().ToListAsync(cancellationToken);
        
        public async Task<Poll?> GetAsync(int id,CancellationToken cancellationToken = default) => 
			await _db.Polls.FindAsync(id,cancellationToken);

		public async Task<Poll> AddAsync(Poll poll,CancellationToken cancellationToken =default)
		{
			await _db.Polls.AddAsync(poll,cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			return poll;
		}

		public async Task<bool> UpdateAsync(int id, Poll poll,CancellationToken cancellationToken = default)
		{
			var currentPoll = await GetAsync(id, cancellationToken);
			if (currentPoll is null)
			{
				return false;
			}

			currentPoll.Title = poll.Title;
			currentPoll.Summary = poll.Summary;
			currentPoll.StartsAt = poll.StartsAt;
			currentPoll.EndsAt = poll.EndsAt;

			await _db.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			var poll = await GetAsync(id, cancellationToken); 
			
			if (poll is null)
				return false; 

			_db.Remove(poll);
			
			await _db.SaveChangesAsync(cancellationToken);

			return true;
		}
		public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
		{
			var poll = await GetAsync(id, cancellationToken);

			if (poll is null)
				return false;

			poll.IsPublished = !poll.IsPublished;

			await _db.SaveChangesAsync(cancellationToken);

			return true;
		}

	}
	}
