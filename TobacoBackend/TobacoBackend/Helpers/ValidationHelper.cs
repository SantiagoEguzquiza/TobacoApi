using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.Helpers
{
    /// <summary>
    /// Helper para validaciones comunes
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida un modelo usando Data Annotations
        /// </summary>
        public static bool TryValidateModel(object model, out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model);
            return Validator.TryValidateObject(model, context, validationResults, true);
        }

        /// <summary>
        /// Valida que un ID sea válido (mayor a 0)
        /// </summary>
        public static bool IsValidId(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Valida que un ID sea válido (mayor a 0 o null)
        /// </summary>
        public static bool IsValidId(int? id)
        {
            return !id.HasValue || id.Value > 0;
        }

        /// <summary>
        /// Valida que un decimal esté en un rango válido
        /// </summary>
        public static bool IsValidDecimal(decimal value, decimal min = 0, decimal max = decimal.MaxValue)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Valida que una lista no esté vacía
        /// </summary>
        public static bool IsNotEmpty<T>(IEnumerable<T>? collection)
        {
            return collection != null && collection.Any();
        }

        /// <summary>
        /// Valida coordenadas geográficas
        /// </summary>
        public static bool IsValidCoordinates(double? lat, double? lon)
        {
            if (!lat.HasValue || !lon.HasValue)
                return true; // Opcional

            return lat.Value >= -90 && lat.Value <= 90 &&
                   lon.Value >= -180 && lon.Value <= 180;
        }
    }
}

