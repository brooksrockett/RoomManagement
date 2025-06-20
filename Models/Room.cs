using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RoomManagement.Models;

[Table("Rooms")]
public class Room
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int Capacity { get; set; }
    public int? SupervisingUser { get; set; }
    public string? Options { get; set; } = "";
}
