using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SQLSaturdayPragueWebApi.Models;

namespace SQLSaturdayPragueWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLTContext _context;

        public ProductsController(AdventureWorksLTContext context)
        {
            _context = context;
        }

        // GET: api/Products/871
        [HttpGet("ID/{id}")]
        public async Task<ActionResult<Product_Model>> GetProduct_ID(int id)
        {
            var product = await _context.Product.FirstOrDefaultAsync(x => x.ProductId == id);

            return new Product_Model()
            {
                ProductID = product.ProductId,
                ListPrice = product.ListPrice,
                Name = product.Name,
                Photo = product.ThumbNailPhoto
            };
        }

        // GET: api/Products/bike
        [HttpGet("{productName}")]
        public ActionResult<IEnumerable<Product_Model>> GetProduct(string productName)
        {
            var list = new List<Product_Model>();

            List<Product> products;

            if (productName != "_")
            {
                products = _context.Product
                    .Include(x => x.ProductCategory)
                    .Include(x => x.ProductModel)
                    .Where(x => x.Name.Contains(productName)).ToList();
            }
            else
            {
                var ids = _context.SalesOrderDetail
                    .GroupBy(c => c.ProductId)
                    .Select(g => new { id = g.Key, Total = g.Sum(p => p.OrderQty) })
                    .OrderByDescending(c => c.Total)
                    //.Take(5)
                    .Select(x => x.id);

                var top5 = ids.Take(5).ToList();

                products = _context.Product
                    .Include(x => x.ProductCategory)
                    .Include(x => x.ProductModel)
                    .Where(x => top5.Contains(x.ProductId)).ToList();
            }

            if (products.Count() == 0)
                return list;

            foreach (var p in products)
            {
                var product_model = new Product_Model()
                {
                    Category = p.ProductCategory.Name,
                    Color = p.Color,
                    ListPrice = p.ListPrice,
                    Model = p.ProductModel.Name,
                    Name = p.Name,
                    Photo = p.ThumbNailPhoto,
                    ProductID = p.ProductId
                };

                list.Add(product_model);
            }

            return list;
        }
    }
}