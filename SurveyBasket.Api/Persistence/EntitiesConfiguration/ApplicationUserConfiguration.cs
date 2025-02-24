
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
	public void Configure(EntityTypeBuilder<ApplicationUser> builder)
	{
		builder.OwnsMany(x => x.RefreshTokens)
			.ToTable("RefreshTokens")
			.WithOwner()
			.HasForeignKey("UserId");

		builder.Property(x => x.FirstName).HasMaxLength(50);
		builder.Property(x => x.LastName).HasMaxLength(50);
	}
}
