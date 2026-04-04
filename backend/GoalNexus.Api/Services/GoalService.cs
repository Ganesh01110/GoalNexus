using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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
    private readonly DynamoDBContext _context;
    private readonly ILogger<GoalService> _logger;
    private const string TableName = "GoalNexus_Goals";
    private readonly DynamoDBOperationConfig _config;

    public GoalService(IAmazonDynamoDB dynamoDb, ILogger<GoalService> logger)
    {
        _context = new DynamoDBContext(dynamoDb);
        _logger = logger;
        // Failsafe: Always override the table name to match our Terraform resource
        _config = new DynamoDBOperationConfig { OverrideTableName = TableName };
    }

    public async Task<IEnumerable<Goal>> GetGoalsAsync(string userId)
    {
        _logger.LogInformation("Fetching goals from {Table} for user: {UserId}", TableName, userId);
        
        // Use the explicit config here
        return await _context.QueryAsync<Goal>(userId, _config).GetRemainingAsync();
    }

    public async Task CreateGoalAsync(Goal goal)
    {
        _logger.LogInformation("Creating new goal in {Table}: {GoalId} for user: {UserId}", TableName, goal.GoalId, goal.UserId);
        
        // Use the explicit config here
        await _context.SaveAsync(goal, _config);
    }

    public async Task DeleteGoalAsync(string userId, string goalId)
    {
        _logger.LogInformation("Deleting goal from {Table}: {GoalId} for user: {UserId}", TableName, goalId, userId);
        
        // Use the explicit config here
        await _context.DeleteAsync<Goal>(userId, goalId, _config);
    }

    public async Task ToggleGoalStatusAsync(string userId, string goalId)
    {
        _logger.LogInformation("Toggling status in {Table} for goal: {GoalId}", TableName, goalId);
        
        // Use the explicit config here for both Load and Save
        var goal = await _context.LoadAsync<Goal>(userId, goalId, _config);
        if (goal != null)
        {
            goal.IsCompleted = !goal.IsCompleted;
            await _context.SaveAsync(goal, _config);
        }
    }
}
