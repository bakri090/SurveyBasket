namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
	public void Configure(EntityTypeBuilder<ApplicationRole> builder)
	{

		builder.HasData([
			new ApplicationRole
			{
				Id = DefaultRoles.Admin.RoleId,
				Name = DefaultRoles.Admin.Name,
				NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
				ConcurrencyStamp = DefaultRoles.Admin.RoleConcurrencyStamp
			},
			new ApplicationRole
			{
				Id = DefaultRoles.Member.RoleId,
				Name = DefaultRoles.Member.Name,
				NormalizedName = DefaultRoles.Member.Name.ToUpper(),
				ConcurrencyStamp = DefaultRoles.Member.RoleConcurrencyStamp,
				IsDefault = true
			}
			]);
	}
}
