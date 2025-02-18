using SurveyBasket.Api.Middleware;
using SurveyBasket.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDependencies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
  //  app.MapOpenApi();
//    app.MapScalarApiReference();
}
app.UseCustomMiddleware();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
