using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace GoalNexus.Api.Models;

/// <summary>
/// Represents a personal goal in the system.
/// This matches the DynamoDB table structure: 
/// UserId (PK) and GoalId (SK).
/// </summary>
[DynamoDBTable("GoalNexus_Goals")]
public class Goal
{
    [DynamoDBHashKey] // Partition Key
    [Required]
    public string UserId { get; set; } = string.Empty;

    [DynamoDBRangeKey] // Sort Key
    [Required]
    public string GoalId { get; set; } = string.Empty;

    [DynamoDBProperty]
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty]
    public bool IsCompleted { get; set; } = false;

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
