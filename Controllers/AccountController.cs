using ASU_Research_2022.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using VideoGuide.Data;
using VideoGuide.IRepository;
using VideoGuide.Models;
using VideoGuide.Repository;
using VideoGuide.Services;
using VideoGuide.View_Model;
using Z.Expressions;

namespace VideoGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly VideoGuideContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthManager _authManager;
        private readonly ImageUrlConverter _fileUrlConverter;
        private readonly IWebHostEnvironment _env;


        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, VideoGuideContext context, IMapper mapper, IAuthManager authManager, UnitOfWork unitOfWork, ImageUrlConverter fileUrlConverter, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
            _authManager = authManager;
            _unitOfWork = unitOfWork;
            _fileUrlConverter = fileUrlConverter;
            _env = env;
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = _mapper.Map<ApplicationUser>(userDTO);
            if (userDTO.Password == "")
            {
                var resultwithoupassword = await _userManager.CreateAsync(user);
                if (!resultwithoupassword.Succeeded)
                {
                    foreach (var error in resultwithoupassword.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }
                userDTO.Roles.Add("User");
            }
            else
            {
            var result = await _userManager.CreateAsync(user, userDTO.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }

            if (userDTO.listGroupID.Count()>0)
            {
            VideoGuide videoGuide = new VideoGuide(_context,_mapper,_fileUrlConverter,_env);
                List<listUserID> listUserIDs = new List<listUserID>();
                listUserID listUserID = new listUserID();
                listUserID.Id = user.Id;
                listUserIDs.Add(listUserID);
                Group_UserDTO Group_UserDTO = new Group_UserDTO()
                {
                    listUserID = listUserIDs,
                    listGroupID = userDTO.listGroupID,
                    column = "Id"
                };
                await videoGuide.AddGroupUser(Group_UserDTO);
            }
            await _userManager.AddToRolesAsync(user, userDTO.Roles);

            return Accepted();

        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO userDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _authManager.ValidateUser(userDTO) && userDTO.Password != "")
            {

                return Problem($"User Name or Password is incorrect", statusCode: 401);
            }
            if (!await _authManager.ActiveUser(userDTO))
            {
                return Problem($"Account is Disable", statusCode: 500);
            }
            var user = await _unitOfWork.ApplicationUser.Get(expression: e => e.UserName == userDTO.UserName);
            if (user.PasswordHash != null && userDTO.Password == "") { return Problem("Please Send Your Password"); }
            //var RolesAssignedUser = await _userManager.GetRolesAsync(User);
#pragma warning disable CS8604 // Possible null reference argument.
            return Accepted(new
            {
                Token = await _authManager.CreateToken(user)
                ,
                user = new { user.Id, user.FullName }
            });
#pragma warning restore CS8604 // Possible null reference argument.

        }
        [HttpPost]
        [Route("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _authManager.ValidateUser(userDTO))
            {

                return Problem($"User Name or Password is incorrect", statusCode: 401);
            }
            var user = await _userManager.FindByNameAsync(userDTO.UserName);
            await _userManager.ChangePasswordAsync(user, userDTO.Password, userDTO.NewPassword);
            return Ok(user);

        }
        [HttpGet]
        [Route("Get_User")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get_User()
        {
            return Ok(await _unitOfWork.ApplicationUser.GetWithSelect(selector: a => new { a.Id, a.FullName }));
        }
        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole(string RoleName)
        {
            var role = new IdentityRole();
            role.Name = RoleName;
            var status = await _roleManager.CreateAsync(role);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            return Ok(role);
        }
        [HttpPost("UpdateRole")]
        public async Task<IActionResult> UpdateRole(string RoleId, string NewRoleName)
        {
            IdentityRole Role = await _roleManager.FindByIdAsync(RoleId);
            if (Role == null)
            {
                return Ok("The role is not Fount");
            }
            Role.Name = NewRoleName;

            var status = await _roleManager.UpdateAsync(Role);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            return Ok(status);
        }
        [HttpPost("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string RoleId)
        {
            IdentityRole Role = await _roleManager.FindByIdAsync(RoleId);
            if (Role == null)
            {
                return BadRequest("The role is not Fount");
            }
            var Roleassignuser = await _userManager.GetUsersInRoleAsync(Role.Name);
            //return Ok(Roleassignuser);
            if (Roleassignuser.Count > 0)
            {
                return BadRequest("The role is assigned users");
            }
            var status = await _roleManager.DeleteAsync(Role);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok(status);
        }
        [HttpGet("GetRole")]
        public IActionResult GetRole()
        {
            return Ok(_roleManager.Roles.ToList());
        }
        [HttpGet("GetAllRolesAssignedUser")]
        public async Task<IActionResult> GetAllRolesAssignedUser(string Id)
        {
            if (Id == null)
            {
                return Ok("Please Send UserId");
            }
            var Roles = _roleManager.Roles.ToList();
            var User = await _userManager.FindByIdAsync(Id);
            if (User == null)
            {
                return Ok("User is Not Found");
            }
            var RolesAssignedUser = await _userManager.GetRolesAsync(User);
            var RolesChecked = Roles.Where(w => RolesAssignedUser.Contains(w.Name)).Select(x => new { x.Id, x.Name, x.NormalizedName, x.ConcurrencyStamp, Checked = RolesAssignedUser.Contains(x.Name) == true });
            return Ok(RolesChecked);
        }
        [HttpPost("AddRoleToUser")]
        public async Task<IActionResult> AddRoleToUser(string Id, List<string> Role)
        {
            if (Id == null || Role.Count() == 0)
            {
                return Ok("Please Send UserId and RoleId");
            }
            var User = await _userManager.FindByIdAsync(Id);
            if (User == null)
            {
                return Ok("User is Not Found");
            }
            //var Role = _roleManager.Roles.Where(x => x.Id == RoleId).SingleOrDefault()?.Name;
            var status = await _userManager.AddToRolesAsync(User, Role);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            return Ok(status);
        }
        [HttpPost("RemoveRoleToUser")]
        public async Task<IActionResult> RemoveRoleToUser(string Id, List<string> Role)
        {
            if (Id == null || Role.Count() == 0)
            {
                return Ok("Please Send UserId and RoleId");
            }
            var User = await _userManager.FindByIdAsync(Id);
            if (User == null)
            {
                return Ok("User is Not Found");
            }
            //var Role = _roleManager.Roles.Where(x => x.Id == RoleId).SingleOrDefault()?.Name;
            var status = await _userManager.RemoveFromRolesAsync(User, Role);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            return Ok(status);
        }
        [HttpPost("RestPassword")]
        public async Task<IActionResult> RestPassword(RestPassword RestPassword)
        {
            var user = await _userManager.FindByNameAsync(RestPassword.UserName);
            if (user == null)
            {
                return Ok("UserName is not found");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (RestPassword.Password == "")
            {
                user.PasswordHash = null;
                _unitOfWork.ApplicationUser.Update(user);
                await _unitOfWork.Save();
                return Ok();
            }
            else
            {

            var status = await _userManager.ResetPasswordAsync(user, token, RestPassword.Password);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok(status);
            }
        }
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(DeleteUser DeleteUser)
        {
            var user = await _userManager.FindByNameAsync(DeleteUser.UserName);
            if (user == null)
            {
                return Ok("UserName is not found");
            }
            var status = await _userManager.DeleteAsync(user);
            if (!status.Succeeded)
            {
                foreach (var error in status.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok(status);
        }
        [HttpPost("Change_FullName")]
        public async Task<IActionResult> Change_FullName(Change_FullNameDTO change_FullNameDTO)
        {
            var user = await _userManager.FindByNameAsync(change_FullNameDTO.UserName);
            if (user == null)
            {
                return Ok("UserName is not found");
            }
            user.FullName = change_FullNameDTO.FullName;
            _unitOfWork.ApplicationUser.Update(user);
            await _unitOfWork.Save();
            return Ok();
        }
        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UpdateUser UpdateUser)
        {
            var user = await _userManager.FindByNameAsync(UpdateUser.UserName);
            if (user == null)
            {
                return Ok("UserName is not found");
            }
            if(user.FullName != UpdateUser.FullName)
            {
                Change_FullNameDTO change_FullNameDTO = new Change_FullNameDTO 
                {
                    UserName = UpdateUser.UserName, FullName = UpdateUser.FullName
                };
                await Change_FullName(change_FullNameDTO);
            }
            RestPassword restPassword = new RestPassword 
            {
                UserName = UpdateUser.UserName,Password = UpdateUser.Password
            };
            await RestPassword(restPassword);
            var Roles = await _userManager.GetRolesAsync(user);
            var status_Delete = await _userManager.RemoveFromRolesAsync(user, Roles);
            if (!status_Delete.Succeeded)
            {
                foreach (var error in status_Delete.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            var status_Insert = await _userManager.AddToRolesAsync(user, UpdateUser.Roles);
            if (!status_Insert.Succeeded)
            {
                foreach (var error in status_Insert.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            user.LockoutEnabled = UpdateUser.Active;
            await _userManager.UpdateAsync(user);

                VideoGuide videoGuide = new VideoGuide(_context, _mapper, _fileUrlConverter, _env);
                List<listUserID> listUserIDs = new List<listUserID>();
                listUserID listUserID = new listUserID();
                listUserID.Id = user.Id;
                listUserIDs.Add(listUserID);
                Group_UserDTO Group_UserDTO = new Group_UserDTO()
                {
                    listUserID = listUserIDs,
                    listGroupID = UpdateUser.listGroupID,
                    column = "Id"
                };
                await videoGuide.AddGroupUser(Group_UserDTO);
            
            return Ok();
        }
    }
}
