using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace SurveyBasket.Api.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IHttpContextAccessor httpContext) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
	private readonly IHttpContextAccessor _httpContext = httpContext;
	public DbSet<Answer> Answers { get; set; }
	public DbSet<Poll> Polls { get; set; }
	public DbSet<Question> Questions { get; set; }
	public DbSet<Vote> Votes { get; set; }
	public DbSet<VoteAnswer> VoteAnswers { get; set; }
	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//{
	//	//optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
	//	base.OnConfiguring(optionsBuilder);
	//}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		var cascade = modelBuilder.Model
			.GetEntityTypes()
			.SelectMany(t => t.GetForeignKeys())
			.Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

		foreach (var fk in cascade)
			fk.DeleteBehavior = DeleteBehavior.Restrict;
		

		base.OnModelCreating(modelBuilder);
	}
	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entries = ChangeTracker.Entries<AuditableEntity>();

		foreach (var entityEntry in entries)
		{
			var currentUserId = _httpContext.HttpContext?.User.GetUserId()!;
			if (entityEntry.State == EntityState.Added)
			{
				entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId;
			}else if (entityEntry.State == EntityState.Modified)
			{
				entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
				entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
			}
			
		}
		return base.SaveChangesAsync(cancellationToken);
	}
}
