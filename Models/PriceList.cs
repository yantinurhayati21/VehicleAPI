using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleAPI.Models
{
    public class PriceList
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? Code { get; set; }
        
        public int Price { get; set; }

        public int YearId { get; set; }

        public VehicleYear? Year { get; set; }

        public int ModelId { get; set; }

        public VehicleModel? Model { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}