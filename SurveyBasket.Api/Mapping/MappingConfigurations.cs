﻿namespace SurveyBasket.Api.Mapping
{
	public class MappingConfigurations : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Poll, PollResponse>()
				.Map(dest => dest.Description, src => src.Description);
		}
	}
}
