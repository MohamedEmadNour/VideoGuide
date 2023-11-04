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
            var groupsDTO = groupData.Select(s => new Get_GroupsDTO
            {
                Local_GroupName = s.Local_GroupName ?? string.Empty,
                Lantin_GroupName = s.Lantin_GroupName ?? string.Empty,
                Image = SendImage(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                Group_Photo_Location = s.Group_Photo_Location ?? string.Empty
            }).ToList();
            return Ok(groupsDTO);
        }
        [NonAction]
        public byte[] SendImage(string filename)
        {
            // Define the path to the file
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", filename);

            // Check if the file exists
            if (System.IO.File.Exists(imagePath))
            {
                // Return the file as a byte array
                return System.IO.File.ReadAllBytes(imagePath);
            }

            return null; // or return a default image or a specific byte array indicating no image found
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
            string filepath = (await _context.Groups.FirstOrDefaultAsync(w => w.GroupID == Update_GroupsDTO.GroupID))?.Group_Photo_Location ?? string.Empty;
            Group group = new Group();
            if (filepath!= Update_GroupsDTO.Group_Photo_Location)
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
            Directory.Delete(filepath);
            }
            else
            {
            group = new Group
            {
                GroupID = Update_GroupsDTO.GroupID,
                Local_GroupName = Update_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Update_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = Update_GroupsDTO.Group_Photo_Location,
                visable = Update_GroupsDTO.visable
            };
            }
            await _context.Groups.SingleUpdateAsync(group);
            await _context.SaveChangesAsync();
            return Accepted();
        }
    }
}
