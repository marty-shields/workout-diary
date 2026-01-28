using System.ComponentModel.DataAnnotations;
using Core.ValueObjects.Exercise;

namespace Infrastructure.Database.Tables;

public class Exercise
{
    public required Guid Id { get; set; }
    [MaxLength(500)]
    public required string Name { get; set; }
    public Force? Force { get; set; }
    public required Level Level { get; set; }
    public Mechanic? Mechanic { get; set; }
    public Equipment? Equipment { get; set; }
    public required IEnumerable<Muscle> PrimaryMuscles { get; set; }
    public IEnumerable<Muscle>? SecondaryMuscles { get; set; }
    public required IEnumerable<string> Instructions { get; set; }
    public required Category Category { get; set; }
}
