using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BusinessLayer.Models
{
    public abstract class ModelBase
    {
        [Key]
        public int id { get; set; }
    }
}
