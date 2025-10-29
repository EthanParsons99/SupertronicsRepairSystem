using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Services;
using static Google.Rpc.Context.AttributeContext.Types;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SupertronicsRepairSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private const string CartSessionCookieName = "CartSessionId";

        public CartController(ICartService cartService, IAuthService authService)
        {
            _cartService = cartService;
            _authService = authService;
        }

        private async Task<(string userId, string sessionId)> ResolveIdentifiersAsync()
        {
            var userId = await _authService.GetCurrentUserIdAsync();
            if (!string.IsNullOrEmpty(userId))
            {
                return (userId, null);
            }

            // Ensure session cookie exists
            if (!Request.Cookies.TryGetValue(CartSessionCookieName, out var sessionId) || string.IsNullOrEmpty(sessionId))
            {
                sessionId = System.Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = System.DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append(CartSessionCookieName, sessionId, cookieOptions);
            }

            return (null, sessionId);
        }

        public class AddRequest
        {
            public string ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AddRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.ProductId)) return BadRequest(new { success = false, message = "Invalid payload" });

            var ids = await ResolveIdentifiersAsync();
            var added = await _cartService.AddToCartAsync(ids.userId, ids.sessionId, req.ProductId, req.Quantity);
            if (!added) return BadRequest(new { success = false });

            var cart = await _cartService.GetCartAsync(ids.userId, ids.sessionId);
            var count = cart?.Items?.Sum(i => i.Quantity) ?? 0;
            return Ok(new { success = true, itemCount = count });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            var ids = await ResolveIdentifiersAsync();
            var cart = await _cartService.GetCartAsync(ids.userId, ids.sessionId) ?? await _cartService.CreateCartAsync(ids.userId, ids.sessionId);
            return Ok(cart);
        }

        public class UpdateRequest
        {
            public string ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.ProductId)) return BadRequest(new { success = false });

            var ids = await ResolveIdentifiersAsync();
            var ok = await _cartService.UpdateQuantityAsync(ids.userId, ids.sessionId, req.ProductId, req.Quantity);
            return Ok(new { success = ok });
        }

        public class RemoveRequest
        {
            public string ProductId { get; set; }
        }

        [HttpPost("Remove")]
        public async Task<IActionResult> Remove([FromBody] RemoveRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.ProductId)) return BadRequest(new { success = false });

            var ids = await ResolveIdentifiersAsync();
            var ok = await _cartService.RemoveItemAsync(ids.userId, ids.sessionId, req.ProductId);
            return Ok(new { success = ok });
        }
    }
}
