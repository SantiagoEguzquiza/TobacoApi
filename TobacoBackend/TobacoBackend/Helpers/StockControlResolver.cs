using TobacoBackend.Domain.Models;

namespace TobacoBackend.Helpers
{
    public static class StockControlResolver
    {
        public static bool ShouldControlStock(bool tenantDefaultStockControl, StockControlMode productMode)
        {
            return productMode switch
            {
                StockControlMode.ForceEnabled => true,
                StockControlMode.ForceDisabled => false,
                _ => tenantDefaultStockControl
            };
        }
    }
}
