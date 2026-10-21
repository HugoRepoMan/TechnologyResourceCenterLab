using System.ComponentModel.DataAnnotations;

namespace Actividad1.Models
{
    // Modelo que representa un equipo tecnológico dentro del centro de recursos
    public class Equipment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        [Display(Name = "Nombre del Equipo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes especificar una categoría (ej. Computación, Redes, Audio).")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La categoría debe tener entre 2 y 50 caracteres.")]
        [Display(Name = "Categoría")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca del fabricante es obligatoria.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres.")]
        [Display(Name = "Marca")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tarifa diaria es obligatoria.")]
        [Range(1.00, 1000.00, ErrorMessage = "La tarifa diaria debe estar entre $1.00 y $1000.00.")]
        [Display(Name = "Tarifa Diaria ($)")]
        public decimal DailyRate { get; set; }

        [Required(ErrorMessage = "Debes indicar la cantidad en stock.")]
        [Range(0, 500, ErrorMessage = "El stock debe ser un número entre 0 y 500 unidades.")]
        [Display(Name = "Unidades en Stock")]
        public int Stock { get; set; }

        [Display(Name = "¿Está Operativo?")]
        public bool IsOperational { get; set; }

        // Determina el estado del producto combinando si funciona y cuántas unidades quedan
        public string GetStatus()
        {
            if (!IsOperational)
            {
                return "Fuera de servicio";
            }
            else if (Stock == 0)
            {
                return "No valido";
            }
            else if (Stock == 1)
            {
                return "Ultima unidad";
            }
            else
            {
                return "Disponible";
            }
        }

        // Método auxiliar equivalente para mostrar texto de disponibilidad en vistas
        public string GetAvailabilityText() => GetStatus();

        // Calcula el costo estimado del alquiler según los días solicitados
        public decimal CalculateEstimatedCost(int days)
        {
            // Validamos que los días de alquiler sean mayores a 0 para evitar costos negativos
            if (days <= 0) return 0;

            return DailyRate * days;
        }
    }
}
