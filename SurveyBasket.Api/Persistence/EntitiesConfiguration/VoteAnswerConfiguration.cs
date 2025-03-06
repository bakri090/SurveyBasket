
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class VoteanswerConfiguration : IEntityTypeConfiguration<VoteAnswer>
{
	public void Configure(EntityTypeBuilder<VoteAnswer> builder)
	{
		builder.HasIndex(x => new {x.VoteId, x.QuestionId }).IsUnique();
	}
}
