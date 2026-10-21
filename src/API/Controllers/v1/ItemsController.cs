using Application.DTOs.Common;
using Application.DTOs.Products;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products/{productId:int}/[controller]")]
[Produces("application/json")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all items belonging to a product.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken cancellationToken)
    {
        var items = await _itemService.GetByProductIdAsync(productId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ItemDto>>.Ok(items));
    }

    /// <summary>
    /// Gets a specific item belonging to a product.
    /// </summary>
    [HttpGet("{itemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int productId, int itemId, CancellationToken cancellationToken)
    {
        var item = await _itemService.GetByIdAsync(productId, itemId, cancellationToken);
        return Ok(ApiResponse<ItemDto>.Ok(item));
    }

    /// <summary>
    /// Adds an item to a product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int productId, [FromBody] CreateItemDto request, CancellationToken cancellationToken)
    {
        var created = await _itemService.CreateAsync(productId, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { productId, itemId = created.Id, version = "1.0" },
            ApiResponse<ItemDto>.Ok(created, "Item added successfully."));
    }

    /// <summary>
    /// Updates quantity of an item under a product.
    /// </summary>
    [HttpPut("{itemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int productId, int itemId, [FromBody] UpdateItemDto request, CancellationToken cancellationToken)
    {
        var updated = await _itemService.UpdateAsync(productId, itemId, request, cancellationToken);
        return Ok(ApiResponse<ItemDto>.Ok(updated, "Item updated successfully."));
    }

    /// <summary>
    /// Removes an item from a product.
    /// </summary>
    [HttpDelete("{itemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int productId, int itemId, CancellationToken cancellationToken)
    {
        await _itemService.DeleteAsync(productId, itemId, cancellationToken);
        return NoContent();
    }
}
