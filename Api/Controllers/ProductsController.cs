using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CachingTaskDbContext _context;
        private readonly DemoService _demoService;

        public ProductsController(CachingTaskDbContext context, DemoService demoService)
        {
            _context = context;
            _demoService = demoService;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {

            string cacheKey = "products_all";

            var cached = await _demoService.GetMemoryCachedDataAsync(cacheKey);

            if (cached != null)
            {

                var products = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(cached);
                return products;
            }


            var dbProducts = await _context.Products.ToListAsync();


            var serialized = System.Text.Json.JsonSerializer.Serialize(dbProducts);
            await _demoService.SaveDataWithWriteThroughAsync(cacheKey, serialized);

            return dbProducts;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            string cacheKey = $"product_{id}";

            var cached = await _demoService.GetMemoryCachedDataAsync(cacheKey);

            if (cached != null)
            {
                var product = System.Text.Json.JsonSerializer.Deserialize<Product>(cached);
                if (product != null)
                    return product;
            }

            var dbProduct = await _context.Products.FindAsync(id);

            if (dbProduct == null)
            {
                return NotFound();
            }


            var serialized = System.Text.Json.JsonSerializer.Serialize(dbProduct);
            await _demoService.SaveDataWithWriteThroughAsync(cacheKey, serialized);

            return dbProduct;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
            [HttpPut("{id}")]
            public async Task<IActionResult> PutProduct(int id, Product product)
            {
                if (id != product.Id)
                {
                    return BadRequest();
                }

                _context.Entry(product).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();


                    string cacheKey = $"product_{id}";
                    var serialized = System.Text.Json.JsonSerializer.Serialize(product);
                    await _demoService.SaveDataWithWriteThroughAsync(cacheKey, serialized);

                    var allProducts = await _context.Products.ToListAsync();
                    var allSerialized = System.Text.Json.JsonSerializer.Serialize(allProducts);
                    await _demoService.SaveDataWithWriteThroughAsync("products_all", allSerialized);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();


            string cacheKey = $"product_{product.Id}";
            var serialized = System.Text.Json.JsonSerializer.Serialize(product);
            await _demoService.SaveDataWithWriteThroughAsync(cacheKey, serialized);


            var allProducts = await _context.Products.ToListAsync();
            var allSerialized = System.Text.Json.JsonSerializer.Serialize(allProducts);
            await _demoService.SaveDataWithWriteThroughAsync("products_all", allSerialized);

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();


            string cacheKey = $"product_{id}";
            await _demoService.SaveDataWithWriteThroughAsync(cacheKey, null);


            var allProducts = await _context.Products.ToListAsync();
            var allSerialized = System.Text.Json.JsonSerializer.Serialize(allProducts);
            await _demoService.SaveDataWithWriteThroughAsync("products_all", allSerialized);

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
