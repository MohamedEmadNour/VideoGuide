using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
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
        private readonly ImageUrlConverter _fileUrlConverter;
        private readonly IWebHostEnvironment _env;

        public VideoGuide(VideoGuideContext context, IMapper mapper, ImageUrlConverter imageUrlConverter, IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _fileUrlConverter = imageUrlConverter;
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
                    Image = _fileUrlConverter.ConvertToUrl(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
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
                    Image = _fileUrlConverter.ConvertToUrl(s.Group_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
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
            string dbPath = await SaveFile(Insert_GroupsDTO.Image, "Images");

            Models.Group group = new Models.Group
            {
                Local_GroupName = Insert_GroupsDTO.Local_GroupName,
                Lantin_GroupName = Insert_GroupsDTO.Lantin_GroupName,
                Group_Photo_Location = dbPath
            };
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            if (Insert_GroupsDTO.listTagID.Count() > 0)
            {
                List<listGroupID> listGroupID = new List<listGroupID>();
                listGroupID GroupID = new listGroupID();
                GroupID.GroupID = group.GroupID;
                listGroupID.Add(GroupID);
                List<listTagID> listTagID = ConverttolistTagID(Insert_GroupsDTO.listTagID);
                GroupTagDTO GroupTagDTO = new GroupTagDTO
                {
                    listTagID = listTagID,
                    listGroupID = listGroupID
                };
                await AddGroupTag(GroupTagDTO);
            }
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

                string dbPath = await SaveFile(Update_GroupsDTO.Image, "Images");

                group = new Models.Group
                {
                    GroupID = Update_GroupsDTO.GroupID,
                    Local_GroupName = Update_GroupsDTO.Local_GroupName,
                    Lantin_GroupName = Update_GroupsDTO.Lantin_GroupName,
                    Group_Photo_Location = dbPath,
                    visable = Update_GroupsDTO.visable
                };
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), _env.WebRootPath, filepath));

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
            GroupTagDTO GroupTagDTO= new GroupTagDTO();
            if (Update_GroupsDTO.listTagID.Count() > 0)
            {
                List<listGroupID> listGroupID = new List<listGroupID>();
                listGroupID GroupID = new listGroupID();
                GroupID.GroupID = group.GroupID;
                listGroupID.Add(GroupID);
                List<listTagID> listTagID = ConverttolistTagID(Update_GroupsDTO.listTagID);
                GroupTagDTO = new GroupTagDTO
                {
                    listTagID = listTagID,
                    listGroupID = listGroupID
                };
                await AddGroupTag(GroupTagDTO);
            }
            else
            {
                await AddGroupTag(GroupTagDTO);
            }
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
            string dbPath = await SaveFile(Insert_TagsDTO.Image, "Images");

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

                string dbPath = await SaveFile(Update_TagsDTO.Image, "Images");
                Tag = new Tag
                {
                    TagID = Update_TagsDTO.TagID,
                    Lantin_TagName = Update_TagsDTO.Lantin_TagName,
                    Local_TagName = Update_TagsDTO.Local_TagName,
                    Tag_Photo_Location = dbPath,
                    visable = Update_TagsDTO.visable
                };
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), _env.WebRootPath, filepath));

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
        public async Task<IActionResult> Get_Tags(int? TagID, int? GroupID = null)
        {
            IQueryable<Tag> baseQuery = _context.Tags
                .Include(Grouptags => Grouptags.GroupTags)
                .ThenInclude(Groups => Groups.Group)
                .Where(w => w.visable == true);
            List<Get_TagsDTO> groupData = new List<Get_TagsDTO>();
            if (GroupID.HasValue)
            {
                baseQuery = baseQuery.Where(group => group.GroupTags.Select(groupid => groupid.GroupID).Contains(GroupID));
            }
            // Apply the filter only if filterId has a value
            if (TagID.HasValue)
            {
                groupData = await baseQuery.Where(w => w.TagID == TagID.Value).Select(s => new Get_TagsDTO
                {
                    Local_TagName = s.Local_TagName ?? string.Empty,
                    Lantin_TagName = s.Lantin_TagName ?? string.Empty,
                    Image = _fileUrlConverter.ConvertToUrl(s.Tag_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
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
                    Image = _fileUrlConverter.ConvertToUrl(s.Tag_Photo_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Tag_Photo_Location = s.Tag_Photo_Location ?? string.Empty,
                    TagID = s.TagID,
                }).ToListAsync();

            }
            return Ok(groupData);
        }
        #endregion
        #region File
        [NonAction]
        public async Task<string>SaveFile(IFormFile? file,string filepath)
        {
            // Create a new name for the file
            string fileName = Path.GetRandomFileName() + Path.GetExtension(file?.FileName);
            var folderName = Path.Combine("Resources", filepath);

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), _env.WebRootPath, folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
            var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require

            // Save the file
            using (var fileStream = new FileStream(Path.Combine(_env.WebRootPath, dbPath), FileMode.Create))
            {
                await file?.CopyToAsync(fileStream);
            }
            return dbPath;
        }
        #endregion
        #region Video
        [HttpPost("AddVideo"), DisableRequestSizeLimit]
        public async Task<IActionResult> AddVideo([FromForm]VideoDTO VideoDTO) 
        {
            Video video = _mapper.Map<Video>(VideoDTO);
            video.Video_Location = await SaveFile(VideoDTO.Video, "Videos");
            await _context.Videos.AddAsync(video);
            await _context.SaveChangesAsync();
            if (VideoDTO.listTagID.Count()>0)
            {
            List<listVideoID> listVideoID = new List<listVideoID>();
            listVideoID VideoID = new listVideoID();
            VideoID.VideoID = video.VideoID;
            listVideoID.Add(VideoID);
            List<listTagID> listTagID = ConverttolistTagID(VideoDTO.listTagID);
            VideoTagDTO VideoTagDTO = new VideoTagDTO
            {
                listTagID = listTagID,
                listVideoID = listVideoID
            };
            await AddVideoTag(VideoTagDTO);
            }
            return Ok(video);
        }

        [HttpGet("Get_Videos")]
        //[Authorize(Roles ="User,Admin")]
        public async Task<IActionResult> Get_Videos(int? TagID, int? VideoID , string? Id = null,string? search = null)
        {
            IQueryable<Models.Video> baseQuery = _context.Videos
                .Where(w => w.visable == true);
            // Apply the filter only if filterId has a value
            if (TagID.HasValue)
            {
                baseQuery = baseQuery.Include(TagVideo => TagVideo.VideoTags).Where(TagVideo => TagVideo.VideoTags.Select(Tag=>Tag.TagID).Contains(TagID));
            }
            if (Id != null && !TagID.HasValue && search == null)
            {
                baseQuery = baseQuery.Include(Video_Fav => Video_Fav.Video_Favs).Where(Video_Fav => Video_Fav.Video_Favs.Select(Fav => Fav.Id).Contains(Id));
            }
            if (search != null)
            {
                baseQuery = baseQuery.Where(ser =>
                ser.Video_Lantin_Title.ToLower().Contains(search) ||
                ser.Video_Local_Tiltle.ToLower().Contains(search) ||
                ser.Video_Lantin_Description.ToLower().Contains(search) ||
                ser.Video_Local_Description.ToLower().Contains(search));
            }
            if (VideoID.HasValue)
            {
                List<Get_Videos_with_tagDTO> video = new List<Get_Videos_with_tagDTO>();

                video = await baseQuery.Where(w => w.VideoID == VideoID.Value).Select(Video_with_tag=> new Get_Videos_with_tagDTO
                {
                    Video_Local_Tiltle = Video_with_tag.Video_Local_Tiltle ?? string.Empty,
                    Video_Lantin_Title = Video_with_tag.Video_Lantin_Title ?? string.Empty,
                    Video = _fileUrlConverter.ConvertToUrl(Video_with_tag.Video_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Video_Location = Video_with_tag.Video_Location ?? string.Empty,
                    VideoID = Video_with_tag.VideoID,
                    Video_CountOfViews = Video_with_tag.Video_CountOfViews,
                    Video_Lantin_Description = Video_with_tag.Video_Lantin_Description ?? string.Empty,
                    Video_Local_Description = Video_with_tag.Video_Local_Description ?? string.Empty,
                    visable = Video_with_tag.visable ?? false,
                    GetVideoTagDTO = Video_with_tag.VideoTags.Select(Video_tag=> new GetVideoTagDTO
                    {
                        Lantin_TagName = Video_tag.Tag.Lantin_TagName ?? string.Empty,
                        Local_TagName = Video_tag.Tag.Local_TagName ?? string.Empty,
                        TagID = Video_tag.TagID ?? 0,
                        VideoTagID = Video_tag.VideoTagID,
                    }).ToList(),
                }).ToListAsync();
                return Ok(video);

            }
            else if(!VideoID.HasValue && !TagID.HasValue)
            {
                List<Get_VideosDTO> video = new List<Get_VideosDTO>();
                if (Id != null)
                {
                    List<int?> taguser = await _context.UserGroups.Where(user => user.Id == Id).
                        SelectMany(group => group.Group.GroupTags.Select(tag => tag.TagID)).ToListAsync();
                    baseQuery = baseQuery.Include(videotag => videotag.VideoTags).
                        Where(videotag => videotag.VideoTags.Any(tag => taguser.Contains(tag.TagID)));
                }
                video = await baseQuery.Select(s => new Get_VideosDTO
            {
                Video_Local_Tiltle = s.Video_Local_Tiltle ?? string.Empty,
                Video_Lantin_Title = s.Video_Lantin_Title ?? string.Empty,
                Video = _fileUrlConverter.ConvertToUrl(s.Video_Location ?? string.Empty), // Now calling the method that returns byte[]
                Video_Location = s.Video_Location ?? string.Empty,
                VideoID = s.VideoID,
                Video_CountOfViews = s.Video_CountOfViews,
                Video_Lantin_Description = s.Video_Lantin_Description ?? string.Empty,
                Video_Local_Description = s.Video_Local_Description ?? string.Empty,
                visable = s.visable ?? false
            }).ToListAsync();
                return Ok(video);
            }
            else if(!VideoID.HasValue && TagID.HasValue && Id != null)
            {
                List<Get_VideoswithfavDTO> video = new List<Get_VideoswithfavDTO>();
                video = await baseQuery.Include(fav=>fav.Video_Favs).Select(s => new Get_VideoswithfavDTO
                {
                    Video_Local_Tiltle = s.Video_Local_Tiltle ?? string.Empty,
                    Video_Lantin_Title = s.Video_Lantin_Title ?? string.Empty,
                    Video = _fileUrlConverter.ConvertToUrl(s.Video_Location ?? string.Empty), // Now calling the method that returns byte[]
                    Video_Location = s.Video_Location ?? string.Empty,
                    VideoID = s.VideoID,
                    Video_CountOfViews = s.Video_CountOfViews,
                    Video_Lantin_Description = s.Video_Lantin_Description ?? string.Empty,
                    Video_Local_Description = s.Video_Local_Description ?? string.Empty,
                    visable = s.visable ?? false,
                    fav = s.Video_Favs.Count(fav=>fav.VideoID == s.VideoID)> 0? true:false
                }).ToListAsync();
                return Ok(video);

            }
            return NoContent();
        }
        [HttpPut("Update_Video"), DisableRequestSizeLimit]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update_Video([FromForm] UpdateVideoDTO UpdateVideoDTO)
        {
            string filepath = _context.Videos.FirstOrDefaultAsync(w => w.VideoID == UpdateVideoDTO.VideoID).Result?.Video_Location ?? string.Empty;
            Video Video = new Video();
            if (filepath != UpdateVideoDTO.Video_Location && UpdateVideoDTO.Video != null)
            {

                string dbPath = await SaveFile(UpdateVideoDTO.Video, "Videos");
                Video = new Video
                {
                    VideoID = UpdateVideoDTO.VideoID,
                    Video_Lantin_Title = UpdateVideoDTO.Video_Lantin_Title,
                    Video_Local_Tiltle = UpdateVideoDTO.Video_Local_Tiltle,
                    Video_Lantin_Description = UpdateVideoDTO.Video_Lantin_Description,
                    Video_Local_Description = UpdateVideoDTO.Video_Local_Description,
                    Video_Location = dbPath,
                    visable = UpdateVideoDTO.visable,
                    Video_CountOfViews = 0
                };
                string Fullpath = Path.Combine(Directory.GetCurrentDirectory(), _env.WebRootPath, filepath);
                System.IO.File.Delete(Fullpath);

            }
            else
            {
                int Video_CountOfViews = _context.Videos.FirstOrDefaultAsync(w => w.VideoID == UpdateVideoDTO.VideoID).Result?.Video_CountOfViews ?? 0;

                Video = new Video
                {
                    VideoID = UpdateVideoDTO.VideoID,
                    Video_Lantin_Title = UpdateVideoDTO.Video_Lantin_Title,
                    Video_Local_Tiltle = UpdateVideoDTO.Video_Local_Tiltle,
                    Video_Lantin_Description = UpdateVideoDTO.Video_Lantin_Description,
                    Video_Local_Description = UpdateVideoDTO.Video_Local_Description,
                    Video_Location = filepath,
                    visable = UpdateVideoDTO.visable,
                    Video_CountOfViews = Video_CountOfViews
                };
            }
            await _context.Videos.SingleUpdateAsync(Video);
            await _context.SaveChangesAsync();
            VideoTagDTO VideoTagDTO = new VideoTagDTO();
            if (UpdateVideoDTO.listTagID.Count() > 0)
            {
                List<listVideoID> listVideoID = new List<listVideoID>();
                listVideoID VideoID = new listVideoID();
                VideoID.VideoID = UpdateVideoDTO.VideoID;
                listVideoID.Add(VideoID);
                List<listTagID> listTagID = ConverttolistTagID(UpdateVideoDTO.listTagID);
                VideoTagDTO = new VideoTagDTO
                {
                    listTagID = listTagID,
                    listVideoID = listVideoID
                };
                await AddVideoTag(VideoTagDTO);
            }
            else
            {
                await AddVideoTag(VideoTagDTO);
            }
            return Accepted(Video);
        }
        [HttpPut("Update_View_Video")]
        public async Task<IActionResult> Update_Video(
            [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
            [DataExists(typeof(Video), "VideoID", ErrorMessage = "This Video is Not Found")]
            int VideoID)
                {
                    Video video = await _context.Videos.FindAsync(VideoID) ?? new Video();
                    if (video.visable == false)
                    {
                        return Problem("This video is disabled");
                    }
                    video.Video_CountOfViews++;
                    await _context.Videos.SingleUpdateAsync(video);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }

        #endregion
        #region VideoTag
        [HttpPost("AddVideoTag")]
        public async Task<IActionResult> AddVideoTag(VideoTagDTO VideoTagDTO)
        {
            var videotagCombinations = (from VideoID in VideoTagDTO.listVideoID.Select(listVideoID => listVideoID.VideoID)
                                         from TagID in VideoTagDTO.listTagID.Select(listTagID => listTagID.TagID)
                                         select new VideoTag { VideoID = VideoID, TagID = TagID }).ToList();
            List<VideoTag> VideoTag = await _context.VideoTags.Where(VideoTag => VideoTagDTO.listVideoID.Select(video=> video.VideoID).ToList().Contains((int)VideoTag.VideoID)).ToListAsync();
            List<VideoTag> VideoTagdelete = VideoTag.Where(videotag => !videotagCombinations.Any(videotagcom => videotagcom.VideoID == videotag.VideoID && videotagcom.TagID == videotag.TagID)).ToList();
            List<VideoTag> VideoTagInsert = videotagCombinations.Where(videotagcom => !VideoTag.Any(videotag => videotag.VideoID == videotagcom.VideoID && videotag.TagID == videotagcom.TagID)).ToList();
            if (VideoTagdelete.Count() > 0)
            {
                await _context.BulkDeleteAsync(VideoTagdelete);
                await _context.SaveChangesAsync();
            }
            if (VideoTagInsert.Count() > 0)
            {
                await _context.BulkInsertAsync(VideoTagInsert);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        #endregion
        #region GroupTag
        [HttpPost("AddGroupTag")]
        public async Task<IActionResult> AddGroupTag(GroupTagDTO GroupTagDTO)
        {
            var grouptagCombinations = (from GroupID in GroupTagDTO.listGroupID.Select(listGroupID => listGroupID.GroupID)
                                        from TagID in GroupTagDTO.listTagID.Select(listTagID => listTagID.TagID)
                                        select new GroupTag { GroupID = GroupID, TagID = TagID }).ToList();
            List<GroupTag> GroupTag = await _context.GroupTags.Where(GroupTag => GroupTagDTO.listGroupID.Select(group => group.GroupID).ToList().Contains((int)GroupTag.GroupID)).ToListAsync();
            List<GroupTag> GroupTagdelete = GroupTag.Where(grouptag => !grouptagCombinations.Any(grouptagcom => grouptagcom.GroupID == grouptag.GroupID && grouptagcom.TagID == grouptag.TagID)).ToList();
            List<GroupTag> GroupTagInsert = grouptagCombinations.Where(grouptagcom => !GroupTag.Any(grouptag => grouptag.GroupID == grouptagcom.GroupID && grouptagcom.TagID == grouptag.TagID)).ToList();
            if (GroupTagdelete.Count() > 0)
            {
                await _context.BulkDeleteAsync(GroupTagdelete);
                await _context.SaveChangesAsync();
            }
            if (GroupTagInsert.Count() > 0)
            {
                await _context.BulkInsertAsync(GroupTagInsert);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        #endregion
        #region helptoconvertfrom int to object
        [NonAction]
        public List<listTagID> ConverttolistTagID(List<int> listint)
        {
            List<listTagID> listTagID = new List<listTagID>();
            foreach (var TagintID in listint)
            {
                listTagID TagID = new listTagID();
                TagID.TagID = TagintID;
                listTagID.Add(TagID);
            }
            return listTagID;
        }

        #endregion
        #region Fav
        [HttpPost("AddFav")]
        public async Task<IActionResult> AddFav(AddFavDTO AddFavDTO)
        {
            Video_Fav Video_Fav = await _context.Video_Favs.Where(Fav=>Fav.VideoID == AddFavDTO.VideoID && Fav.Id == AddFavDTO.Id).FirstOrDefaultAsync();
            if (Video_Fav == null)
            {
                Video_Fav=_mapper.Map<Video_Fav>(AddFavDTO);
                await _context.Video_Favs.AddAsync(Video_Fav);
                await _context.SaveChangesAsync();
                return Ok();

            }
            _context.Video_Favs.Remove(Video_Fav);
            _context.SaveChanges();
            return Ok();
        }
        #endregion
    }
}
