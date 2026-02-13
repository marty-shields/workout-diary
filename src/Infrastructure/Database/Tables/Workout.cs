using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Tables;

[Index(nameof(WorkoutDate))]
[Index(nameof(UserId))]
[PrimaryKey(nameof(Id), nameof(UserId))]
public class Workout
{
    public required Guid Id { get; set; }
    public required string UserId { get; set; }
    public string? Notes { get; set; }
    public required int TotalDurationMinutes { get; set; }
    public required DateTimeOffset WorkoutDate { get; set; }
    public IEnumerable<WorkoutActivity> WorkoutActivities { get; } = new List<WorkoutActivity>();
}
