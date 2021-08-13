using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BusinessLayer.Models
{
    public class ImageModel : ModelBase
    {


        [Column(TypeName = "nvarchar(100)")]
        public string ImageName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        [NotMapped]
        public string ImageSrc { get; set; }
    }
}
