using Tourney.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.Configure<RouteOptions>(options =>
{
	options.LowercaseUrls = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
	//options.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/openapi/v1.json", "TourneyAPI V1");
	});
}

Dictionary<Guid, Tournament> tournaments = new Dictionary<Guid, Tournament>();
Tournament? currentTournament = null;

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();


