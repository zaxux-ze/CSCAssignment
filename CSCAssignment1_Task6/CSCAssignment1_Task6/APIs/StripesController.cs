using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSCAssignment1_Task6.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSCAssignment1_Task6.APIs
{
    [Route("api/[controller]")]
    public class StripesController : Controller
    {
        private static string apiSecretKey = "XXXX";
        private static string webhookSecret = "XXXX";

        // GET: api/<controller>
        [HttpGet("GetProductItems")]
        public IActionResult GetProductItem()
        {
            StripeConfiguration.ApiKey = apiSecretKey;

            List<StoreItem> storeItems = new List<StoreItem>();
            var options = new PriceListOptions { };
            var priceService = new PriceService();
            StripeList<Price> prices = priceService.List(options);

            foreach (Price priceData in prices)
            {
                StoreItem product = new StoreItem();
                product.ProductId = priceData.ProductId;
                product.PriceId = priceData.Id;
                product.Price = (long)priceData.UnitAmount;

                var productService = new ProductService();
                Product productItem = productService.Get(product.ProductId);
                product.Name = productItem.Name;
                product.Description = productItem.Description;
                storeItems.Add(product);
            }

            return new JsonResult(new { items = storeItems});
        }


        [HttpPost("checkoutSession")]
        public IActionResult CheckoutSession([FromForm] CreateCheckoutSessionRequest req)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = "https://localhost:44333/Stripe/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:44333/Stripe/Index",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = req.PriceId,
                        Quantity = 1,
                    },
                },
            };

            var service = new SessionService();
            try
            {
                var session = service.Create(options);
                return Ok(new { sessionId = session.Id });
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.StripeError.Message);
                return BadRequest(new { message = e.StripeError.Message });
            }
        }

        [HttpGet("viewInvoice/{sessionId}")]
        public IActionResult ViewInvoice(string sessionId) {

            StripeConfiguration.ApiKey = apiSecretKey;

            try
            {
                var sessionService = new SessionService();
                var session = sessionService.Get(sessionId);

                var subService = new SubscriptionService();
                Subscription sub = subService.Get(session.SubscriptionId);

                var invoiceService = new InvoiceService();
                Invoice inv = invoiceService.Get(sub.LatestInvoiceId);
                return Ok(new { urlLink = inv.HostedInvoiceUrl });

            }
            catch (StripeException e) {
                return BadRequest(new { message = e.StripeError.Message });
            }

            
        }

        [HttpPost("customerPortal")]
        public IActionResult CustomerPortal([FromForm] CustomerPortalRequest req)
        {
            var checkoutSessionId = req.SessionId;
            var checkoutService = new SessionService();
            var checkoutSession = checkoutService.Get(checkoutSessionId);

            var returnUrl = "https://localhost:44333/Stripe/Index";

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = checkoutSession.CustomerId,
                ReturnUrl = returnUrl,
            };
            var service = new Stripe.BillingPortal.SessionService();
            var session = service.Create(options);

            return Ok(session);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ListenToWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                Console.WriteLine($"Session ID: {session.Id}");
            }

            return Ok();
        }
    }
}
