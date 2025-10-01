using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class PricingService
    {
        public class PricingResult
        {
            public decimal TotalPrice { get; set; }
            public List<PricingBreakdown> Breakdown { get; set; } = new List<PricingBreakdown>();
            public decimal? SpecialPrice { get; set; }
            public decimal? GlobalDiscount { get; set; }
            public decimal FinalPrice { get; set; }
        }

        public class PricingBreakdown
        {
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// Calculates the optimal pricing for a given quantity of a product
        /// </summary>
        /// <param name="product">The product with quantity prices</param>
        /// <param name="requestedQuantity">The quantity requested</param>
        /// <param name="specialPrice">Special price for unit quantity (if any)</param>
        /// <param name="globalDiscount">Global discount percentage (if any)</param>
        /// <returns>Pricing result with breakdown</returns>
        public PricingResult CalculateOptimalPricing(Producto product, int requestedQuantity, decimal? specialPrice = null, decimal? globalDiscount = null)
        {
            // Use the product's base price as unit price
            var unitPrice = new ProductQuantityPrice
            {
                Quantity = 1,
                TotalPrice = specialPrice ?? product.Precio
            };

            // Get all available quantity prices (only packs, quantity > 1)
            var availablePrices = (product.QuantityPrices ?? new List<ProductQuantityPrice>())
                .Where(qp => qp.Quantity > 1)
                .OrderBy(qp => qp.Quantity)
                .ToList();

            // Add the unit price first
            availablePrices.Insert(0, unitPrice);

            // If no packs are configured, just use unit pricing
            if (availablePrices.Count == 1)
            {
                return new PricingResult
                {
                    TotalPrice = unitPrice.TotalPrice * requestedQuantity,
                    Breakdown = new List<PricingBreakdown>
                    {
                        new PricingBreakdown
                        {
                            Quantity = 1,
                            UnitPrice = unitPrice.TotalPrice,
                            TotalPrice = unitPrice.TotalPrice * requestedQuantity,
                            Count = requestedQuantity
                        }
                    },
                    SpecialPrice = specialPrice,
                    FinalPrice = unitPrice.TotalPrice * requestedQuantity
                };
            }

            // Calculate optimal combination using dynamic programming
            var result = CalculateOptimalCombination(availablePrices, requestedQuantity);
            result.SpecialPrice = specialPrice;

            // Apply global discount if provided
            if (globalDiscount.HasValue && globalDiscount > 0)
            {
                result.GlobalDiscount = globalDiscount.Value;
                result.FinalPrice = result.TotalPrice * (1 - globalDiscount.Value / 100);
            }
            else
            {
                result.FinalPrice = result.TotalPrice;
            }

            return result;
        }

        private PricingResult CalculateOptimalCombination(List<ProductQuantityPrice> availablePrices, int requestedQuantity)
        {
            // Create a dictionary for quick lookup
            var priceDict = availablePrices.ToDictionary(qp => qp.Quantity, qp => qp.TotalPrice);

            // Dynamic programming approach
            var dp = new decimal[requestedQuantity + 1];
            var parent = new int[requestedQuantity + 1];
            var usedQuantities = new Dictionary<int, int>();

            // Initialize DP array
            for (int i = 1; i <= requestedQuantity; i++)
            {
                dp[i] = decimal.MaxValue;
            }
            dp[0] = 0;

            // Fill DP array
            for (int i = 1; i <= requestedQuantity; i++)
            {
                foreach (var qp in availablePrices)
                {
                    if (qp.Quantity <= i)
                    {
                        var cost = dp[i - qp.Quantity] + qp.TotalPrice;
                        if (cost < dp[i])
                        {
                            dp[i] = cost;
                            parent[i] = qp.Quantity;
                        }
                    }
                }
            }

            // If we can't find a solution, fall back to unit prices
            if (dp[requestedQuantity] == decimal.MaxValue)
            {
                var unitPrice = availablePrices.First(qp => qp.Quantity == 1);
                return new PricingResult
                {
                    TotalPrice = unitPrice.TotalPrice * requestedQuantity,
                    Breakdown = new List<PricingBreakdown>
                    {
                        new PricingBreakdown
                        {
                            Quantity = 1,
                            UnitPrice = unitPrice.TotalPrice,
                            TotalPrice = unitPrice.TotalPrice * requestedQuantity,
                            Count = requestedQuantity
                        }
                    }
                };
            }

            // Reconstruct the solution
            var breakdown = new Dictionary<int, int>();
            int remaining = requestedQuantity;
            while (remaining > 0)
            {
                int quantity = parent[remaining];
                if (breakdown.ContainsKey(quantity))
                    breakdown[quantity]++;
                else
                    breakdown[quantity] = 1;
                remaining -= quantity;
            }

            // Create breakdown list
            var breakdownList = new List<PricingBreakdown>();
            foreach (var kvp in breakdown.OrderBy(x => x.Key))
            {
                var unitPrice = priceDict[kvp.Key] / kvp.Key;
                breakdownList.Add(new PricingBreakdown
                {
                    Quantity = kvp.Key,
                    UnitPrice = unitPrice,
                    TotalPrice = priceDict[kvp.Key] * kvp.Value,
                    Count = kvp.Value
                });
            }

            return new PricingResult
            {
                TotalPrice = dp[requestedQuantity],
                Breakdown = breakdownList
            };
        }

        /// <summary>
        /// Validates that a product has valid quantity prices
        /// </summary>
        /// <param name="quantityPrices">List of quantity prices to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateQuantityPrices(List<ProductQuantityPriceDTO> quantityPrices)
        {
            if (quantityPrices == null)
                return true; // Empty list is valid (no packs configured)

            // Check for duplicate quantities
            var quantities = quantityPrices.Select(qp => qp.Quantity).ToList();
            if (quantities.Count != quantities.Distinct().Count())
                return false;

            // Check for valid quantities and prices
            foreach (var qp in quantityPrices)
            {
                if (qp.Quantity < 2 || qp.TotalPrice <= 0) // Only allow packs (quantity >= 2)
                    return false;
            }

            return true;
        }
    }
}
