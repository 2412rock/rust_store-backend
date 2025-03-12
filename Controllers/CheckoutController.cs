using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Http.Response;
using PaypalServerSdk.Standard.Models;
using Rust_store_backend.Models.DB;
using Rust_store_backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Rust_store_backend.Controllers
{
    [ApiController]
    public class CheckoutController : Controller
    {
        private readonly OrdersController _ordersController;
        private readonly PaymentsController _paymentsController;
        private readonly Dictionary<string, CheckoutPaymentIntent> _paymentIntentMap;
        private Microsoft.Extensions.Configuration.IConfiguration _configuration { get; }
        private string _paypalClientId
        {
            get { return System.Environment.GetEnvironmentVariable("PAYPAL_CLIENT"); }
        }
        private string _paypalClientSecret
        {
            get { return System.Environment.GetEnvironmentVariable("PAYPAL_SECRET"); }
            
        }

        private readonly ILogger<CheckoutController> _logger;
        private readonly RustDBContext _context;
        private readonly RCONService _rcon;

        public CheckoutController(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<CheckoutController> logger, RustDBContext context, RCONService rcon)
        {
            _rcon = rcon;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _paymentIntentMap = new Dictionary<string, CheckoutPaymentIntent>
        {
            { "CAPTURE", CheckoutPaymentIntent.Capture },
            { "AUTHORIZE", CheckoutPaymentIntent.Authorize }
        };


            // Initialize the PayPal SDK client
            PaypalServerSdkClient client = new PaypalServerSdkClient.Builder()
                .Environment(PaypalServerSdk.Standard.Environment.Production)//.Environment(PaypalServerSdk.Standard.Environment.Production)
                .ClientCredentialsAuth(
                    new ClientCredentialsAuthModel.Builder(_paypalClientId, _paypalClientSecret).Build()
                )
                .LoggingConfig(config =>
                    config
                        .LogLevel(LogLevel.Information)
                        .RequestConfig(reqConfig => reqConfig.Body(true))
                        .ResponseConfig(respConfig => respConfig.Headers(true))
                )
                .Build();

            _ordersController = client.OrdersController;
            _paymentsController = client.PaymentsController;
        }


        [HttpPost("api/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] dynamic cart)
        {
            try
            {

                var result = await _CreateOrder(cart);
                return StatusCode((int)result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to create order:", ex);
                return StatusCode(500, new { error = "Failed to create order. " + ex.Message });
            }
        }

        private async Task<dynamic> _CreateOrder(dynamic cart)
        {
            Console.WriteLine("Creating Order, reading cart params");
            var cartArray = cart.GetProperty("cart");
            var cartElement1 = cartArray[0];
            string steamId = cartElement1.GetProperty("steamId").GetString();
            string game = cartElement1.GetProperty("game").GetString();
            var cart2 = cartElement1.GetProperty("cart");
            JsonElement cartItems = cart2.GetProperty("items");

            if (string.IsNullOrEmpty(steamId))
            {
                throw new ArgumentException("SteamId cannot be null or empty.");
            }

            var user = _context.Users.FirstOrDefault(u => u.SteamId == steamId);
            if (user == null)
            {
                user = new DBUser { SteamId = steamId };
                _context.Users.Add(user);
                _context.SaveChanges();
                
            }

            var order = new DBOrder
            {
                Game = game,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                TransactionFinalized = false,
                TransactionFinalizedButPlayerDidNotGet = false,
                OrderItems = new List<DBOrderItem>(),
                Total = 0
            };

            foreach (JsonElement item in cartItems.EnumerateArray())
            {
                //string productName = item.GetProperty("productName").GetString();
                int price = item.GetProperty("price").GetInt32();
                int quantity = item.GetProperty("numberOfItems").GetInt32();

                string ingameProduct;
                try
                {
                    ingameProduct = GetIngameProducts(price, game);
                }
                catch(Exception e)
                {
                    throw new Exception($"Ingame cash value invalid {steamId}");
                }

                var product = _context.Products.FirstOrDefault(p => p.ProductName == ingameProduct);
                if (product == null)
                {
                    throw new Exception($"Product name invalid {steamId}");
                }
                

                var orderItem = new DBOrderItem
                {
                    ProductId = product.Id,
                    NumberOfItems = quantity,
                    Subtotal = price * quantity
                };
                order.Total += orderItem.Subtotal;
                order.OrderItems.Add(orderItem);
                _context.SaveChanges();
            }

            order.TotalNumberOfItems = order.OrderItems.Sum(oi => oi.NumberOfItems);
            

            var pricePerItem = 1; // Example unit price

            OrdersCreateInput ordersCreateInput = new OrdersCreateInput
            {
                Body = new OrderRequest
                {
                    Intent = _paymentIntentMap["CAPTURE"],
                    PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        Amount = new AmountWithBreakdown { CurrencyCode = "USD", MValue = order.Total.ToString(), },
                    },
                },

                },
            };


            ApiResponse<Order> result = await _ordersController.OrdersCreateAsync(ordersCreateInput);
            //var orderId = result.Data.Id;
            //var order = new OrderDB();
            //order.OrderId = orderId;
            //order.SteamId = steamId;
            //order.Amount = GetIngameCash(int.Parse(cartQuantity));
            order.PaypalOrderId = result.Data.Id;
            _context.Orders.Add(order);
            order.TransactionFinalized = false;
            _context.SaveChanges();
            return result;
        }

        private string GetIngameProducts(int quantity, string game)
        {
            if(game == "rust")
            {
                switch (quantity)
                {
                    case 1:
                        return "120 shop points";
                    case 3:
                        return "500 shop points";
                    case 5:
                        return "1000 shop points";
                    case 10:
                        return "2300 shop points";
                    case 30:
                        return "7000 shop points";

                    default:
                        throw new Exception("Invalid amount");
                }
            }
            else if(game == "eco")
            {
                switch (quantity)
                {
                    case 1:
                        return "1 crafting permit";
                    case 3:
                        return "5 crafting permits";
                    case 5:
                        return "10 crafting permits";
                    //case 10:
                    //    return "23 crafting permits";
                    //case 30:
                    //    return "70 crafting permits";

                    default:
                        throw new Exception("Invalid amount");
                }
            }
            else
            {
                throw new Exception("Invalid game");
            }
            
        }


        [HttpPost("api/orders/{orderID}/capture")]
        public async Task<IActionResult> CaptureOrder(string orderID)
        {
            try
            {
                var result = await _CaptureOrder(orderID);
                DBOrder order = null;
                try
                {
                    if(orderID != null)
                    {
                        order = await _context.Orders.Include(e => e.OrderItems).Include(e => e.User).FirstOrDefaultAsync(e => e.PaypalOrderId == orderID);
                        if (order != null)
                        {
                            order.TransactionFinalized = true;
                            _context.Update(order);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        return StatusCode(500, "Order id was null");
                    }
                    if(order.Game == "rust")
                    {
                        var steamId = order.User.SteamId;
                        foreach (var item in order.OrderItems)
                        {
                            for (int i = 0; i < item.NumberOfItems; i++)
                            {
                                var product = await _context.Products.FirstOrDefaultAsync(e => e.Id == item.ProductId);
                                await RustGivePlayerProduct(product.ProductName, steamId);
                            }
                        }
                    }
                    else if(order.Game == "eco")
                    {
                        var steamId = order.User.SteamId;
                        foreach(var item in order.OrderItems)
                        {
                            for (int i = 0; i < item.NumberOfItems; i++)
                            {
                                var product = await _context.Products.FirstOrDefaultAsync(e => e.Id == item.ProductId);
                                await EcoGivePlayerProduct(product.ProductName, steamId);
                            }
                        }
                    }
                     
                }
                catch
                {
                    if(order != null)
                    {
                        order.TransactionFinalizedButPlayerDidNotGet = true;
                        _context.Update(order);
                        await _context.SaveChangesAsync();
                    }
                    
                    Console.WriteLine($"Failed to deposit ingame money for order id {orderID}");
                    return StatusCode(403);
                }
                
                Console.WriteLine("Order completed!");

                return StatusCode((int)result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to capture order:", ex);
                return StatusCode(500, new { error = "Failed to capture order." });
            }
        }

        private async Task EcoGivePlayerProduct(string productName, string steamId)
        {   
            if(productName == "1 crafting permit")
            {
                await _rcon.EcoRawCommandAsync($"gc-givepermit {steamId}");
            }
            else if (productName == "5 crafting permits")
            {
                for(int i=0; i < 5; i++)
                {
                    await _rcon.EcoRawCommandAsync($"gc-givepermit {steamId}");
                }
            }
            else if (productName == "10 crafting permits")
            {
                for (int i = 0; i < 10; i++)
                {
                    await _rcon.EcoRawCommandAsync($"gc-givepermit {steamId}");
                }
            }
            //else if (productName == "23 crafting permits")
            //{
            //    await _rcon.EcoRawCommandAsync($"gc-givepermit");
            //}
            //else if (productName == "70 crafting permits")
            //{
            //    await _rcon.EcoRawCommandAsync($"gc-givepermit");
            //}
        }

        private async Task RustGivePlayerProduct(string productName, string steamId)
        {
            if (productName == "120 shop points")
            {
                await _rcon.DepositCommandAsync(120, steamId);
            }
            else if (productName == "500 shop points")
            {
                await _rcon.DepositCommandAsync(500, steamId);
            }
            else if (productName == "1000 shop points")
            {
                await _rcon.DepositCommandAsync(1000, steamId);
            }
            else if (productName == "2300 shop points")
            {
                await _rcon.DepositCommandAsync(2300, steamId);
            }
            else if (productName == "7000 shop points")
            {
                await _rcon.DepositCommandAsync(7000, steamId);
            }
        }

        private async Task<dynamic> _CaptureOrder(string orderID)
        {
            OrdersCaptureInput ordersCaptureInput = new OrdersCaptureInput { Id = orderID, };

            ApiResponse<Order> result = await _ordersController.OrdersCaptureAsync(ordersCaptureInput);

            return result;
        }





    }
}
