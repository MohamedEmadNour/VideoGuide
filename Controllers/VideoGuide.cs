using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Runtime.CompilerServices;
using VideoGuide.Models;
using VideoGuide.View_Model;

namespace VideoGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoGuide : ControllerBase
    {
        private readonly VideoGuideContext _context;
        private readonly IMapper _mapper;
        public VideoGuide(VideoGuideContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("Get_Groups")]
        //[Authorize(Roles ="User,Admin")]
        public async Task<IActionResult> Get_Groups()
        {
            var groupData = await _context.Groups.Where(w=>w.visable == true)
            .Select(s => new { s.Local_GroupName, s.Lantin_GroupName, s.Group_Photo_Location })
            .ToListAsync();

            // After the data is retrieved, then load the images
            var groupsDTO =  groupData.Select(async s => new Get_GroupsDTO
            {
                Local_GroupName = s.Local_GroupName ?? string.Empty,
                Lantin_GroupName = s.Lantin_GroupName ?? string.Empty,
                Image = await SendImage(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                Group_Photo_Location = s.Group_Photo_Location ?? string.Empty
            }).Select(s=>s.Result).ToList();
            return Ok(groupsDTO);
        }
        public static IDictionary<string, string> GetAllImageMimeTypes()
        {
            return new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".pjpeg", "image/pjpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".webp", "image/webp" },
            { ".tiff", "image/tiff" },
            { ".tif", "image/tiff" }, // TIFF has two common file extensions
            { ".svg", "image/svg+xml" },
            { ".bmp", "image/bmp" },
            { ".ico", "image/vnd.microsoft.icon" },
            { ".heif", "image/heif" },
            { ".heic", "image/heic" },
            // Add more MIME types as needed
        };
        }
        [NonAction]
        public async Task<FileContentResult> SendImage(string filename)
        {
            // Define the path to the file
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", filename);
            if (!System.IO.File.Exists(imagePath))
            {
                // If the file is not found, return an appropriate response, such as a 404
                return null;
            }
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            var mimeTypes = GetAllImageMimeTypes();
            var contentType = mimeTypes.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";

            var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            return new FileContentResult(imageBytes, contentType);
        }
        [HttpPost("Insert_Groups") , DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Insert_Groups([FromForm]Insert_GroupsDTO Insert_GroupsDTO)
        {
            // Create a new name for the file
            string fileName = Path.GetRandomFileName() + Path.GetExtension(Insert_GroupsDTO.Image?.FileName);
            var folderName = Path.Combine(Directory.GetCurrentDirectory(),"Resources", "Images");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
            var dbPath = Path.Combine(pathToSave, fileName); //you can add this path to a list and then return all dbPaths to the client if require

            // Save the file
            using (var fileStream = new FileStream(dbPath, FileMode.Create))
            {
                await Insert_GroupsDTO.Image?.CopyToAsync(fileStream);
            }
            Group group = new Group
            {
                Local_GroupName = Insert_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Insert_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = dbPath
            };
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            return Accepted();
        }
        [HttpPut("Update_Groups"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update_Groups([FromForm] Update_GroupsDTO Update_GroupsDTO)
        {
            string filepath = _context.Groups.FirstOrDefaultAsync(w => w.GroupID == Update_GroupsDTO.GroupID).Result?.Group_Photo_Location?? string.Empty ;
            Group group = new Group();
            if (filepath!= Update_GroupsDTO.Group_Photo_Location && Update_GroupsDTO.Image != null)
            {

            // Create a new name for the file
            string fileName = Path.GetRandomFileName() + Path.GetExtension(Update_GroupsDTO.Image?.FileName);
            var folderName = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
            var dbPath = Path.Combine(pathToSave, fileName); //you can add this path to a list and then return all dbPaths to the client if require

            // Save the file
            using (var fileStream = new FileStream(dbPath, FileMode.Create))
            {
                await Update_GroupsDTO.Image?.CopyToAsync(fileStream);
            }
            group = new Group
            {
                GroupID = Update_GroupsDTO.GroupID,
                Local_GroupName = Update_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Update_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = dbPath,
                visable = Update_GroupsDTO.visable
            };
                System.IO.File.Delete(filepath);

            }
            else
            {
            group = new Group
            {
                GroupID = Update_GroupsDTO.GroupID,
                Local_GroupName = Update_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Update_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = filepath,
                visable = Update_GroupsDTO.visable
            };
            }
            await _context.Groups.SingleUpdateAsync(group);
            await _context.SaveChangesAsync();
            return Accepted(group);
        }
    }
}
