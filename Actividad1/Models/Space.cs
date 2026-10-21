using System.ComponentModel.DataAnnotations;

namespace Actividad1.Models
{
    // Modelo que representa un espacio físico o laboratorio reservable
    public class Space
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del espacio o sala es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        [Display(Name = "Nombre del Espacio")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicación física es obligatoria (ej. Edificio A, Piso 2).")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "La ubicación debe tener entre 4 y 100 caracteres.")]
        [Display(Name = "Ubicación")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes ingresar la capacidad máxima de personas.")]
        [Range(1, 200, ErrorMessage = "La capacidad debe estar entre 1 y 200 personas.")]
        [Display(Name = "Capacidad (Personas)")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "La tarifa por hora es obligatoria.")]
        [Range(5.00, 500.00, ErrorMessage = "La tarifa por hora debe estar entre $5.00 y $500.00.")]
        [Display(Name = "Tarifa por Hora ($)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "¿Está Disponible?")]
        public bool IsAvailable { get; set; }

        // Determina la disponibilidad actual de la sala
        public string GetAvailabilityStatus()
        {
            return IsAvailable ? "Valido" : "No es valido";
        }

        // Clasifica el tipo de sala según el aforo que soporta
        public string GetCapacityType()
        {
            if (Capacity <= 10) return "Sala Pequeña";
            if (Capacity <= 30) return "Laboratorio Mediano";
            return "Auditorio / Sala Grande";
        }

        // Calcula el costo estimado por horas de reserva
        public decimal CalculateEstimatedCost(int hours)
        {
            // Solo permitimos reservas de entre 1 y 12 horas continuas
            if (hours <= 0 || hours > 12)
            {
                return 0;
            }

            return HourlyRate * hours;
        }
    }
}
