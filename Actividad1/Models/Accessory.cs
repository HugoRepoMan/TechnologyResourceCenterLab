using System.ComponentModel.DataAnnotations;

namespace Actividad1.Models
{
    // Modelo para los accesorios periféricos y consumibles tecnológicos
    public class Accessory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del accesorio es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        [Display(Name = "Nombre del Accesorio")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca del accesorio es obligatoria.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres.")]
        [Display(Name = "Marca")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría del accesorio es obligatoria.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La categoría debe tener entre 2 y 50 caracteres.")]
        [Display(Name = "Categoría")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(1.00, 2000.00, ErrorMessage = "El precio debe estar entre $1.00 y $2000.00.")]
        [Display(Name = "Precio Unitario ($)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La cantidad en inventario es obligatoria.")]
        [Range(0, 1000, ErrorMessage = "La cantidad debe estar entre 0 y 1000 unidades.")]
        [Display(Name = "Cantidad en Stock")]
        public int Quantity { get; set; }

        // Devuelve un texto descriptivo del stock actual para las etiquetas visuales
        public string GetInventoryStatus()
        {
            if (Quantity == 0) return "OUT OF STOCK";
            if (Quantity < 5) return "LOW STOCK";
            return "IN STOCK";
        }

        // Calcula el costo total si un usuario quiere adquirir múltiples unidades
        public decimal CalculateBulkCost(int requestedQuantity)
        {
            // La cantidad solicitada debe ser positiva y no superar el stock disponible
            if (requestedQuantity <= 0 || requestedQuantity > Quantity)
            {
                return 0;
            }
            return Price * requestedQuantity;
        }
    }
}
