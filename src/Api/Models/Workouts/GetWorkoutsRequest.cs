using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Workouts;

public class GetWorkoutsRequest
{
    [FromQuery]
    [Range(1, 100)]
    public int PageSize { get; set; } = 100;

    [FromQuery]
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}
