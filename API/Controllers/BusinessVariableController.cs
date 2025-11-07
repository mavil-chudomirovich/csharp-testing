using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.BusinessVariable.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles business variable.
    /// </summary>
    [Route("api/business-variables")]
    [ApiController]
    public class BusinessVariableController(IBusinessVariableService businessVariableService) : ControllerBase
    {
        private readonly IBusinessVariableService _businessVariableService = businessVariableService;


        /// <summary>
        ///Get all business variable.
        /// </summary>
        /// <response code="200">Login successfully.</response>
        [HttpGet]
        public async Task<IActionResult> GetBusinessVariables()
        {
            var businessVariables = await _businessVariableService.GetAllAsync();
            return Ok(businessVariables);
        }

        /// <summary>
        /// Authenticates a user and returns an access token if the credentials are valid.
        /// </summary>
        /// <param name="id">business variable id</param>
        /// <param name="req">update req object</param>
        /// <returns>Access token if login is successful.</returns>
        /// <response code="200">Login successfully.</response>
        /// <response code="400">Missing field required</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> UpdateBusinessVariables([FromRoute] Guid id, [FromBody] UpdateBusinessVariableReq req)
        {
            await _businessVariableService.UpdateAsync(id, req);
            return Ok();
        }
    }

}
