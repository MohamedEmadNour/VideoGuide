using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VideoGuide.Models;
using VideoGuide.Services;
using VideoGuide.View_Model;

namespace VideoGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoGuide : ControllerBase
    {
        private readonly VideoGuideContext _context;
        private readonly IMapper _mapper;
        private readonly ImageUrlConverter _imageUrlConverter;
        private readonly IWebHostEnvironment _env;

        public VideoGuide(VideoGuideContext context, IMapper mapper, ImageUrlConverter imageUrlConverter, IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _imageUrlConverter = imageUrlConverter;
            _env = env;
        }
        #region Group
        [HttpGet("Get_Groups")]
        //[Authorize(Roles ="User,Admin")]
        public async Task<IActionResult> Get_Groups(int? GroupID)
        {
            IQueryable<Models.Group> baseQuery = _context.Groups
                .Include(i => i.UserGroups)
                .Where(w => w.visable == true);
            List<Get_GroupsDTO> groupData = new List<Get_GroupsDTO>();
            // Apply the filter only if filterId has a value
            if (GroupID.HasValue)
            {
                groupData = await baseQuery.Where(w => w.GroupID == GroupID.Value).Select(s => new Get_GroupsDTO
                {
                    Local_GroupName = s.Local_GroupName ?? string.Empty,
                    Lantin_GroupName = s.Lantin_GroupName ?? string.Empty,
                    Image = _imageUrlConverter.ConvertToUrl(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Group_Photo_Location = s.Group_Photo_Location ?? string.Empty,
                    GroupID = s.GroupID,
                    GetGroupUser = s.UserGroups.Select(u => new GetGroupUser
                    {
                        Id = u.Id ?? string.Empty,
                        FullName = u.IdNavigation.FullName,
                        GroupID = u.GroupID,
                        Local_GroupName = u.Group.Local_GroupName ?? string.Empty,
                        Lantin_GroupName = u.Group.Lantin_GroupName ?? string.Empty
                    }).ToList()
                })
         .ToListAsync();
            }
            else
            {
                groupData = await baseQuery.Select( s => new Get_GroupsDTO
                {
                    Local_GroupName = s.Local_GroupName??string.Empty,
                    Lantin_GroupName = s.Lantin_GroupName ?? string.Empty,
                    Image = _imageUrlConverter.ConvertToUrl(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Group_Photo_Location = s.Group_Photo_Location ?? string.Empty,
                    GroupID = s.GroupID
                }).ToListAsync();

            }
            return Ok(groupData);
        }
        [HttpPost("Insert_Groups"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Insert_Groups([FromForm] Insert_GroupsDTO Insert_GroupsDTO)
        {
            string dbPath = await SaveImage(Insert_GroupsDTO.Image);

            Models.Group group = new Models.Group
            {
                Local_GroupName = Insert_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Insert_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = dbPath
            };
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            return Accepted(Get_Groups(group.GroupID).Result);
        }
        [HttpPut("Update_Groups"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update_Groups([FromForm] Update_GroupsDTO Update_GroupsDTO)
        {
            string filepath = _context.Groups.FirstOrDefaultAsync(w => w.GroupID == Update_GroupsDTO.GroupID).Result?.Group_Photo_Location ?? string.Empty;
            Models.Group group = new Models.Group();
            if (filepath != Update_GroupsDTO.Group_Photo_Location && Update_GroupsDTO.Image != null)
            {

                string dbPath = await SaveImage(Update_GroupsDTO.Image);

                group = new Models.Group
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
                group = new Models.Group
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
        #endregion
        #region GroupUser
        [HttpPost("AddGroupUser")]
        public async Task<IActionResult> AddGroupUser(GroupUserDTO groupUserDTOs)
        {
            var groupUserCombinations = (from groupId in groupUserDTOs.listGroupID.Select(listGroupID=> listGroupID.GroupID)
                                        from UserId in groupUserDTOs.listId.Select(s => s.Id)
                                        select new UserGroup { GroupID = groupId, Id = UserId }).ToList();
            List<UserGroup> UserGroup = await _context.UserGroups.Where(w => groupUserDTOs.listId.Select(s => s.Id).Contains(w.Id)).ToListAsync();
            List<UserGroup> UserGroupdelete = UserGroup.Where(w=> !groupUserCombinations.Any(g=>g.Id == w.Id && g.GroupID == w.GroupID)).ToList();
            List<UserGroup> UserGroupInsert = groupUserCombinations.Where(w => !UserGroup.Any(u => u.Id == w.Id && u.GroupID == w.GroupID)).ToList();
            if (UserGroupdelete.Count()>0)
            {
            await _context.BulkDeleteAsync(UserGroupdelete);
            await _context.SaveChangesAsync();
            }
            if (UserGroupInsert.Count()>0) 
            {
            await _context.BulkInsertAsync(UserGroupInsert);
            await _context.SaveChangesAsync();
            }
            return Accepted(await _context.UserGroups.Select(s => new GetGroupUser
            {
                Id = s.Id?? string.Empty,
                FullName = s.IdNavigation.FullName,
                GroupID = s.GroupID,
                Local_GroupName = s.Group.Local_GroupName ?? string.Empty,
                Lantin_GroupName = s.Group.Lantin_GroupName ?? string.Empty
            }).ToListAsync());
        }
        #endregion
        #region Tag
        [HttpPost("Insert_Tags"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Insert_Tags([FromForm] Insert_TagsDTO Insert_TagsDTO)
        {
            string dbPath = await SaveImage(Insert_TagsDTO.Image);

            Tag Tag = new Tag
            {
                Local_TagName = Insert_TagsDTO.Local_TagName,
                Lantin_TagName = Insert_TagsDTO.Lantin_TagName,
                Tag_Photo_Location = dbPath
            };
            await _context.Tags.AddAsync(Tag);
            await _context.SaveChangesAsync();
            return Accepted(Get_Tags(Tag.TagID).Result);
        }
        [HttpPut("Update_Tags"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update_Tags([FromForm] Update_TagsDTO Update_TagsDTO)
        {
            string filepath = _context.Tags.FirstOrDefaultAsync(w => w.TagID == Update_TagsDTO.TagID).Result?.Tag_Photo_Location ?? string.Empty;
            Tag Tag = new Tag();
            if (filepath != Update_TagsDTO.Tag_Photo_Location && Update_TagsDTO.Image != null)
            {

                string dbPath = await SaveImage(Update_TagsDTO.Image);
                Tag = new Tag
                {
                    TagID = Update_TagsDTO.TagID,
                    Lantin_TagName = Update_TagsDTO.Lantin_TagName,
                    Local_TagName = Update_TagsDTO.Local_TagName,
                    Tag_Photo_Location = dbPath,
                    visable = Update_TagsDTO.visable
                };
                System.IO.File.Delete(filepath);

            }
            else
            {
                Tag = new Tag
                {
                    TagID = Update_TagsDTO.TagID,
                    Lantin_TagName = Update_TagsDTO.Lantin_TagName,
                    Local_TagName = Update_TagsDTO.Local_TagName,
                    Tag_Photo_Location = filepath,
                    visable = Update_TagsDTO.visable
                };
            }
            await _context.Tags.SingleUpdateAsync(Tag);
            await _context.SaveChangesAsync();
            return Accepted(Tag);
        }
        [HttpGet("Get_Tags")]
        //[Authorize(Roles ="User,Admin")]
        public async Task<IActionResult> Get_Tags(int? TagID)
        {
            IQueryable<Tag> baseQuery = _context.Tags
                .Include(Grouptags => Grouptags.GroupTags)
                .ThenInclude(Groups => Groups.Group)
                .Where(w => w.visable == true);
            List<Get_TagsDTO> groupData = new List<Get_TagsDTO>();
            // Apply the filter only if filterId has a value
            if (TagID.HasValue)
            {
                groupData = await baseQuery.Where(w => w.TagID == TagID.Value).Select(s => new Get_TagsDTO
                {
                    Local_TagName = s.Local_TagName ?? string.Empty,
                    Lantin_TagName = s.Lantin_TagName ?? string.Empty,
                    Image = _imageUrlConverter.ConvertToUrl(s.Tag_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Tag_Photo_Location = s.Tag_Photo_Location ?? string.Empty,
                    TagID = s.TagID,
                    GetTagGroup = s.GroupTags.Select(u => new GetTagGroup
                    {
                        GroupID = u.Group.GroupID,
                        Local_GroupName = u.Group.Local_GroupName ?? string.Empty,
                        Lantin_GroupName = u.Group.Lantin_GroupName ?? string.Empty
                    }).ToList()
                })
         .ToListAsync();
            }
            else
            {
                groupData = await baseQuery.Select(s => new Get_TagsDTO
                {
                    Local_TagName = s.Local_TagName ?? string.Empty,
                    Lantin_TagName = s.Lantin_TagName ?? string.Empty,
                    Image = _imageUrlConverter.ConvertToUrl(s.Tag_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Tag_Photo_Location = s.Tag_Photo_Location ?? string.Empty,
                    TagID = s.TagID,
                }).ToListAsync();

            }
            return Ok(groupData);
        }
        #endregion
        #region Image
        [NonAction]
        public async Task<string>SaveImage(IFormFile? Image)
        {
            // Create a new name for the file
            string fileName = Path.GetRandomFileName() + Path.GetExtension(Image?.FileName);
            var folderName = Path.Combine("Resources", "Images");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), _env.WebRootPath, folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
            var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require

            // Save the file
            using (var fileStream = new FileStream(Path.Combine(_env.WebRootPath, dbPath), FileMode.Create))
            {
                await Image?.CopyToAsync(fileStream);
            }
            return dbPath;
        }
        #endregion
    }
}
