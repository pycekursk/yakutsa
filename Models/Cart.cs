using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using RetailCRMCore.Models;

using yakutsa.Data;

namespace yakutsa.Models
{
    public class Cart
    {
        [JsonIgnore]
        public HttpContext _httpContext;
        private double discountManualPercent;
        private double discountManualAmount;
        private PromoCode? promoCode;

        public Cart(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public double Price
        {
            get
            {
                double result = 0;
                if (DiscountManualPercent > 0)
                {
                    result = InitialPrice - (InitialPrice / 100 * DiscountManualPercent) > 0 ? InitialPrice - InitialPrice / 100 * DiscountManualPercent : 0;
                }
                if (DiscountManualAmount > 0)
                {
                    result = InitialPrice - discountManualAmount >= 0 ? InitialPrice - discountManualAmount : 0;
                }
                return result;
            }
        }

        public double DiscountManualAmount
        {
            get => discountManualAmount;
            set
            {
                discountManualAmount = value;
            }
        }

        public double DiscountManualPercent
        {
            get => discountManualPercent;
            set
            {
                discountManualPercent = value;
            }
        }

        public double InitialPrice
        {
            get
            {
                double result = 0;
                CartProducts.ForEach(x => result += x.Price);
                return result;
            }
        }

        public double DiscountTotal { get; set; }

        public PromoCode? PromoCode
        {
            get => promoCode;
            set
            {
                promoCode = value;
                if (value != null)
                {
                    if (value.PromoCodeType == PromoCodeType.Dynamic)
                    {
                        DiscountManualPercent = value.Value;
                    }
                    else
                    {
                        DiscountManualAmount = value.Value;
                    }
                }
            }
        }

        public double Weight
        {
            get
            {
                double result = 0;
                CartProducts.ForEach(x => result += x.Offer.weight);
                return result;
            }
        }

        public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

        public int Count { get { int count = 0; CartProducts.ForEach(cp => count += cp.Count); return count; } }

        public List<CartProduct> Add(Product product, Offer offer, int count)
        {
            var cartProduct = CartProducts.FirstOrDefault(cp => cp.Offer.id == offer.id);

            if (cartProduct != null) cartProduct.Count += count;
            else CartProducts.Add(new CartProduct { Product = product, Offer = offer, Count = count });

            _httpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(this));
            return CartProducts;
        }

        public List<CartProduct> ChangeCount(int productId, int offerId, int count = 0)
        {
            if (count == 0) CartProducts.Remove(CartProducts.FirstOrDefault(o => o.Product.id == productId && o.Offer.id == offerId)!);
            else
            {
                var cardProduct = CartProducts.FirstOrDefault(cp => cp.Product.id == productId && cp.Offer.id == offerId);
                cardProduct!.Count = count;
            }
            _httpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(this));
            return CartProducts;
        }

        public void UsePromoCode(int promoCodeId)
        {
            //PromoCode = promoCode;
            //PromoCode.PromoCodeState = PromoCodeState.NotActive;

            using (var ctx = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>()))
            {
                var loyalty = ctx.Loyalty.Include(l => l.PromoCodes).OrderBy(l => l.Id).Last();
                PromoCode = loyalty.PromoCodes?.FirstOrDefault(p => p.Id == promoCodeId);
                PromoCode.PromoCodeState = PromoCodeState.Used;
                ctx.Loyalty.Update(loyalty);
                ctx.SaveChanges();
            }

            _httpContext.Session.SetString("cart", Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
    }
}