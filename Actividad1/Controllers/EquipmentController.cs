using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Actividad1.Controllers
{
    public class EquipmentController : Controller
    {
        // Usamos "static readonly" para que la lista viva en memoria durante toda la ejecución de la app.
        // Sin "static", cada petición HTTP crearía un controlador nuevo y perderíamos los cambios hechos en Create/Edit/Delete.
        private static readonly List<Equipment> _equipmentList = new()
        {
            new Equipment { Id = 1, Name = "Latitude 5550", Category = "Computación", Brand = "Dell", DailyRate = 25.00m, Stock = 4, IsOperational = true },
            new Equipment { Id = 2, Name = "MacBook Pro M3", Category = "Computación", Brand = "Apple", DailyRate = 45.00m, Stock = 1, IsOperational = true },
            new Equipment { Id = 3, Name = "EcoTank 4800", Category = "Impresión", Brand = "Epson", DailyRate = 15.00m, Stock = 0, IsOperational = true },
            new Equipment { Id = 4, Name = "Catalyst 9200", Category = "Redes", Brand = "Cisco", DailyRate = 35.00m, Stock = 5, IsOperational = false },
            new Equipment { Id = 5, Name = "Alpha a7 III", Category = "Fotografía", Brand = "Sony", DailyRate = 40.00m, Stock = 2, IsOperational = true },
            new Equipment { Id = 6, Name = "iPad Pro 12.9", Category = "Tableta", Brand = "Apple", DailyRate = 30.00m, Stock = 3, IsOperational = true },
            new Equipment { Id = 7, Name = "Galaxy Tab S9", Category = "Tableta", Brand = "Samsung", DailyRate = 25.00m, Stock = 1, IsOperational = true },
            new Equipment { Id = 8, Name = "ThinkPad T14", Category = "Computación", Brand = "Lenovo", DailyRate = 22.00m, Stock = 8, IsOperational = true },
            new Equipment { Id = 9, Name = "UniFi 6 Lite", Category = "Redes", Brand = "Ubiquiti", DailyRate = 10.00m, Stock = 12, IsOperational = true },
            new Equipment { Id = 10, Name = "Micrófono SM7B", Category = "Audio", Brand = "Shure", DailyRate = 18.00m, Stock = 0, IsOperational = true },
            new Equipment { Id = 11, Name = "Wireless GO II", Category = "Audio", Brand = "Rode", DailyRate = 20.00m, Stock = 2, IsOperational = true },
            new Equipment { Id = 12, Name = "UltraSharp 27", Category = "Monitor", Brand = "Dell", DailyRate = 15.00m, Stock = 6, IsOperational = true },
            new Equipment { Id = 13, Name = "Cámara EOS R5", Category = "Fotografía", Brand = "Canon", DailyRate = 50.00m, Stock = 1, IsOperational = true },
            new Equipment { Id = 14, Name = "Cámara web Brio 4K", Category = "Video", Brand = "Logitech", DailyRate = 12.00m, Stock = 4, IsOperational = false },
            new Equipment { Id = 15, Name = "Nighthawk AX12", Category = "Redes", Brand = "Netgear", DailyRate = 15.00m, Stock = 3, IsOperational = true },
            new Equipment { Id = 16, Name = "Tableta Cintiq 16", Category = "Dibujo", Brand = "Wacom", DailyRate = 28.00m, Stock = 2, IsOperational = true },
            new Equipment { Id = 17, Name = "Proyector TK850i", Category = "Monitor", Brand = "BenQ", DailyRate = 35.00m, Stock = 0, IsOperational = false },
            new Equipment { Id = 18, Name = "ROG Zephyrus G14", Category = "Computación", Brand = "Asus", DailyRate = 40.00m, Stock = 1, IsOperational = true },
            new Equipment { Id = 19, Name = "WH-1000XM5", Category = "Audio", Brand = "Sony", DailyRate = 15.00m, Stock = 5, IsOperational = true },
            new Equipment { Id = 20, Name = "PowerEdge R740", Category = "Servidor", Brand = "Dell", DailyRate = 100.00m, Stock = 1, IsOperational = true }
        };

        // READ: Muestra el catálogo completo de equipos
        public IActionResult Index()
        {
            return View(_equipmentList);
        }

        // READ: Muestra el detalle de un equipo específico buscando por su Id
        public IActionResult Details(int id)
        {
            Equipment? equipment = _equipmentList.FirstOrDefault(e => e.Id == id);
            if (equipment is null)
            {
                return NotFound(); // Si alguien pone un ID que no existe en la URL, devolvemos 404
            }
            return View(equipment);
        }

        // Vista especializada: filtra solo los equipos que funcionan y tienen stock disponible
        public IActionResult Available()
        {
            var availableEquipment = _equipmentList
                .Where(e => e.IsOperational && e.Stock > 0)
                .ToList();

            return View(availableEquipment);
        }

        // CREATE (GET): Muestra el formulario vacío para registrar un nuevo equipo
        public IActionResult Create()
        {
            return View(new Equipment { IsOperational = true, Stock = 1, DailyRate = 15.00m });
        }

        // CREATE (POST): Recibe los datos del formulario y los guarda si pasan las validaciones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Equipment equipment)
        {
            // Primero revisamos que todas las reglas (DataAnnotations) del modelo se cumplan
            if (!ModelState.IsValid)
            {
                return View(equipment); // Si hay errores, regresamos al formulario mostrando los mensajes
            }

            // Calculamos el nuevo ID buscando el máximo actual + 1 para no repetir IDs si antes se borró algún registro
            equipment.Id = _equipmentList.Any() ? _equipmentList.Max(x => x.Id) + 1 : 1;
            _equipmentList.Add(equipment);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Busca el equipo por ID y carga sus datos actuales en el formulario de edición
        public IActionResult Edit(int id)
        {
            Equipment? equipment = _equipmentList.FirstOrDefault(e => e.Id == id);
            if (equipment is null)
            {
                return NotFound();
            }
            return View(equipment);
        }

        // UPDATE (POST): Guarda los cambios hechos al equipo tras validar el formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Equipment updatedEquipment)
        {
            if (id != updatedEquipment.Id)
            {
                return NotFound();
            }

            Equipment? existing = _equipmentList.FirstOrDefault(e => e.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedEquipment);
            }

            // Actualizamos las propiedades del objeto que ya está en nuestra lista estática
            existing.Name = updatedEquipment.Name;
            existing.Category = updatedEquipment.Category;
            existing.Brand = updatedEquipment.Brand;
            existing.DailyRate = updatedEquipment.DailyRate;
            existing.Stock = updatedEquipment.Stock;
            existing.IsOperational = updatedEquipment.IsOperational;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Muestra una pantalla de confirmación antes de borrar definitivamente
        public IActionResult Delete(int id)
        {
            Equipment? equipment = _equipmentList.FirstOrDefault(e => e.Id == id);
            if (equipment is null)
            {
                return NotFound();
            }
            return View(equipment);
        }

        // DELETE (POST): Ejecuta el borrado real cuando el usuario confirma en pantalla
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Equipment? equipment = _equipmentList.FirstOrDefault(e => e.Id == id);
            if (equipment is null)
            {
                return NotFound();
            }

            _equipmentList.Remove(equipment);
            return RedirectToAction(nameof(Index));
        }
    }
}
