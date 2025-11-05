using API.Filters;
using Application;
using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.CitizenIdentity.Request;
using Application.Dtos.Common.Request;
using Application.Dtos.DriverLicense.Request;
using Application.Dtos.Staff.Request;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    /// <summary>
    /// Handles user-related operations such as authentication, profile management,
    /// and Google account integration.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserService service,
        IUserProfileSerivce userProfileSerivce, IMemoryCache cache) : ControllerBase
    {
        private readonly IUserService _userService = service;
        private readonly IUserProfileSerivce _userProfileService = userProfileSerivce;
        private readonly IMemoryCache _cache = cache;

        /// <summary>
        /// Retrieves all users with optional filters for phone number, citizen ID number, or driver license number.
        /// </summary>
        /// <param name="phone">Optional filter for the user's phone number.</param>
        /// <param name="citizenIdNumber">Optional filter for the user's citizen ID number.</param>
        /// <param name="driverLicenseNumber">Optional filter for the user's driver license number.</param>
        /// <param name="roleName">Optional filter for the user's role name.</param>
        /// <param name="pagination">Optional filter for the pagination.</param>
        /// <returns>List of users matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No users found matching the given filters.</response>
        [HttpGet]
        [RoleAuthorize(RoleName.Admin, RoleName.Staff)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? phone,
            [FromQuery] string? citizenIdNumber,
            [FromQuery] string? driverLicenseNumber,
            [FromQuery] string? roleName,
            [FromQuery] PaginationParams pagination)
        {
            var users = await _userService.GetAllWithPaginationAsync(
                phone, citizenIdNumber, driverLicenseNumber, roleName, pagination);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves all staff users with optional filters for name and station.
        /// </summary>
        /// <param name="pagination">Optional filter for the pagination.</param>
        /// <param name="name">Optional filter for the staff member's name.</param>
        /// <param name="stationId">Optional filter for the station the staff is assigned to.</param>
        /// <returns>List of staff members matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No staff members found matching the given filters.</response>
        [HttpGet("staffs")]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> GetAllStaff(
            [FromQuery] PaginationParams pagination,
            [FromQuery] string? name,
            [FromQuery] Guid? stationId
        )
        {
            var users = await _userService.GetAllStaffAsync(pagination, name, stationId);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// Accessible only to staff and admin roles with role-based permission checks.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>User details if access is allowed and the user exists.</returns>
        /// <response code="200">User retrieved successfully.</response>
        /// <response code="403">Access denied. Insufficient role permissions.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}")]
        [RoleAuthorize(RoleName.Staff, RoleName.Admin)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString());
            var user = await _userService.GetByIdAsync(userId)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);
            var userFromDb = await _userService.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);
            if (user.Role!.Name == RoleName.Staff)
            {
                return userFromDb.Role!.Name == RoleName.Customer ? Ok(userFromDb) : throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
            }
            if (user.Role.Name == RoleName.Admin)
            {
                return userFromDb.Role!.Name == RoleName.Customer || userFromDb.Role.Name == RoleName.Staff ? Ok(userFromDb) : throw new ForbidenException(Message.UserMessage.DoNotHavePermission); ;
            }
            throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
        }

        /// <summary>
        /// Creates a new user with the specified information.
        /// </summary>
        /// <param name="req">Request containing user details such as name, email, role, and station assignment.</param>
        /// <returns>The unique identifier of the created user.</returns>
        /// <response code="200">Success — user created.</response>
        /// <response code="400">Invalid user data.</response>
        /// <response code="409">User with the same email already exists.</response>
        [HttpPost]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff, RoleName.SuperAdmin])]
        public async Task<IActionResult> Create([FromBody] CreateUserReq req)
        {
            var userId = await _userService.CreateAsync(req);
            return Ok(new { userId });
        }

        /// <summary>
        /// Updates an existing user's information by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="req">Request containing updated user details such as name, role, or contact information.</param>
        /// <returns>Success message if the user information is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid user data.</response>
        /// <response code="404">User not found.</response>
        [HttpPatch("{id}")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] UserUpdateReq req)
        {
            await _userProfileService.UpdateAsync(id, req);
            return Ok();
        }

        /// <summary>
        /// Uploads a citizen identity image for a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="req">Request containing one or more image files to upload.</param>
        /// <returns>Uploaded citizen identity information.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="404">User not found.</response>
        //upload citizenId for Anonymous
        [HttpPut("{id}/citizen-identity")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCitizenIdById(Guid id, [FromForm] UploadImagesReq req)
        {
            var citizenIdentity = await _userProfileService.UploadCitizenIdAsync(id, req);
            return Ok(citizenIdentity);
        }

        /// <summary>
        /// Uploads a driver license image for a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="req">Request containing one or more image files to upload.</param>
        /// <returns>Uploaded driver license information.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id}/driver-license")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDriverLicenseById(Guid id, [FromForm] UploadImagesReq req)
        {
            var driverLisence = await _userProfileService.UploadDriverLicenseAsync(id, req);
            return Ok(driverLisence);
        }

        /// <summary>
        /// Updates the citizen identity information of a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="req">Request containing updated citizen identity information.</param>
        /// <returns>Updated citizen identity details.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid citizen identity data.</response>
        /// <response code="404">User or citizen identity record not found.</response>

        [HttpPatch("{id}/citizen-identity")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> UpdateCitizenIdentityById(Guid id, [FromBody] UpdateCitizenIdentityReq req)
        {
            var result = await _userProfileService.UpdateCitizenIdentityAsync(id, req);
            return Ok(result);
        }

        /// <summary>
        /// Updates the driver license information of a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="req">Request containing updated driver license information.</param>
        /// <returns>Updated driver license details.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid driver license data.</response>
        /// <response code="404">User or driver license record not found.</response>
        [HttpPatch("{id}/driver-license")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> UpdateDriverLicenseById(Guid id, [FromBody] UpdateDriverLicenseReq req)
        {
            var result = await _userProfileService.UpdateDriverLicenseAsync(id, req);
            return Ok(result);
        }

        /// <summary>
        /// Deletes the citizen identity information of a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user whose citizen identity will be deleted.</param>
        /// <returns>Success message if the citizen identity is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">User or citizen identity record not found.</response>
        [HttpDelete("{id}/citizen-identity")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> DeleteCitizenIdentityById(Guid id)
        {
            await _userProfileService.DeleteCitizenIdentityAsync(id);
            return Ok();
        }

        /// <summary>
        /// Deletes the driver license information of a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user whose driver license will be deleted.</param>
        /// <returns>Success message if the driver license is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">User or driver license record not found.</response>
        [HttpDelete("{id}/driver-license")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> DeleteDriverLicenseById(Guid id)
        {
            await _userProfileService.DeleteDriverLicenseAsync(id);
            return Ok();
        }

        /// <summary>
        /// Deletes a user by their unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>Success message if the user is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">User not found.</response>
        /// <response code="403">Forbidden — only admins can perform this action.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            await _userService.DeleteCustomer(id);
            return Ok();
        }

        /// <summary>
        /// Get the citizen identity information of the user id.
        /// </summary>
        /// <returns>Success message if the citizen identity information is get successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Driver license record not found.</response>
        [HttpGet("{id}/citizen-identity")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> GetCitizenIdentityByUserIdAsync(Guid id)
        {
            var result = await _userProfileService.GetMyCitizenIdentityAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get the driver license information of the user id.
        /// </summary>
        /// <returns>Success message if the driver license information is get successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Driver license record not found.</response>
        [HttpGet("{id}/driver-license")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> GetDriverLicensetByUserIdAsync(Guid id)
        {
            var result = await _userProfileService.GetMyDriverLicenseAsync(id);
            return Ok(result);
        }
        /*
         * Status code
         * 200 success
         * 404 not found
         */

        //[HttpGet("phone/{phone}")]
        //[RoleAuthorize("Staff", "Admin")]
        //public async Task<IActionResult> GetUserByPhone(string phone)
        //{
        //    var user = await _userService.GetByPhoneAsync(phone);
        //    return Ok(user);
        //}

        //*
        // * Status code
        // * 200 success
        // */

        //[HttpGet]
        //[RoleAuthorize(RoleName.Staff, RoleName.Admin)]
        //public async Task<IActionResult> GetAll()
        //{
        //    var users = await _userService.GetAllUsersAsync();
        //    return Ok(users);
        //}

        //[HttpGet("citizen-identity/{idNumber}")]
        //[RoleAuthorize("Staff", "Admin")]
        //public async Task<IActionResult> getUserByCitizenIdNumber(string idNumber)
        //{
        //    var userView = await _userService.GetByCitizenIdentityAsync(idNumber);
        //    return Ok(userView);
        //}

        //[HttpGet("driver-license/{number}")]
        //[RoleAuthorize("Staff", "Admin")]
        //public async Task<IActionResult> getUserByDriverLisence(string number)
        //{
        //    var userView = await _userService.GetByDriverLicenseAsync(number);
        //    return Ok(userView);
        //}
    }
}