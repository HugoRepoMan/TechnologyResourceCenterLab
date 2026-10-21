using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace TechnologyResourceCenter.Controllers
{
    public class KitsController : Controller
    {
        // Lista estática y readonly para conservar en memoria las altas, bajas y cambios de los Kits
        private static readonly List<Kit> _kitsList = new()
        {
            new Kit { Id = 1, Name = "Kit de Inicio Arduino", Category = "Electrónica", Level = "PRINCIPIANTE", DailyRate = 8.00m, Quantity = 15, IsOperational = true },
            new Kit { Id = 2, Name = "Kit Básico de Electrónica", Category = "Electrónica", Level = "PRINCIPIANTE", DailyRate = 5.00m, Quantity = 10, IsOperational = true },
            new Kit { Id = 3, Name = "Kit de Podcast", Category = "Audio", Level = "PRINCIPIANTE", DailyRate = 12.00m, Quantity = 6, IsOperational = true },
            new Kit { Id = 4, Name = "Kit de Fotografía", Category = "Multimedia", Level = "PRINCIPIANTE", DailyRate = 18.00m, Quantity = 0, IsOperational = true },
            new Kit { Id = 5, Name = "Kit IoT", Category = "Internet de las Cosas", Level = "INTERMEDIO", DailyRate = 15.00m, Quantity = 8, IsOperational = true },
            new Kit { Id = 6, Name = "Kit de Robótica", Category = "Robótica", Level = "INTERMEDIO", DailyRate = 20.00m, Quantity = 5, IsOperational = true },
            new Kit { Id = 7, Name = "Kit de Streaming", Category = "Multimedia", Level = "INTERMEDIO", DailyRate = 10.00m, Quantity = 4, IsOperational = false },
            new Kit { Id = 8, Name = "Kit de Inteligencia Artificial", Category = "IA", Level = "AVANZADO", DailyRate = 35.00m, Quantity = 2, IsOperational = true },
            new Kit { Id = 9, Name = "Kit de Redes Avanzadas", Category = "Redes", Level = "AVANZADO", DailyRate = 25.00m, Quantity = 3, IsOperational = true },
            new Kit { Id = 10, Name = "Kit de Realidad Virtual", Category = "RV", Level = "AVANZADO", DailyRate = 40.00m, Quantity = 1, IsOperational = true }
        };

        // READ: Muestra todos los kits tecnológicos
        public IActionResult Index()
        {
            return View(_kitsList);
        }

        // READ: Muestra el detalle completo de un kit específico
        public IActionResult Details(int id)
        {
            Kit? kit = _kitsList.FirstOrDefault(k => k.Id == id);

            if (kit is null)
                return NotFound();

            return View(kit);
        }

        // Vista especializada: ordena y agrupa los kits según su nivel de dificultad
        public IActionResult ByLevel()
        {
            var kitsOrderedByLevel = _kitsList.OrderBy(k => k.Level).ToList();
            return View(kitsOrderedByLevel);
        }

        // CREATE (GET): Muestra el formulario para dar de alta un nuevo kit
        public IActionResult Create()
        {
            return View(new Kit { Level = "PRINCIPIANTE", IsOperational = true, Quantity = 5, DailyRate = 10.00m });
        }

        // CREATE (POST): Verifica ModelState.IsValid y guarda el nuevo kit con un ID seguro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kit kit)
        {
            if (!ModelState.IsValid)
            {
                return View(kit);
            }

            kit.Id = _kitsList.Any() ? _kitsList.Max(x => x.Id) + 1 : 1;
            _kitsList.Add(kit);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Busca el kit a editar y lo envía a la vista
        public IActionResult Edit(int id)
        {
            Kit? kit = _kitsList.FirstOrDefault(k => k.Id == id);
            if (kit is null)
            {
                return NotFound();
            }
            return View(kit);
        }

        // UPDATE (POST): Valida y actualiza los datos del kit seleccionado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Kit updatedKit)
        {
            if (id != updatedKit.Id)
            {
                return NotFound();
            }

            Kit? existing = _kitsList.FirstOrDefault(k => k.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedKit);
            }

            existing.Name = updatedKit.Name;
            existing.Category = updatedKit.Category;
            existing.Level = updatedKit.Level;
            existing.DailyRate = updatedKit.DailyRate;
            existing.Quantity = updatedKit.Quantity;
            existing.IsOperational = updatedKit.IsOperational;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Muestra la pantalla de confirmación antes de eliminar el kit
        public IActionResult Delete(int id)
        {
            Kit? kit = _kitsList.FirstOrDefault(k => k.Id == id);
            if (kit is null)
            {
                return NotFound();
            }
            return View(kit);
        }

        // DELETE (POST): Remueve el kit de la lista tras confirmar la acción
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Kit? kit = _kitsList.FirstOrDefault(k => k.Id == id);
            if (kit is null)
            {
                return NotFound();
            }

            _kitsList.Remove(kit);
            return RedirectToAction(nameof(Index));
        }
    }
}
