using Amazon.DynamoDBv2;
using GoalNexus.Api.Models;
using GoalNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Hybrid Bridge: Allows app to run natively on AWS Lambda Serverless OR Local Kestrel
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);


// Configure AWS Options
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();

// Register Custom Services
builder.Services.AddScoped<IGoalService, GoalService>();

// Enable CORS for React development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// --- API Endpoints ---

var goalsGroup = app.MapGroup("/api/goals");

// GET: Fetch all goals for a specific user
goalsGroup.MapGet("/{userId}", async (string userId, IGoalService goalService, ILogger<Program> logger) =>
{
    logger.LogInformation("Request: GET /api/goals/{UserId}", userId);
    var goals = await goalService.GetGoalsAsync(userId);
    return Results.Ok(goals);
});

// POST: Create a new goal
goalsGroup.MapPost("/", async ([FromBody] Goal goal, IGoalService goalService, ILogger<Program> logger) =>
{
    if (string.IsNullOrEmpty(goal.UserId) || string.IsNullOrEmpty(goal.Title))
    {
        return Results.BadRequest("UserId and Title are required.");
    }

    goal.GoalId = Guid.NewGuid().ToString();
    goal.CreatedAt = DateTime.UtcNow;

    logger.LogInformation("Request: POST /api/goals | New Goal: {Title}", goal.Title);
    await goalService.CreateGoalAsync(goal);
    return Results.Created($"/api/goals/{goal.UserId}/{goal.GoalId}", goal);
});

// PATCH: Toggle completion status
goalsGroup.MapPatch("/{userId}/{goalId}/toggle", async (string userId, string goalId, IGoalService goalService, ILogger<Program> logger) =>
{
    logger.LogInformation("Request: PATCH /api/goals/{UserId}/{GoalId}/toggle", userId, goalId);
    await goalService.ToggleGoalStatusAsync(userId, goalId);
    return Results.NoContent();
});

// DELETE: Remove a goal
goalsGroup.MapDelete("/{userId}/{goalId}", async (string userId, string goalId, IGoalService goalService, ILogger<Program> logger) =>
{
    logger.LogInformation("Request: DELETE /api/goals/{UserId}/{GoalId}", userId, goalId);
    await goalService.DeleteGoalAsync(userId, goalId);
    return Results.NoContent();
});

app.Run();
