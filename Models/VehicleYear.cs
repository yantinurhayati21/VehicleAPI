using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleAPI.Models
{
    public class VehicleYear
    {
        public int Id { get; set; }

        [Range(1886, 2024, ErrorMessage = "Year must be between 1886 and 2024")]
        public int Year { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}