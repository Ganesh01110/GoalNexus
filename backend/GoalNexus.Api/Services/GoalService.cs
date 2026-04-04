using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GoalNexus.Api.Models;

namespace GoalNexus.Api.Services;

public interface IGoalService
{
    Task<IEnumerable<Goal>> GetGoalsAsync(string userId);
    Task CreateGoalAsync(Goal goal);
    Task DeleteGoalAsync(string userId, string goalId);
    Task ToggleGoalStatusAsync(string userId, string goalId);
}

public class GoalService : IGoalService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDBContext _context;
    private readonly ILogger<GoalService> _logger;
    private const string TableName = "GoalNexus_Goals";

    public GoalService(IAmazonDynamoDB dynamoDb, ILogger<GoalService> logger)
    {
        _dynamoDb = dynamoDb;
        _context = new DynamoDBContext(dynamoDb); // Note: In newer SDKs, preference is for builder, but this is fine for now if initialized correctly.
        _logger = logger;
    }

    public async Task<IEnumerable<Goal>> GetGoalsAsync(string userId)
    {
        _logger.LogInformation("Fetching goals for user: {UserId}", userId);
        
        var conditions = new List<ScanCondition>
        {
            new ScanCondition("UserId", ScanOperator.Equal, userId)
        };

        return await _context.QueryAsync<Goal>(userId).GetRemainingAsync();
    }

    public async Task CreateGoalAsync(Goal goal)
    {
        _logger.LogInformation("Creating new goal: {GoalId} for user: {UserId}", goal.GoalId, goal.UserId);
        await _context.SaveAsync(goal);
    }

    public async Task DeleteGoalAsync(string userId, string goalId)
    {
        _logger.LogInformation("Deleting goal: {GoalId} for user: {UserId}", goalId, userId);
        await _context.DeleteAsync<Goal>(userId, goalId);
    }

    public async Task ToggleGoalStatusAsync(string userId, string goalId)
    {
        _logger.LogInformation("Toggling status for goal: {GoalId}", goalId);
        
        var goal = await _context.LoadAsync<Goal>(userId, goalId);
        if (goal != null)
        {
            goal.IsCompleted = !goal.IsCompleted;
            await _context.SaveAsync(goal);
        }
    }
}
