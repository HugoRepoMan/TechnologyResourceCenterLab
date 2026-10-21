using System.ComponentModel.DataAnnotations;

namespace Actividad1.Models
{
    // Modelo para los kits de trabajo agrupados por área y nivel de dificultad
    public class Kit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del kit es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre del kit debe tener entre 3 y 80 caracteres.")]
        [Display(Name = "Nombre del Kit")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría del kit es obligatoria.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La categoría debe tener entre 2 y 50 caracteres.")]
        [Display(Name = "Categoría")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes indicar el nivel (PRINCIPIANTE, INTERMEDIO o AVANZADO).")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "El nivel debe tener entre 4 y 30 caracteres.")]
        [Display(Name = "Nivel de Complejidad")]
        public string Level { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tarifa diaria del kit es obligatoria.")]
        [Range(1.00, 500.00, ErrorMessage = "La tarifa diaria debe estar entre $1.00 y $500.00.")]
        [Display(Name = "Tarifa Diaria ($)")]
        public decimal DailyRate { get; set; }

        [Required(ErrorMessage = "La cantidad disponible es obligatoria.")]
        [Range(0, 200, ErrorMessage = "La cantidad debe estar entre 0 y 200 kits.")]
        [Display(Name = "Cantidad Disponible")]
        public int Quantity { get; set; }

        [Display(Name = "¿Kit Operativo?")]
        public bool IsOperational { get; set; }

        // Determina el estado actual combinando cantidad disponible y condición operativa
        public string GetState()
        {
            if (!IsOperational) return "NEEDS REPAIR";
            if (Quantity == 0) return "OUT OF STOCK";
            if (Quantity <= 2) return "LOW STOCK";
            return "AVAILABLE";
        }

        // Devuelve el nivel de complejidad del kit para mostrar en tarjetas o reportes
        public string GetKitComplexity() => string.IsNullOrWhiteSpace(Level) ? "GENERAL" : Level.ToUpperInvariant();

        // Calcula el costo estimado de alquiler según el número de días
        public decimal CalculateEstimatedCost(int days)
        {
            // Los días solicitados deben ser mayores a cero
            if (days <= 0) return 0;
            return DailyRate * days;
        }
    }
}
