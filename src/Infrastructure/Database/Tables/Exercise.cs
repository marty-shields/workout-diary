using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Core.ValueObjects.Exercise;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Tables;

[Index(nameof(Name), IsUnique = true)]
public class Exercise
{
    public int Id { get; set; }
    [MaxLength(500)]
    public required string Name { get; set; }
    public Force? Force { get; set; }
    public required Level Level { get; set; }
    public Mechanic? Mechanic { get; set; }
    public Equipment? Equipment { get; set; }
    public required List<Muscle> PrimaryMuscles { get; set; }
    public List<Muscle>? SecondaryMuscles { get; set; }
    public required List<string> Instructions { get; set; }
    public required Category Category { get; set; }
}
