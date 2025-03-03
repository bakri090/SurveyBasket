﻿
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
	public void Configure(EntityTypeBuilder<Answer> builder)
	{
		builder.HasIndex(x => new { x.Id, x.Content }).IsUnique();
		builder.Property(x => x.Content).HasMaxLength(1000);
	}
}
