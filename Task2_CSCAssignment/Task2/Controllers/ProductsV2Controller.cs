using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Task2.Models;

namespace Task2.Controllers
{
    public class ProductsV2Controller : ApiController
    {
        static readonly IProductRepository repository = new ProductRepository();

        [HttpGet]
        [Route("api/v2/products", Name = "getAllProducts")]
        public IEnumerable<Product> GetAllProducts()
        {
            return repository.GetAll();
        }

        [HttpGet]
        [Route("api/v2/products/{id:int:min(2)}", Name = "getProductById")]
        public Product GetProduct(int id)
        {
            Product item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item;
        }

        [HttpGet]
        [Route("api/v2/products", Name = "getProductByCategory")]
        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return repository.GetAll().Where(
                p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        [HttpPost]
        [Route("api/v2/addProduct")]
        public HttpResponseMessage PostProduct(Product item)
        {
            if (ModelState.IsValid)
            {
                item = repository.Add(item);

                var response = Request.CreateResponse<Product>(HttpStatusCode.Created, item);

                string uri = Url.Link("getProductById", new { id = item.Id });
                response.Headers.Location = new Uri(uri);

                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        [HttpPut]
        [Route("api/v2/updateProduct/{id:int}")]
        public HttpResponseMessage PutProduct(int id, Product product)
        {
            product.Id = id;
            if (!repository.Update(product))
            {
                //throw new HttpResponseException(HttpStatusCode.NotFound);
                return Request.CreateResponse(HttpStatusCode.NotFound, "ID cannot be found");
            }
            else
            {
                return Request.CreateResponse<Product>(HttpStatusCode.OK, product);
            }  
        }

        [HttpDelete]
        [Route("api/v2/deleteProduct/{id:int}")]
        public HttpResponseMessage DeleteProduct(int id)
        {
            Product item = repository.Get(id);
            if (item == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "ID cannot be found");
            }

            else
            {
                repository.Remove(id);
                return Request.CreateResponse(HttpStatusCode.OK, "Product with ID of " + id + " is deleted."); 
            }

        }


    }
}
