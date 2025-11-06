using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Brand.Request;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles brand management operations such as creation, update, deletion, and retrieval.
    /// </summary>
    [ApiController]
    [Route("api/brands")]
    public class BrandsController(IBrandService brandService) : ControllerBase
    {
        private readonly IBrandService _brandService = brandService;

        /// <summary>
        /// Retrieves all vehicle brands in the system (admin or staff only).
        /// </summary>
        /// <returns>List of all brands.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _brandService.GetAllAsync();
            return Ok(brands);
        }

        /// <summary>
        /// Retrieves detailed information about a specific brand by its unique identifier (admin or staff only).
        /// </summary>
        /// <param name="id">The unique identifier of the brand.</param>
        /// <returns>Detailed information of the specified brand.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission.</response>
        /// <response code="404">Brand not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            return Ok(brand);
        }

        /// <summary>
        /// Creates a new vehicle brand (admin only).
        /// </summary>
        /// <param name="dto">Request containing brand details such as name, description, country, and founded year.</param>
        /// <returns>Information about the created brand.</returns>
        /// <response code="201">Brand created successfully.</response>
        /// <response code="400">Invalid brand data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Create([FromBody] BrandReq dto)
        {
            var created = await _brandService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing vehicle brand by its unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the brand to update.</param>
        /// <param name="dto">Request containing updated brand details.</param>
        /// <returns>Updated brand information.</returns>
        /// <response code="200">Brand updated successfully.</response>
        /// <response code="400">Invalid brand data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission.</response>
        /// <response code="404">Brand not found.</response>
        [HttpPut("{id:guid}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandReq dto)
        {
            var updated = await _brandService.UpdateAsync(id, dto);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a vehicle brand by its unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the brand to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        /// <response code="204">Brand deleted successfully.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission.</response>
        /// <response code="404">Brand not found.</response>
        [HttpDelete("{id:guid}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _brandService.DeleteAsync(id);
            return NoContent();
        }
    }
}
