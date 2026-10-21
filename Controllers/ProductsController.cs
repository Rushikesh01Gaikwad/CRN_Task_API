using CRUD_API.Context;
using CRUD_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CRUD_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Items)
                .OrderBy(p => p.Id);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                products
            });
        }

        // GET: api/products/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            product.Id = 0;
            product.ProductName = product.ProductName.Trim();
            product.CreatedBy = product.CreatedBy.Trim();
            product.CreatedOn = DateTime.UtcNow;
            product.ModifiedBy = null;
            product.ModifiedOn = null;

            foreach (var item in product.Items)
            {
                item.Id = 0;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }

        // PUT: api/products/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            Product request)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            if (string.IsNullOrWhiteSpace(request.ProductName) ||
                request.ProductName.Trim().Length > 255)
            {
                return BadRequest(new
                {
                    message = "ProductName is required and must not exceed 255 characters."
                });
            }

            if (string.IsNullOrWhiteSpace(request.ModifiedBy) ||
                request.ModifiedBy.Trim().Length > 100)
            {
                return BadRequest(new
                {
                    message = "ModifiedBy is required and must not exceed 100 characters."
                });
            }

            product.ProductName = request.ProductName.Trim();
            product.ModifiedBy = request.ModifiedBy.Trim();
            product.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // DELETE: api/products/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
