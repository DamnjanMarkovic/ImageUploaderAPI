using BusinessLayer.Models;
using DataContext.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUploaderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImageController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        // GET: api/Image
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageModel>>> GetImages()
        {
            return await _context.Images
              .Select(x => new ImageModel()
              {
                  id = x.id,
                  ImageName = x.ImageName,
                  ImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, x.ImageName)
              })
              .ToListAsync();
        }


        // GET: api/Image/5
        [HttpGet("{Id}")]
        public async Task<ActionResult<ImageModel>> GetImageModel(int id)
        {
            var imageModel = await _context.Images.FindAsync(id);

            if (imageModel == null)
            {
                return NotFound();
            }

            return imageModel;
        }

        // PUT: api/Image/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{Id}")]
        public async Task<IActionResult> PutImageModel(int id, [FromForm] ImageModel imageModel)
        {
            if (id != imageModel.id)
            {
                return BadRequest();
            }

            if (imageModel.ImageFile != null)
            {
                DeleteImage(imageModel.ImageName);
                imageModel.ImageName = await SaveImage(imageModel.ImageFile);
            }

            _context.Entry(imageModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        


        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<ActionResult<ImageModel>> PostImageModel(List<IFormFile> iFormFiles)
        {
            string returning = "";
            foreach (var iFormFile in iFormFiles)
            {
                ImageModel imageModel = new ImageModel();
                

                if (ImageWithThatNameExists(iFormFile.FileName))
                {
                    returning = iFormFile.FileName;
                }
                else
                {
                    
                    imageModel.ImageName = await SaveImage(iFormFile);
                    _context.Images.Add(imageModel);
                    await _context.SaveChangesAsync();
                    
                }

                
            }

            if (!string.IsNullOrEmpty(returning))
            {
                return new ContentResult
                {
                    StatusCode = 201,
                    Content = $"Image ${returning} already exists!",
                    ContentType = "text/plain"
                };
            }
            else
            {
                return Ok(201);
            }
            
        }

        private bool ImageWithThatNameExists(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
            {
                return true;
            }

            return false;

        }

        // DELETE: api/Image/5
        [HttpDelete("{Id}")]
        public async Task<ActionResult<ImageModel>> DeleteImageModel(int id)
        {
            var imageModel = await _context.Images.FindAsync(id);
            if (imageModel == null)
            {
                return NotFound();
            }
            DeleteImage(imageModel.ImageName);
            _context.Images.Remove(imageModel);
            await _context.SaveChangesAsync();

            return imageModel;
        }

        private bool ImageModelExists(int id)
        {
                return _context.Images.Any(e => e.id == id);
        }

        [NonAction]
        public async Task<string> SaveImage(IFormFile imageFile)
        {

            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageFile.FileName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageFile.FileName;
        }

        [NonAction]
        public void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
    }
}