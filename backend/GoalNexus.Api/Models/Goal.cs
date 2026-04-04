using System.ComponentModel.DataAnnotations;

namespace GoalNexus.Api.Models;

/// <summary>
/// Represents a personal goal in the system.
/// This matches the DynamoDB table structure: 
/// UserId (PK) and GoalId (SK).
/// </summary>
public class Goal
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string GoalId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
