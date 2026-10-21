# UNIVERSIDAD DE LAS FUERZAS ARMADAS — ESPE
## SEDE SANTO DOMINGO DE LOS TSÁCHILAS

| **Datos Institucionales** | **Información de la Práctica** |
| :--- | :--- |
| **Departamento:** Ciencias de la Computación | **Fecha:** 20 de octubre de 2026 |
| **Carrera:** Ingeniería en Tecnologías de la Información | **Docente:** Ing. [Nombre del Docente], Mgtr. |
| **Asignatura:** Desarrollo Web / Aplicaciones Web MVC | **Práctica N°:** 02 |
| **Nivel / Paralelo:** Sexto Nivel | **Calificación:** ___________________ |

---

<div align="center">

### <span style="font-size:14pt">**Implementación de Operaciones CRUD, Validaciones con DataAnnotations y Persistencia en Memoria en ASP.NET Core MVC (.NET 10)**</span>

<span style="font-size:12pt">**Autor:** [Nombre del Estudiante]</span>  
<span style="font-size:11pt">*Correo institucional: usuario@espe.edu.ec*</span>

</div>

---

### **Resumen**
<p style="font-size:11pt; text-align:justify;">
El presente informe técnico documenta la ampliación arquitectónica y funcional del sistema web <i>Technology Resource Center</i> (<code>dope.resources</code>), desarrollado bajo el patrón Modelo-Vista-Controlador (MVC) en la plataforma ASP.NET Core (.NET 10). El trabajo aborda la resolución del problema de volatilidad de datos entre peticiones HTTP mediante la refactorización de las colecciones internas hacia estructuras estáticas de solo lectura (<code>private static readonly List&lt;T&gt;</code>) en los cuatro controladores principales: <code>EquipmentController</code>, <code>SpacesController</code>, <code>KitsController</code> y <code>AccessoriesController</code>. Asimismo, se fortaleció la capa de dominio incorporando reglas declarativas de validación con atributos <code>DataAnnotations</code> (<code>[Required]</code>, <code>[StringLength]</code>, <code>[Range]</code> y <code>[Display]</code>), conservando intactos los métodos de lógica de negocio para el cálculo de tarifas y estados operativos. Se implementó el ciclo completo de operaciones CRUD (Create, Read, Update y Delete) con verificación estricta de <code>ModelState.IsValid</code>, protección contra ataques CSRF mediante <code>[ValidateAntiForgeryToken]</code>, generación segura de identificadores libres de colisiones y manejo de errores HTTP 404 (<code>NotFound()</code>). Finalmente, se ejecutó una matriz exhaustiva de 28 casos de prueba (siete por módulo) que comprueba la estabilidad, integridad referencial en memoria y coherencia visual del sistema.
</p>

**Palabras Clave:** ASP.NET Core MVC, DataAnnotations, CRUD.

---

## 1. Introducción

En el desarrollo de aplicaciones empresariales modernas sobre la web, el patrón arquitectónico Modelo-Vista-Controlador (MVC) permite desacoplar la representación de los datos de negocio, la lógica de control de flujo y la interfaz de usuario. Sin embargo, dado que el protocolo HTTP es inherentemente sin estado (*stateless*), el framework ASP.NET Core instancia un nuevo objeto controlador por cada petición entrante. Cuando una aplicación en fase de prototipado utiliza colecciones en memoria (`List<T>`) declaradas como campos de instancia, cualquier inserción, actualización o eliminación se pierde inmediatamente tras ejecutarse una redirección (`RedirectToAction`).

La presente práctica resuelve esta limitación técnica dentro del proyecto **Technology Resource Center**, un portal académico diseñado para administrar préstamos y reservas de cuatro tipos de recursos tecnológicos: Equipos (`Equipment`), Espacios Físicos (`Space`), Kits de Laboratorio (`Kit`) y Accesorios (`Accessory`). A lo largo del documento se detalla la implementación de listas estáticas en memoria, el blindaje de los modelos mediante metadatos de validación tanto en cliente como en servidor, la creación de las vistas faltantes para el módulo de espacios y la construcción de formularios tipados con Razor y Bootstrap.

---

## 2. Objetivo(s)

### 2.1. Objetivo General
Implementar un sistema CRUD completo, seguro y validado sobre los cuatro módulos del proyecto *Technology Resource Center* en ASP.NET Core MVC (.NET 10), garantizando la persistencia en memoria mediante colecciones estáticas y el cumplimiento de las reglas de negocio definidas.

### 2.2. Objetivos Específicos
1. Corregir la pérdida de estado entre peticiones HTTP convirtiendo las listas de datos de los cuatro controladores en colecciones estáticas de solo lectura (`private static readonly List<T>`).
2. Incorporar validaciones declarativas mediante atributos `DataAnnotations` (`[Required]`, `[StringLength]`, `[Range]`, `[Display]`) en las clases `Equipment`, `Space`, `Kit` y `Accessory`, preservando sus métodos de negocio originales.
3. Desarrollar las acciones HTTP `GET` y `POST` para las operaciones `Create`, `Edit` y `Delete`, incluyendo validación con `ModelState.IsValid`, cálculo seguro de identificadores y manejo de respuestas `NotFound()`.
4. Diseñar e integrar las vistas Razor correspondientes a los cuatro módulos (incluyendo `Details.cshtml` y `Available.cshtml` del módulo `Spaces`), respetando la identidad gráfica oscura del sistema (`content-card`, `dope-card`, badges e iconografía `bi-*`).

---

## 3. Marco Teórico

### 3.1. Ciclo de Vida de los Controladores en ASP.NET Core MVC y Listas Estáticas
En ASP.NET Core MVC, el contenedor de inyección de dependencias registra los controladores con un ciclo de vida transitorio (*Transient*) por petición HTTP. Esto significa que cuando el usuario envía un formulario mediante `POST` y el servidor responde con un código HTTP `302 Found` (`RedirectToAction("Index")`), el navegador realiza una nueva petición `GET` atendida por una nueva instancia del controlador.
Al declarar la colección de datos como `private static readonly List<T>`, la lista pasa a pertenecer al tipo de la clase y no a una instancia individual, permaneciendo alojada en el *Heap* administrado del CLR durante todo el tiempo de ejecución del proceso web. El modificador `readonly` garantiza además que la referencia a la lista no pueda ser reemplazada accidentalmente por `null` u otra instancia.

### 3.2. Validación de Modelos con DataAnnotations y `ModelState`
El espacio de nombres `System.ComponentModel.DataAnnotations` provee atributos declarativos que permiten definir restricciones directamente sobre las propiedades del modelo:
- **`[Required]`:** Exige que el campo no sea nulo ni contenga una cadena vacía.
- **`[StringLength]`:** Limita la longitud mínima y máxima de caracteres aceptados.
- **`[Range]`:** Restringe valores numéricos o decimales dentro de un intervalo cerrado `[mínimo, máximo]`.
- **`[Display]`:** Personaliza el nombre amigable del campo en las etiquetas generadas por los *Tag Helpers* (`<label asp-for="...">`).

Durante el proceso de *Model Binding*, ASP.NET Core evalúa automáticamente estos atributos y registra cualquier infracción en el diccionario `ModelState`. La propiedad booleana `ModelState.IsValid` permite al controlador interceptar datos malformados antes de alterar la colección de datos.

### 3.3. Seguridad en Formularios POST (`[ValidateAntiForgeryToken]`)
Las operaciones que mutan estado (`Create`, `Edit`, `Delete`) están expuestas a ataques de falsificación de peticiones en sitios cruzados (*CSRF / XSRF*). El uso combinado del Form Tag Helper (`<form asp-action="...">`) y el atributo `[ValidateAntiForgeryToken]` en el método de acción `POST` obliga a validar un token criptográfico oculto (`__RequestVerificationToken`), rechazando peticiones originadas fuera de la aplicación.

---

## 4. Descripción del Procedimiento

### 4.1. Materiales y Herramientas Utilizadas
- **Entorno de Desarrollo:** Visual Studio 2022 / SDK de .NET 10 (`net10.0`).
- **Framework Web:** ASP.NET Core MVC con motor de vistas Razor.
- **Frontend y Estilos:** Bootstrap 5, Bootstrap Icons (`v1.11.3`), CSS3 personalizado (`site.css` con tokens de diseño *dope.security*) y jQuery Unobtrusive Validation.

---

### 4.2. Paso 1: Actualización de Modelos con Validaciones (`DataAnnotations`)
En el primer paso se modificaron los cuatro modelos del dominio (`Equipment`, `Space`, `Kit` y `Accessory`) agregando las anotaciones `[Required]`, `[StringLength]`, `[Range]` y `[Display]` con mensajes personalizados en español, conservando intactos los métodos de cálculo y evaluación de estado. A continuación se presenta el código fuente completo de cada modelo:

#### 4.2.1. Modelo `Models/Equipment.cs`
```csharp
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
```

#### 4.2.2. Modelo `Models/Space.cs`
```csharp
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
```

#### 4.2.3. Modelo `Models/Kit.cs`
```csharp
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
```

#### 4.2.4. Modelo `Models/Accessory.cs`
```csharp
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
```

---

### 4.3. Paso 2: Persistencia Estática, Cálculo Seguro de IDs y Controladores CRUD
En los cuatro controladores se aseguraron las listas como `private static readonly List<T>` para mantener la persistencia en memoria entre peticiones `GET` y `POST`. Asimismo, se programaron las acciones completas `Index`, `Details`, `Create (GET/POST)`, `Edit (GET/POST)`, `Delete (GET/POST)` y la acción especializada de cada módulo (`Available`, `ByLevel`, `ByCategory`), utilizando el cálculo seguro de IDs `lista.Any() ? lista.Max(x => x.Id) + 1 : 1`.

#### 4.3.1. Controlador `Controllers/EquipmentController.cs`
```csharp
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
```

#### 4.3.2. Controlador `Controllers/SpacesController.cs`
```csharp
using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace TechnologyResourceCenter.Controllers
{
    public class SpacesController : Controller
    {
        // Lista estática y de solo lectura para mantener los datos vivos entre peticiones HTTP (GET y POST)
        private static readonly List<Space> _spacesList = new()
        {
            new Space { Id = 1, Name = "Laboratorio de Computación", Location = "Edificio A, Piso 2", Capacity = 30, HourlyRate = 15.00m, IsAvailable = true },
            new Space { Id = 2, Name = "Sala Multimedia", Location = "Edificio B, Planta Baja", Capacity = 15, HourlyRate = 20.00m, IsAvailable = false },
            new Space { Id = 3, Name = "Sala de Streaming", Location = "Edificio B, Piso 1", Capacity = 4, HourlyRate = 10.00m, IsAvailable = true },
            new Space { Id = 4, Name = "Sala de Innovación", Location = "Edificio C, Piso 3", Capacity = 50, HourlyRate = 35.00m, IsAvailable = true },
            new Space { Id = 5, Name = "Laboratorio de Robótica", Location = "Edificio A, Sótano", Capacity = 20, HourlyRate = 25.00m, IsAvailable = false }
        };

        // READ: Muestra todos los espacios registrados
        public IActionResult Index()
        {
            return View(_spacesList);
        }

        // READ: Muestra la información detallada de un espacio según su ID
        public IActionResult Details(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);

            if (space is null)
                return NotFound();

            return View(space);
        }

        // Vista especializada: muestra únicamente los espacios que están habilitados para reserva
        public IActionResult Available()
        {
            var availableSpaces = _spacesList.Where(s => s.IsAvailable).ToList();
            return View(availableSpaces);
        }

        // CREATE (GET): Abre el formulario para registrar un nuevo espacio físico
        public IActionResult Create()
        {
            return View(new Space { IsAvailable = true, Capacity = 20, HourlyRate = 15.00m });
        }

        // CREATE (POST): Valida los datos ingresados y agrega el espacio a la lista en memoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Space space)
        {
            if (!ModelState.IsValid)
            {
                return View(space);
            }

            // Generación segura de ID para evitar duplicados después de eliminar elementos
            space.Id = _spacesList.Any() ? _spacesList.Max(x => x.Id) + 1 : 1;
            _spacesList.Add(space);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Carga en el formulario los datos del espacio que se quiere modificar
        public IActionResult Edit(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }
            return View(space);
        }

        // UPDATE (POST): Aplica los cambios al espacio si las validaciones del modelo son correctas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Space updatedSpace)
        {
            if (id != updatedSpace.Id)
            {
                return NotFound();
            }

            Space? existing = _spacesList.FirstOrDefault(s => s.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedSpace);
            }

            existing.Name = updatedSpace.Name;
            existing.Location = updatedSpace.Location;
            existing.Capacity = updatedSpace.Capacity;
            existing.HourlyRate = updatedSpace.HourlyRate;
            existing.IsAvailable = updatedSpace.IsAvailable;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Presenta la vista de confirmación con los datos del espacio a eliminar
        public IActionResult Delete(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }
            return View(space);
        }

        // DELETE (POST): Elimina el espacio de la lista una vez confirmado por el usuario
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }

            _spacesList.Remove(space);
            return RedirectToAction(nameof(Index));
        }
    }
}
```

#### 4.3.3. Controlador `Controllers/KitsController.cs`
```csharp
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
```

#### 4.3.4. Controlador `Controllers/AccessoriesController.cs`
```csharp
using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace TechnologyResourceCenter.Controllers
{
    public class AccessoriesController : Controller
    {
        // Lista estática y de solo lectura para persistir los accesorios en memoria mientras corre la aplicación
        private static readonly List<Accessory> _accessoriesList = new()
        {
            new Accessory { Id = 1, Name = "Wireless Microphone", Brand = "Rode", Category = "Audio", Price = 250.00m, Quantity = 8 },
            new Accessory { Id = 2, Name = "Brio 4K Webcam", Brand = "Logitech", Category = "Video", Price = 160.00m, Quantity = 12 },
            new Accessory { Id = 3, Name = "Heavy Duty Tripod", Brand = "Manfrotto", Category = "Video", Price = 120.00m, Quantity = 5 },
            new Accessory { Id = 4, Name = "7-in-1 USB-C Hub", Brand = "Anker", Category = "Connectivity", Price = 45.00m, Quantity = 25 },
            new Accessory { Id = 5, Name = "HDMI to USB-C Adapter", Brand = "Satechi", Category = "Connectivity", Price = 20.00m, Quantity = 30 },
            new Accessory { Id = 6, Name = "8-Port Network Switch", Brand = "TP-Link", Category = "Networking", Price = 35.00m, Quantity = 15 },
            new Accessory { Id = 7, Name = "Wi-Fi 6 Router", Brand = "Asus", Category = "Networking", Price = 180.00m, Quantity = 6 },
            new Accessory { Id = 8, Name = "Noise Cancelling Headphones", Brand = "Sony", Category = "Audio", Price = 350.00m, Quantity = 10 },
            new Accessory { Id = 9, Name = "Elgato HD60 X Capture Card", Brand = "Corsair", Category = "Video", Price = 190.00m, Quantity = 4 },
            new Accessory { Id = 10, Name = "1TB Portable SSD", Brand = "SanDisk", Category = "Storage", Price = 110.00m, Quantity = 20 },
            new Accessory { Id = 11, Name = "Mechanical Keyboard MX", Brand = "Logitech", Category = "Computing", Price = 100.00m, Quantity = 14 },
            new Accessory { Id = 12, Name = "MX Master 3S Mouse", Brand = "Logitech", Category = "Computing", Price = 99.00m, Quantity = 18 },
            new Accessory { Id = 13, Name = "2TB External HDD", Brand = "Seagate", Category = "Storage", Price = 75.00m, Quantity = 22 },
            new Accessory { Id = 14, Name = "DisplayPort to HDMI Cable", Brand = "Belkin", Category = "Connectivity", Price = 15.00m, Quantity = 40 },
            new Accessory { Id = 15, Name = "Studio Monitor Speakers", Brand = "Yamaha", Category = "Audio", Price = 300.00m, Quantity = 2 }
        };

        // READ: Muestra el listado general de accesorios
        public IActionResult Index()
        {
            return View(_accessoriesList);
        }

        // READ: Muestra el detalle de un accesorio específico validando que el ID exista
        public IActionResult Details(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);

            if (accessory is null)
                return NotFound();

            return View(accessory);
        }

        // Vista especializada: organiza los accesorios alfabéticamente por su categoría
        public IActionResult ByCategory()
        {
            var accessoriesByCategory = _accessoriesList.OrderBy(a => a.Category).ToList();
            return View(accessoriesByCategory);
        }

        // CREATE (GET): Muestra el formulario para registrar un nuevo accesorio
        public IActionResult Create()
        {
            return View(new Accessory { Quantity = 10, Price = 25.00m });
        }

        // CREATE (POST): Valida el modelo y agrega el nuevo accesorio calculando un ID libre de colisiones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Accessory accessory)
        {
            if (!ModelState.IsValid)
            {
                return View(accessory);
            }

            accessory.Id = _accessoriesList.Any() ? _accessoriesList.Max(x => x.Id) + 1 : 1;
            _accessoriesList.Add(accessory);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Busca el accesorio por ID para cargarlo en la vista de edición
        public IActionResult Edit(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }
            return View(accessory);
        }

        // UPDATE (POST): Guarda los cambios realizados al accesorio si el formulario es válido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Accessory updatedAccessory)
        {
            if (id != updatedAccessory.Id)
            {
                return NotFound();
            }

            Accessory? existing = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedAccessory);
            }

            existing.Name = updatedAccessory.Name;
            existing.Brand = updatedAccessory.Brand;
            existing.Category = updatedAccessory.Category;
            existing.Price = updatedAccessory.Price;
            existing.Quantity = updatedAccessory.Quantity;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Muestra la confirmación previa a eliminar el accesorio
        public IActionResult Delete(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }
            return View(accessory);
        }

        // DELETE (POST): Elimina el accesorio definitivamente de la lista en memoria
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }

            _accessoriesList.Remove(accessory);
            return RedirectToAction(nameof(Index));
        }
    }
}
```

---

### 4.4. Paso 3: Construcción de Vistas Razor y Navegación Completa
Se crearon las vistas faltantes del módulo `Spaces` (`Details.cshtml` y `Available.cshtml`) y se implementaron las vistas `Create.cshtml`, `Edit.cshtml`, `Delete.cshtml` e `Index.cshtml` para los cuatro módulos utilizando `content-card`, `dope-card`, badges de Bootstrap e íconos `bi-*`.

#### 4.4.1. Vistas Faltantes del Módulo `Spaces` (`Details.cshtml` y `Available.cshtml`)

**Archivo: `Views/Spaces/Details.cshtml`**
```html
@model Actividad1.Models.Space

@{
    ViewData["Title"] = "Detalle del Espacio";
    string badgeColor = Model.IsAvailable ? "bg-success" : "bg-danger";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">D E T A I L S</h1>

    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="content-card dope-card p-5" style="border-color: var(--color-signal-violet);">
                <div class="d-flex justify-content-between align-items-center">
                    <h2><i class="bi bi-building me-2" style="color: var(--color-signal-violet);"></i>@Model.Name</h2>
                    <span class="badge bg-dark border border-secondary">ID #@Model.Id</span>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="row mb-4">
                    <div class="col-sm-6">
                        <p><strong>Ubicación:</strong> @Model.Location</p>
                        <p><strong>Capacidad Máxima:</strong> @Model.Capacity personas</p>
                        <p><strong>Clasificación:</strong> @Model.GetCapacityType()</p>
                    </div>
                    <div class="col-sm-6">
                        <p><strong>Tarifa por Hora:</strong> $@Model.HourlyRate.ToString("F2")</p>
                        <p><strong>Estado:</strong> <span class="badge @badgeColor">@Model.GetAvailabilityStatus()</span></p>
                        <p><strong>Habilitado:</strong> @(Model.IsAvailable ? "Disponible para reserva" : "Ocupado / En mantenimiento")</p>
                    </div>
                </div>

                @* Cálculo de costos por bloques horarios usando el método del modelo *@
                <div class="p-3 mb-4 rounded" style="background-color: rgba(247,249,250,0.05);">
                    <h5 style="color: var(--color-signal-violet);"><i class="bi bi-calculator me-2"></i>Costo Estimado de Reserva</h5>
                    <ul class="list-unstyled mb-0">
                        <li>2 horas: <strong>$@Model.CalculateEstimatedCost(2).ToString("F2")</strong></li>
                        <li>4 horas (Media jornada): <strong>$@Model.CalculateEstimatedCost(4).ToString("F2")</strong></li>
                        <li>8 horas (Jornada completa): <strong>$@Model.CalculateEstimatedCost(8).ToString("F2")</strong></li>
                    </ul>
                </div>

                <div class="d-flex flex-wrap gap-2 mt-4">
                    <a asp-action="Index" class="btn btn-dope-outline">
                        <i class="bi bi-arrow-left me-1"></i> Volver a Espacios
                    </a>
                    <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-dope-violet">
                        <i class="bi bi-pencil-square me-1"></i> Editar
                    </a>
                    <a asp-action="Delete" asp-route-id="@Model.Id" class="btn btn-outline-danger">
                        <i class="bi bi-trash3 me-1"></i> Eliminar
                    </a>
                    <a asp-action="Available" class="btn btn-dope-outline ms-auto">
                        <i class="bi bi-door-open me-1"></i> Ver Disponibles
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>
```

**Archivo: `Views/Spaces/Available.cshtml`**
```html
@model IEnumerable<Actividad1.Models.Space>

@{
    ViewData["Title"] = "Espacios Disponibles";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center" style="color: var(--color-signal-violet);">A V A I L A B L E</h1>
    <p class="text-center mb-5" style="color: var(--color-steel);">
        <i class="bi bi-check-circle-fill text-success me-1"></i> Mostrando únicamente las salas y laboratorios habilitados para reserva inmediata.
    </p>

    <div class="row g-4">
        @foreach (var space in Model)
        {
            <div class="col-md-4">
                <div class="content-card dope-card text-center h-100 d-flex flex-column">
                    <h5>@space.Name</h5>
                    <p style="color: var(--color-steel);"><i class="bi bi-geo-alt me-1"></i>@space.Location</p>
                    <h3 class="mb-2">$@space.HourlyRate.ToString("F2")<span style="font-size: 14px; color: var(--color-steel);">/hora</span></h3>
                    <p class="small mb-2" style="color: var(--color-steel);">Aforo: @space.Capacity personas (@space.GetCapacityType())</p>
                    <div class="mb-3">
                        <span class="badge bg-success">@space.GetAvailabilityStatus()</span>
                    </div>

                    <div class="mt-auto">
                        <a asp-action="Details" asp-route-id="@space.Id" class="btn btn-dope-outline w-100">
                            <i class="bi bi-calendar2-check me-1"></i> Ver Detalles de Sala
                        </a>
                    </div>
                </div>
            </div>
        }
    </div>

    <div class="text-center mt-5">
        <a asp-action="Index" class="btn btn-dope-outline">
            <i class="bi bi-arrow-left me-1"></i> Ver Todos los Espacios
        </a>
    </div>
</div>
```

#### 4.4.2. Código Completo de Vistas CRUD del Módulo `Equipment`

**Archivo: `Views/Equipment/Index.cshtml`**
```html
@model IEnumerable<Actividad1.Models.Equipment>

@{
    ViewData["Title"] = "Catálogo de Equipos";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">E Q U I P M E N T</h1>

    @* Barra de herramientas superior para crear un nuevo equipo o filtrar disponibles *@
    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <p class="mb-0" style="color: var(--color-steel);">
            <i class="bi bi-pc-display me-1"></i> Total registrados: <strong>@Model.Count()</strong> equipos
        </p>
        <div class="d-flex gap-2">
            <a asp-action="Available" class="btn btn-dope-outline">
                <i class="bi bi-check2-circle me-1"></i> Ver Disponibles
            </a>
            <a asp-action="Create" class="btn btn-dope-violet">
                <i class="bi bi-plus-lg me-1"></i> Nuevo Equipo
            </a>
        </div>
    </div>

    <div class="row g-4">
        @foreach (var item in Model)
        {
            string badgeColor = item.GetStatus() switch
            {
                "Disponible" or "AVAILABLE" => "bg-success",
                "Ultima unidad" or "LAST UNIT" => "bg-warning text-dark",
                "No valido" or "NOT AVAILABLE" => "bg-danger",
                "Fuera de servicio" or "OUT OF SERVICE" => "bg-dark border border-secondary",
                _ => "bg-secondary"
            };

            <div class="col-md-6 col-lg-4">
                <div class="content-card dope-card h-100 d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-start mb-1">
                        <h4 class="mb-0">@item.Name</h4>
                        <span class="badge bg-dark border border-secondary">#@item.Id</span>
                    </div>
                    <p class="text-muted mb-3" style="color: var(--color-steel) !important;">
                        <i class="bi bi-tag me-1"></i>@item.Brand | @item.Category
                    </p>

                    <ul class="list-unstyled mb-4">
                        <li><i class="bi bi-cash-coin me-2" style="color: var(--color-signal-violet);"></i><strong>Tarifa Diaria:</strong> $@item.DailyRate.ToString("F2")</li>
                        <li><i class="bi bi-boxes me-2" style="color: var(--color-signal-violet);"></i><strong>Stock:</strong> @item.Stock unidades</li>
                        <li class="mt-2">
                            <strong>Estado:</strong>
                            <span class="badge rounded-pill @badgeColor">@item.GetStatus()</span>
                        </li>
                    </ul>

                    @* Botones de acción CRUD (Detalles, Editar, Eliminar) *@
                    <div class="mt-auto d-flex gap-2">
                        <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-dope-outline flex-grow-1 text-center">
                            <i class="bi bi-eye me-1"></i> Detalles
                        </a>
                        <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-sm btn-dope-outline" title="Editar">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-sm btn-outline-danger" title="Eliminar">
                            <i class="bi bi-trash3"></i>
                        </a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

**Archivo: `Views/Equipment/Details.cshtml`**
```html
@model Actividad1.Models.Equipment

@{
    ViewData["Title"] = "Detalle del Equipo";

    string badgeColor = Model.GetStatus() switch
    {
        "Disponible" or "AVAILABLE" => "bg-success",
        "Ultima unidad" or "LAST UNIT" => "bg-warning text-dark",
        "No valido" or "NOT AVAILABLE" => "bg-danger",
        "Fuera de servicio" or "OUT OF SERVICE" => "bg-dark border",
        _ => "bg-secondary"
    };
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">D E T A I L S</h1>

    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="content-card dope-card p-5" style="border-color: var(--color-signal-violet);">
                <div class="d-flex justify-content-between align-items-center">
                    <h2><i class="bi bi-laptop me-2" style="color: var(--color-signal-violet);"></i>@Model.Name</h2>
                    <span class="badge bg-dark border border-secondary">ID #@Model.Id</span>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="row mb-4">
                    <div class="col-sm-6">
                        <p><strong>Categoría:</strong> @Model.Category</p>
                        <p><strong>Marca:</strong> @Model.Brand</p>
                        <p><strong>Stock Actual:</strong> @Model.Stock unidades</p>
                        <p><strong>Estado:</strong> <span class="badge @badgeColor">@Model.GetStatus()</span></p>
                    </div>
                    <div class="col-sm-6">
                        <p><strong>Tarifa Diaria:</strong> $@Model.DailyRate.ToString("F2")</p>
                        <p><strong>Operativo:</strong> @(Model.IsOperational ? "Sí (En funcionamiento)" : "No (En mantenimiento)")</p>
                    </div>
                </div>

                <div class="p-3 mb-4 rounded" style="background-color: rgba(247,249,250,0.05);">
                    <h5 style="color: var(--color-signal-violet);"><i class="bi bi-calculator me-2"></i>Costo Estimado de Alquiler</h5>
                    <ul class="list-unstyled mb-0">
                        <li>1 día: <strong>$@Model.CalculateEstimatedCost(1).ToString("F2")</strong></li>
                        <li>3 días: <strong>$@Model.CalculateEstimatedCost(3).ToString("F2")</strong></li>
                        <li>5 días: <strong>$@Model.CalculateEstimatedCost(5).ToString("F2")</strong></li>
                    </ul>
                </div>

                <div class="d-flex flex-wrap gap-2 mt-4">
                    <a asp-action="Index" class="btn btn-dope-outline">
                        <i class="bi bi-arrow-left me-1"></i> Volver al Catálogo
                    </a>
                    <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-dope-violet">
                        <i class="bi bi-pencil-square me-1"></i> Editar
                    </a>
                    <a asp-action="Delete" asp-route-id="@Model.Id" class="btn btn-outline-danger">
                        <i class="bi bi-trash3 me-1"></i> Eliminar
                    </a>
                    <a asp-action="Available" class="btn btn-dope-outline ms-auto">
                        <i class="bi bi-check2-circle me-1"></i> Ver Disponibles
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>
```

**Archivo: `Views/Equipment/Create.cshtml`**
```html
@model Actividad1.Models.Equipment

@{
    ViewData["Title"] = "Registrar Nuevo Equipo";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">C R E A T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-plus-circle me-2" style="color: var(--color-signal-violet);"></i>Registrar Equipo Tecnológico</h3>
                <p style="color: var(--color-steel); font-size: 14px;">Completa los datos del equipo. Todos los campos marcados son validados antes de guardar.</p>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Create" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" placeholder="Ej. MacBook Pro M3" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Brand" class="form-label"></label>
                            <input asp-for="Brand" class="form-control" placeholder="Ej. Apple, Dell, Sony" />
                            <span asp-validation-for="Brand" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" placeholder="Ej. Computación, Audio, Redes" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="DailyRate" class="form-label"></label>
                            <input asp-for="DailyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="DailyRate" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Stock" class="form-label"></label>
                            <input asp-for="Stock" type="number" class="form-control" />
                            <span asp-validation-for="Stock" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsOperational" class="form-check-input" type="checkbox" />
                        <label asp-for="IsOperational" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-save me-1"></i> Guardar Equipo
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-x-circle me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Equipment/Edit.cshtml`**
```html
@model Actividad1.Models.Equipment

@{
    ViewData["Title"] = "Editar Equipo";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">E D I T</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-pencil-square me-2" style="color: var(--color-signal-violet);"></i>Editar Equipo #@Model.Id</h3>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Edit" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
                    <input type="hidden" asp-for="Id" />

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Brand" class="form-label"></label>
                            <input asp-for="Brand" class="form-control" />
                            <span asp-validation-for="Brand" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="DailyRate" class="form-label"></label>
                            <input asp-for="DailyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="DailyRate" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Stock" class="form-label"></label>
                            <input asp-for="Stock" type="number" class="form-control" />
                            <span asp-validation-for="Stock" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsOperational" class="form-check-input" type="checkbox" />
                        <label asp-for="IsOperational" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-check2-circle me-1"></i> Guardar Cambios
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Equipment/Delete.cshtml`**
```html
@model Actividad1.Models.Equipment

@{
    ViewData["Title"] = "Eliminar Equipo";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center text-danger">D E L E T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5 border-danger">
                <div class="d-flex align-items-center mb-3 text-danger">
                    <i class="bi bi-exclamation-triangle-fill fs-2 me-3"></i>
                    <div>
                        <h3 class="mb-0">¿Confirmas la eliminación de este equipo?</h3>
                        <small style="color: var(--color-steel);">Esta acción removerá el equipo del inventario en memoria.</small>
                    </div>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="p-3 mb-4 rounded" style="background-color: rgba(220, 53, 69, 0.08);">
                    <h4 class="mb-2">@Model.Name</h4>
                    <p class="mb-1"><strong>ID:</strong> #@Model.Id</p>
                    <p class="mb-1"><strong>Marca / Categoría:</strong> @Model.Brand | @Model.Category</p>
                    <p class="mb-1"><strong>Tarifa Diaria:</strong> $@Model.DailyRate.ToString("F2")</p>
                    <p class="mb-0"><strong>Stock:</strong> @Model.Stock unidades (@Model.GetStatus())</p>
                </div>

                <form asp-action="Delete" method="post">
                    <input type="hidden" asp-for="Id" />
                    <div class="d-flex gap-3">
                        <button type="submit" class="btn btn-danger">
                            <i class="bi bi-trash3-fill me-1"></i> Sí, Eliminar Definitivamente
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

#### 4.4.3. Resto de Vistas CRUD del Módulo `Spaces` (`Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Delete.cshtml`)

**Archivo: `Views/Spaces/Index.cshtml`**
```html
@model IEnumerable<Actividad1.Models.Space>

@{
    ViewData["Title"] = "Espacios Tecnológicos";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">S P A C E S</h1>

    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <p class="mb-0" style="color: var(--color-steel);">
            <i class="bi bi-building me-1"></i> Total registrados: <strong>@Model.Count()</strong> espacios
        </p>
        <div class="d-flex gap-2">
            <a asp-action="Available" class="btn btn-dope-outline">
                <i class="bi bi-door-open me-1"></i> Espacios Disponibles
            </a>
            <a asp-action="Create" class="btn btn-dope-violet">
                <i class="bi bi-plus-lg me-1"></i> Nuevo Espacio
            </a>
        </div>
    </div>

    <div class="row g-4">
        @foreach (var space in Model)
        {
            string badgeColor = space.IsAvailable ? "bg-success" : "bg-danger";

            <div class="col-md-6">
                <div class="content-card dope-card h-100 d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-start mb-1">
                        <h3 class="mb-0">@space.Name</h3>
                        <span class="badge bg-dark border border-secondary">#@space.Id</span>
                    </div>
                    <p class="mb-3" style="color: var(--color-steel);">
                        <i class="bi bi-geo-alt me-1"></i>@space.Location
                    </p>

                    <ul class="list-unstyled mb-4">
                        <li><i class="bi bi-people me-2" style="color: var(--color-signal-violet);"></i><strong>Capacidad:</strong> @space.Capacity personas (@space.GetCapacityType())</li>
                        <li><i class="bi bi-clock-history me-2" style="color: var(--color-signal-violet);"></i><strong>Tarifa por Hora:</strong> $@space.HourlyRate.ToString("F2") / hora</li>
                        <li class="mt-2">
                            <strong>Estado:</strong>
                            <span class="badge rounded-pill @badgeColor">@space.GetAvailabilityStatus()</span>
                        </li>
                    </ul>

                    <div class="mt-auto d-flex gap-2">
                        <a asp-controller="Spaces" asp-action="Details" asp-route-id="@space.Id" class="btn btn-sm btn-dope-outline flex-grow-1 text-center">
                            <i class="bi bi-eye me-1"></i> Ver Detalles
                        </a>
                        <a asp-controller="Spaces" asp-action="Edit" asp-route-id="@space.Id" class="btn btn-sm btn-dope-outline" title="Editar">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a asp-controller="Spaces" asp-action="Delete" asp-route-id="@space.Id" class="btn btn-sm btn-outline-danger" title="Eliminar">
                            <i class="bi bi-trash3"></i>
                        </a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

**Archivo: `Views/Spaces/Create.cshtml`**
```html
@model Actividad1.Models.Space

@{
    ViewData["Title"] = "Registrar Nuevo Espacio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">C R E A T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-plus-circle me-2" style="color: var(--color-signal-violet);"></i>Registrar Espacio Tecnológico</h3>
                <p style="color: var(--color-steel); font-size: 14px;">Ingresa los datos del laboratorio o sala de innovación.</p>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Create" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" placeholder="Ej. Laboratorio de Ciberdefensa" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Location" class="form-label"></label>
                        <input asp-for="Location" class="form-control" placeholder="Ej. Edificio Central, Piso 2" />
                        <span asp-validation-for="Location" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Capacity" class="form-label"></label>
                            <input asp-for="Capacity" type="number" class="form-control" />
                            <span asp-validation-for="Capacity" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="HourlyRate" class="form-label"></label>
                            <input asp-for="HourlyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="HourlyRate" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsAvailable" class="form-check-input" type="checkbox" />
                        <label asp-for="IsAvailable" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-save me-1"></i> Guardar Espacio
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-x-circle me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Spaces/Edit.cshtml`**
```html
@model Actividad1.Models.Space

@{
    ViewData["Title"] = "Editar Espacio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">E D I T</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-pencil-square me-2" style="color: var(--color-signal-violet);"></i>Editar Espacio #@Model.Id</h3>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Edit" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
                    <input type="hidden" asp-for="Id" />

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Location" class="form-label"></label>
                        <input asp-for="Location" class="form-control" />
                        <span asp-validation-for="Location" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Capacity" class="form-label"></label>
                            <input asp-for="Capacity" type="number" class="form-control" />
                            <span asp-validation-for="Capacity" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="HourlyRate" class="form-label"></label>
                            <input asp-for="HourlyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="HourlyRate" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsAvailable" class="form-check-input" type="checkbox" />
                        <label asp-for="IsAvailable" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-check2-circle me-1"></i> Guardar Cambios
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Spaces/Delete.cshtml`**
```html
@model Actividad1.Models.Space

@{
    ViewData["Title"] = "Eliminar Espacio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center text-danger">D E L E T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5 border-danger">
                <div class="d-flex align-items-center mb-3 text-danger">
                    <i class="bi bi-exclamation-triangle-fill fs-2 me-3"></i>
                    <div>
                        <h3 class="mb-0">¿Confirmas la eliminación de este espacio?</h3>
                        <small style="color: var(--color-steel);">El espacio dejará de estar listado en el centro de recursos.</small>
                    </div>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="p-3 mb-4 rounded" style="background-color: rgba(220, 53, 69, 0.08);">
                    <h4 class="mb-2">@Model.Name</h4>
                    <p class="mb-1"><strong>ID:</strong> #@Model.Id</p>
                    <p class="mb-1"><strong>Ubicación:</strong> @Model.Location</p>
                    <p class="mb-1"><strong>Capacidad:</strong> @Model.Capacity personas (@Model.GetCapacityType())</p>
                    <p class="mb-0"><strong>Tarifa por Hora:</strong> $@Model.HourlyRate.ToString("F2")</p>
                </div>

                <form asp-action="Delete" method="post">
                    <input type="hidden" asp-for="Id" />
                    <div class="d-flex gap-3">
                        <button type="submit" class="btn btn-danger">
                            <i class="bi bi-trash3-fill me-1"></i> Sí, Eliminar Espacio
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

#### 4.4.4. Vistas CRUD del Módulo `Kits` (`Index.cshtml`, `Details.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Delete.cshtml`)

**Archivo: `Views/Kits/Index.cshtml`**
```html
@model IEnumerable<Actividad1.Models.Kit>

@{
    ViewData["Title"] = "Kits Tecnológicos";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">K I T S</h1>

    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <p class="mb-0" style="color: var(--color-steel);">
            <i class="bi bi-box-seam me-1"></i> Total registrados: <strong>@Model.Count()</strong> kits
        </p>
        <div class="d-flex gap-2">
            <a asp-action="ByLevel" class="btn btn-dope-outline">
                <i class="bi bi-bar-chart-steps me-1"></i> Agrupar por Nivel
            </a>
            <a asp-action="Create" class="btn btn-dope-violet">
                <i class="bi bi-plus-lg me-1"></i> Nuevo Kit
            </a>
        </div>
    </div>

    <div class="row g-4">
        @foreach (var kit in Model)
        {
            string statusColor = kit.GetState() == "AVAILABLE" ? "bg-success" : (kit.GetState() == "NEEDS REPAIR" || kit.GetState() == "OUT OF STOCK" ? "bg-danger" : "bg-warning text-dark");
            string levelColor = (kit.Level == "BEGINNER" || kit.Level == "PRINCIPIANTE") ? "bg-success" : ((kit.Level == "INTERMEDIATE" || kit.Level == "INTERMEDIO") ? "bg-warning text-dark" : "bg-dark border border-light");

            <div class="col-md-6 col-lg-4">
                <div class="content-card dope-card h-100 d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h4 class="mb-0">@kit.Name</h4>
                        <span class="badge @levelColor" style="font-size: 10px; letter-spacing: 1px;">@kit.GetKitComplexity()</span>
                    </div>
                    <p style="color: var(--color-steel);"><i class="bi bi-folder2-open me-1"></i>@kit.Category | ID #@kit.Id</p>

                    <ul class="list-unstyled mb-4">
                        <li><i class="bi bi-cash-coin me-2" style="color: var(--color-signal-violet);"></i><strong>Tarifa Diaria:</strong> $@kit.DailyRate.ToString("F2")</li>
                        <li><i class="bi bi-layers me-2" style="color: var(--color-signal-violet);"></i><strong>Disponibles:</strong> @kit.Quantity kits</li>
                        <li class="mt-2">
                            <strong>Condición:</strong>
                            <span class="badge rounded-pill @statusColor">@kit.GetState()</span>
                        </li>
                    </ul>

                    <div class="mt-auto d-flex gap-2">
                        <a asp-controller="Kits" asp-action="Details" asp-route-id="@kit.Id" class="btn btn-sm btn-dope-outline flex-grow-1 text-center">
                            <i class="bi bi-eye me-1"></i> Detalles
                        </a>
                        <a asp-controller="Kits" asp-action="Edit" asp-route-id="@kit.Id" class="btn btn-sm btn-dope-outline" title="Editar">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a asp-controller="Kits" asp-action="Delete" asp-route-id="@kit.Id" class="btn btn-sm btn-outline-danger" title="Eliminar">
                            <i class="bi bi-trash3"></i>
                        </a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

**Archivo: `Views/Kits/Create.cshtml`**
```html
@model Actividad1.Models.Kit

@{
    ViewData["Title"] = "Registrar Nuevo Kit";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">C R E A T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-plus-circle me-2" style="color: var(--color-signal-violet);"></i>Registrar Kit Tecnológico</h3>
                <p style="color: var(--color-steel); font-size: 14px;">Define los datos del kit y su nivel de complejidad académica.</p>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Create" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" placeholder="Ej. Kit de Sensores Industriales" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" placeholder="Ej. Electrónica, Robótica, IA" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Level" class="form-label"></label>
                            <select asp-for="Level" class="form-select">
                                <option value="">-- Selecciona un nivel --</option>
                                <option value="PRINCIPIANTE">PRINCIPIANTE</option>
                                <option value="INTERMEDIO">INTERMEDIO</option>
                                <option value="AVANZADO">AVANZADO</option>
                            </select>
                            <span asp-validation-for="Level" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="DailyRate" class="form-label"></label>
                            <input asp-for="DailyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="DailyRate" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Quantity" class="form-label"></label>
                            <input asp-for="Quantity" type="number" class="form-control" />
                            <span asp-validation-for="Quantity" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsOperational" class="form-check-input" type="checkbox" />
                        <label asp-for="IsOperational" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-save me-1"></i> Guardar Kit
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-x-circle me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Kits/Edit.cshtml`**
```html
@model Actividad1.Models.Kit

@{
    ViewData["Title"] = "Editar Kit";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">E D I T</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-pencil-square me-2" style="color: var(--color-signal-violet);"></i>Editar Kit #@Model.Id</h3>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Edit" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
                    <input type="hidden" asp-for="Id" />

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Level" class="form-label"></label>
                            <select asp-for="Level" class="form-select">
                                <option value="">-- Selecciona un nivel --</option>
                                <option value="PRINCIPIANTE">PRINCIPIANTE</option>
                                <option value="INTERMEDIO">INTERMEDIO</option>
                                <option value="AVANZADO">AVANZADO</option>
                            </select>
                            <span asp-validation-for="Level" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="DailyRate" class="form-label"></label>
                            <input asp-for="DailyRate" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="DailyRate" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Quantity" class="form-label"></label>
                            <input asp-for="Quantity" type="number" class="form-control" />
                            <span asp-validation-for="Quantity" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="form-check form-switch mb-4 mt-2">
                        <input asp-for="IsOperational" class="form-check-input" type="checkbox" />
                        <label asp-for="IsOperational" class="form-check-label"></label>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-check2-circle me-1"></i> Guardar Cambios
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Kits/Delete.cshtml`**
```html
@model Actividad1.Models.Kit

@{
    ViewData["Title"] = "Eliminar Kit";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center text-danger">D E L E T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5 border-danger">
                <div class="d-flex align-items-center mb-3 text-danger">
                    <i class="bi bi-exclamation-triangle-fill fs-2 me-3"></i>
                    <div>
                        <h3 class="mb-0">¿Confirmas la eliminación de este kit?</h3>
                        <small style="color: var(--color-steel);">El kit será retirado del listado general de préstamos.</small>
                    </div>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="p-3 mb-4 rounded" style="background-color: rgba(220, 53, 69, 0.08);">
                    <h4 class="mb-2">@Model.Name</h4>
                    <p class="mb-1"><strong>ID:</strong> #@Model.Id</p>
                    <p class="mb-1"><strong>Categoría / Nivel:</strong> @Model.Category | @Model.GetKitComplexity()</p>
                    <p class="mb-1"><strong>Tarifa Diaria:</strong> $@Model.DailyRate.ToString("F2")</p>
                    <p class="mb-0"><strong>Cantidad:</strong> @Model.Quantity unidades (@Model.GetState())</p>
                </div>

                <form asp-action="Delete" method="post">
                    <input type="hidden" asp-for="Id" />
                    <div class="d-flex gap-3">
                        <button type="submit" class="btn btn-danger">
                            <i class="bi bi-trash3-fill me-1"></i> Sí, Eliminar Kit
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

#### 4.4.5. Vistas CRUD del Módulo `Accessories` (`Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Delete.cshtml`)

**Archivo: `Views/Accessories/Index.cshtml`**
```html
@model IEnumerable<Actividad1.Models.Accessory>

@{
    ViewData["Title"] = "Catálogo de Accesorios";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">A C C E S S O R I E S</h1>

    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <p class="mb-0" style="color: var(--color-steel);">
            <i class="bi bi-headphones me-1"></i> Total registrados: <strong>@Model.Count()</strong> accesorios
        </p>
        <div class="d-flex gap-2">
            <a asp-action="ByCategory" class="btn btn-dope-outline">
                <i class="bi bi-grid me-1"></i> Agrupar por Categoría
            </a>
            <a asp-action="Create" class="btn btn-dope-violet">
                <i class="bi bi-plus-lg me-1"></i> Nuevo Accesorio
            </a>
        </div>
    </div>

    <div class="row g-4">
        @foreach (var item in Model)
        {
            string inventoryColor = item.GetInventoryStatus() switch
            {
                "IN STOCK" => "bg-success",
                "LOW STOCK" => "bg-warning text-dark",
                "OUT OF STOCK" => "bg-danger",
                _ => "bg-secondary"
            };

            <div class="col-md-6 col-lg-3">
                <div class="content-card dope-card h-100 d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-start mb-1">
                        <h5 class="mb-0">@item.Name</h5>
                        <span class="badge bg-dark border border-secondary">#@item.Id</span>
                    </div>
                    <p style="color: var(--color-steel); font-size: 14px;">@item.Brand | @item.Category</p>

                    <ul class="list-unstyled mb-4" style="font-size: 15px;">
                        <li><i class="bi bi-tag me-2" style="color: var(--color-signal-violet);"></i><strong>Precio:</strong> $@item.Price.ToString("F2")</li>
                        <li><i class="bi bi-box me-2" style="color: var(--color-signal-violet);"></i><strong>Stock:</strong> @item.Quantity uds.</li>
                        <li class="mt-2">
                            <span class="badge rounded-pill @inventoryColor">@item.GetInventoryStatus()</span>
                        </li>
                    </ul>

                    <div class="mt-auto d-flex gap-2">
                        <a asp-controller="Accessories" asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-dope-outline flex-grow-1 text-center">
                            <i class="bi bi-eye me-1"></i> Detalles
                        </a>
                        <a asp-controller="Accessories" asp-action="Edit" asp-route-id="@item.Id" class="btn btn-sm btn-dope-outline" title="Editar">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a asp-controller="Accessories" asp-action="Delete" asp-route-id="@item.Id" class="btn btn-sm btn-outline-danger" title="Eliminar">
                            <i class="bi bi-trash3"></i>
                        </a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

**Archivo: `Views/Accessories/Create.cshtml`**
```html
@model Actividad1.Models.Accessory

@{
    ViewData["Title"] = "Registrar Nuevo Accesorio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">C R E A T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-plus-circle me-2" style="color: var(--color-signal-violet);"></i>Registrar Accesorio</h3>
                <p style="color: var(--color-steel); font-size: 14px;">Ingresa los datos del periférico o accesorio tecnológico.</p>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Create" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" placeholder="Ej. Adaptador USB-C a Ethernet Gigabit" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Brand" class="form-label"></label>
                            <input asp-for="Brand" class="form-control" placeholder="Ej. Anker, Logitech, Belkin" />
                            <span asp-validation-for="Brand" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" placeholder="Ej. Connectivity, Audio, Video" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Price" class="form-label"></label>
                            <input asp-for="Price" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="Price" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Quantity" class="form-label"></label>
                            <input asp-for="Quantity" type="number" class="form-control" />
                            <span asp-validation-for="Quantity" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-save me-1"></i> Guardar Accesorio
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-x-circle me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Accessories/Edit.cshtml`**
```html
@model Actividad1.Models.Accessory

@{
    ViewData["Title"] = "Editar Accesorio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center">E D I T</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5" style="border-color: var(--color-signal-violet);">
                <h3 class="mb-3"><i class="bi bi-pencil-square me-2" style="color: var(--color-signal-violet);"></i>Editar Accesorio #@Model.Id</h3>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <form asp-action="Edit" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
                    <input type="hidden" asp-for="Id" />

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label"></label>
                        <input asp-for="Name" class="form-control" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Brand" class="form-label"></label>
                            <input asp-for="Brand" class="form-control" />
                            <span asp-validation-for="Brand" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Category" class="form-label"></label>
                            <input asp-for="Category" class="form-control" />
                            <span asp-validation-for="Category" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label asp-for="Price" class="form-label"></label>
                            <input asp-for="Price" type="number" step="0.01" class="form-control" />
                            <span asp-validation-for="Price" class="text-danger small"></span>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label asp-for="Quantity" class="form-label"></label>
                            <input asp-for="Quantity" type="number" class="form-control" />
                            <span asp-validation-for="Quantity" class="text-danger small"></span>
                        </div>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <button type="submit" class="btn btn-dope-violet">
                            <i class="bi bi-check2-circle me-1"></i> Guardar Cambios
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Archivo: `Views/Accessories/Delete.cshtml`**
```html
@model Actividad1.Models.Accessory

@{
    ViewData["Title"] = "Eliminar Accesorio";
}

<div class="container mt-5">
    <h1 class="stamped-heading text-center text-danger">D E L E T E</h1>

    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-7">
            <div class="content-card dope-card p-4 p-md-5 border-danger">
                <div class="d-flex align-items-center mb-3 text-danger">
                    <i class="bi bi-exclamation-triangle-fill fs-2 me-3"></i>
                    <div>
                        <h3 class="mb-0">¿Confirmas la eliminación de este accesorio?</h3>
                        <small style="color: var(--color-steel);">El registro se eliminará permanentemente de la lista en memoria.</small>
                    </div>
                </div>
                <hr style="border-color: rgba(247,249,250,0.2);" />

                <div class="p-3 mb-4 rounded" style="background-color: rgba(220, 53, 69, 0.08);">
                    <h4 class="mb-2">@Model.Name</h4>
                    <p class="mb-1"><strong>ID:</strong> #@Model.Id</p>
                    <p class="mb-1"><strong>Marca / Categoría:</strong> @Model.Brand | @Model.Category</p>
                    <p class="mb-1"><strong>Precio Unitario:</strong> $@Model.Price.ToString("F2")</p>
                    <p class="mb-0"><strong>Stock:</strong> @Model.Quantity unidades (@Model.GetInventoryStatus())</p>
                </div>

                <form asp-action="Delete" method="post">
                    <input type="hidden" asp-for="Id" />
                    <div class="d-flex gap-3">
                        <button type="submit" class="btn btn-danger">
                            <i class="bi bi-trash3-fill me-1"></i> Sí, Eliminar Accesorio
                        </button>
                        <a asp-action="Index" class="btn btn-dope-outline">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

---

### 4.5. Evidencias Gráficas del Procedimiento (Capturas de Interfaz en Ejecución)

> **[ ESPACIO PARA CAPTURA 1 ]**  
> *Insertar aquí captura del navegador en `/Equipment` mostrando el catálogo con los botones Nuevo Equipo, Ver Disponibles, Detalles, Editar y Eliminar.*  
> **Figura 1.** Catálogo principal del módulo `Equipment` con controles CRUD integrados.

> **[ ESPACIO PARA CAPTURA 2 ]**  
> *Insertar aquí captura del navegador en `/Equipment/Create` intentando enviar campos vacíos o fuera de rango para evidenciar los mensajes de error.*  
> **Figura 2.** Validación de `DataAnnotations` en acción sobre el formulario de creación.

> **[ ESPACIO PARA CAPTURA 3 ]**  
> *Insertar aquí captura del navegador en `/Spaces/Details/1` y `/Spaces/Available`.*  
> **Figura 3.** Vistas `Details.cshtml` y `Available.cshtml` implementadas para el módulo `Spaces`.

> **[ ESPACIO PARA CAPTURA 4 ]**  
> *Insertar aquí captura del navegador en `/Kits/Delete/7` mostrando la pantalla de confirmación visual antes del borrado.*  
> **Figura 4.** Pantalla de confirmación de eliminación (`Delete.cshtml`) con protección Anti-Forgery Token.

> **[ ESPACIO PARA CAPTURA 5 ]**  
> *Insertar aquí captura del navegador en `/Kits/ByLevel` y `/Accessories/ByCategory` tras haber agregado y editado registros.*  
> **Figura 5.** Persistencia de datos verificada en las vistas agrupadas `ByLevel` y `ByCategory`.

---

## 5. Análisis de Resultados

### 5.1. Tablas Resumen de Validaciones Implementadas

#### Tabla 1. Reglas de validación en el modelo `Equipment`
| Propiedad | Tipo | Atributos DataAnnotations | Regla / Restricción |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | `[Required]`, `[StringLength(80, MinimumLength = 3)]`, `[Display]` | Obligatorio, entre 3 y 80 caracteres. |
| `Category` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 2)]`, `[Display]` | Obligatorio, entre 2 y 50 caracteres. |
| `Brand` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 2)]`, `[Display]` | Obligatorio, entre 2 y 50 caracteres. |
| `DailyRate` | `decimal` | `[Required]`, `[Range(1.00, 1000.00)]`, `[Display]` | Obligatorio, valor entre \$1.00 y \$1000.00. |
| `Stock` | `int` | `[Required]`, `[Range(0, 500)]`, `[Display]` | Obligatorio, entero entre 0 y 500 unidades. |

#### Tabla 2. Reglas de validación en el modelo `Space`
| Propiedad | Tipo | Atributos DataAnnotations | Regla / Restricción |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | `[Required]`, `[StringLength(80, MinimumLength = 3)]`, `[Display]` | Obligatorio, entre 3 y 80 caracteres. |
| `Location` | `string` | `[Required]`, `[StringLength(100, MinimumLength = 4)]`, `[Display]` | Obligatorio, entre 4 y 100 caracteres. |
| `Capacity` | `int` | `[Required]`, `[Range(1, 200)]`, `[Display]` | Obligatorio, aforo entre 1 y 200 personas. |
| `HourlyRate` | `decimal` | `[Required]`, `[Range(5.00, 500.00)]`, `[Display]` | Obligatorio, tarifa entre \$5.00 y \$500.00. |

#### Tabla 3. Reglas de validación en el modelo `Kit`
| Propiedad | Tipo | Atributos DataAnnotations | Regla / Restricción |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | `[Required]`, `[StringLength(80, MinimumLength = 3)]`, `[Display]` | Obligatorio, entre 3 y 80 caracteres. |
| `Category` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 2)]`, `[Display]` | Obligatorio, entre 2 y 50 caracteres. |
| `Level` | `string` | `[Required]`, `[StringLength(30, MinimumLength = 4)]`, `[Display]` | Obligatorio (`PRINCIPIANTE`, `INTERMEDIO`, `AVANZADO`). |
| `DailyRate` | `decimal` | `[Required]`, `[Range(1.00, 500.00)]`, `[Display]` | Obligatorio, tarifa entre \$1.00 y \$500.00. |
| `Quantity` | `int` | `[Required]`, `[Range(0, 200)]`, `[Display]` | Obligatorio, cantidad entre 0 y 200 kits. |

#### Tabla 4. Reglas de validación en el modelo `Accessory`
| Propiedad | Tipo | Atributos DataAnnotations | Regla / Restricción |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | `[Required]`, `[StringLength(80, MinimumLength = 3)]`, `[Display]` | Obligatorio, entre 3 y 80 caracteres. |
| `Brand` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 2)]`, `[Display]` | Obligatorio, entre 2 y 50 caracteres. |
| `Category` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 2)]`, `[Display]` | Obligatorio, entre 2 y 50 caracteres. |
| `Price` | `decimal` | `[Required]`, `[Range(1.00, 2000.00)]`, `[Display]` | Obligatorio, precio entre \$1.00 y \$2000.00. |
| `Quantity` | `int` | `[Required]`, `[Range(0, 1000)]`, `[Display]` | Obligatorio, stock entre 0 y 1000 unidades. |

---

### 5.2. Matriz Completa de Casos de Prueba (7 Pruebas por Módulo)

#### 5.2.1. Módulo 1: `Equipment`
| N° | Caso de Prueba | Datos de Entrada / Acción | Resultado Esperado | Resultado Obtenido | Estado |
| :-: | :--- | :--- | :--- | :--- | :-: |
| **EQ-01** | Creación válida | `Name="Osciloscopio Digital"`, `Brand="Tektronix"`, `Category="Electrónica"`, `DailyRate=35.00`, `Stock=3`, `IsOperational=true` | Se asigna `Id = 21`, se agrega a `_equipmentList` y redirige a `/Equipment`. | Registro creado con `Id = 21` y listado en catálogo con estado `Disponible`. | **Aprobado** |
| **EQ-02** | Creación inválida | `Name=""` (vacío), `DailyRate=0`, `Stock=-5` | `ModelState.IsValid` es `false`; permanece en la vista mostrando mensajes de error en rojo. | El formulario bloquea el envío y muestra los mensajes de `[Required]` y `[Range]`. | **Aprobado** |
| **EQ-03** | Edición válida | Editar `Id = 1` (`Latitude 5550`): cambiar `DailyRate=28.00` y `Stock=6` | Se actualizan los datos en la lista estática y redirige a `/Equipment`. | El equipo `#1` refleja la tarifa de `\$28.00` y stock de `6 unidades`. | **Aprobado** |
| **EQ-04** | Edición inválida | Editar `Id = 2`: cambiar `Name="A"` (1 carácter) y `DailyRate=5000` | Rechaza los cambios por violar `[StringLength]` y `[Range]`; mantiene los valores en pantalla para corrección. | No se sobrescribe el objeto y se visualizan las alertas de validación. | **Aprobado** |
| **EQ-05** | Eliminación confirmada | Acceder a `/Equipment/Delete/3` y confirmar envío POST | El equipo `EcoTank 4800` (`Id = 3`) se elimina de `_equipmentList` y redirige al índice. | El total de equipos disminuye en 1 y el `Id = 3` ya no aparece en el listado. | **Aprobado** |
| **EQ-06** | Consulta de ID inexistente | Navegar a `/Equipment/Details/999` o `/Equipment/Edit/999` | El controlador intercepta `equipment is null` y retorna `NotFound()` (HTTP 404). | El navegador recibe código de estado `404 Not Found` sin lanzar excepción nula. | **Aprobado** |
| **EQ-07** | Vista especializada (`Available`) | Navegar a `/Equipment/Available` | Muestra solo equipos donde `IsOperational == true && Stock > 0`. | Se excluyen equipos sin stock (`Stock == 0`) o en mantenimiento (`IsOperational == false`). | **Aprobado** |

#### 5.2.2. Módulo 2: `Spaces`
| N° | Caso de Prueba | Datos de Entrada / Acción | Resultado Esperado | Resultado Obtenido | Estado |
| :-: | :--- | :--- | :--- | :--- | :-: |
| **SP-01** | Creación válida | `Name="Laboratorio de Ciberdefensa"`, `Location="Edificio C, Piso 1"`, `Capacity=25`, `HourlyRate=30.00`, `IsAvailable=true` | Se asigna `Id = 6`, se guarda en `_spacesList` y redirige a `/Spaces`. | El nuevo laboratorio aparece en el catálogo como `Laboratorio Mediano` y estado `Valido`. | **Aprobado** |
| **SP-02** | Creación inválida | `Name="La"`, `Location=""`, `Capacity=0`, `HourlyRate=2.00` | `ModelState.IsValid` retorna `false`; se muestran errores de longitud, obligatoriedad y rango. | Se impide el registro y se despliegan los mensajes de validación bajo cada campo. | **Aprobado** |
| **SP-03** | Edición válida | Editar `Id = 2` (`Sala Multimedia`): cambiar `IsAvailable=true` y `Capacity=20` | Se actualiza el estado del espacio en memoria y redirige al catálogo. | La `Sala Multimedia` cambia su badge a verde (`Valido`) con capacidad de `20 personas`. | **Aprobado** |
| **SP-04** | Edición inválida | Editar `Id = 1`: ingresar `Capacity=350` (supera máximo de 200) | Se rechaza la modificación y se muestra el mensaje `"La capacidad debe estar entre 1 y 200 personas."` | El formulario conserva la vista de edición indicando el error de rango. | **Aprobado** |
| **SP-05** | Eliminación confirmada | Acceder a `/Spaces/Delete/5` (`Laboratorio de Robótica`) y confirmar POST | Se remueve el registro `Id = 5` de `_spacesList` y redirige a `/Spaces`. | El espacio `#5` desaparece del catálogo sin afectar los demás registros. | **Aprobado** |
| **SP-06** | Consulta de ID inexistente | Navegar a `/Spaces/Details/500` | Retorna respuesta HTTP `404 Not Found`. | Respuesta HTTP 404 controlada correctamente por `SpacesController`. | **Aprobado** |
| **SP-07** | Vista especializada (`Available`) | Navegar a `/Spaces/Available` | Renderiza la nueva vista `Available.cshtml` filtrando solo salas con `IsAvailable == true`. | Se listan únicamente las salas disponibles con su aforo y tarifa por hora. | **Aprobado** |

#### 5.2.3. Módulo 3: `Kits`
| N° | Caso de Prueba | Datos de Entrada / Acción | Resultado Esperado | Resultado Obtenido | Estado |
| :-: | :--- | :--- | :--- | :--- | :-: |
| **KT-01** | Creación válida | `Name="Kit Drone Programable"`, `Category="Robótica"`, `Level="AVANZADO"`, `DailyRate=28.00`, `Quantity=4`, `IsOperational=true` | Se genera `Id = 11`, se inserta en `_kitsList` y redirige a `/Kits`. | El kit `#11` se visualiza con badge `AVANZADO` y estado `AVAILABLE`. | **Aprobado** |
| **KT-02** | Creación inválida | `Name=""`, `Level=""` (sin seleccionar), `DailyRate=-10` | No supera la validación; muestra mensajes de error en `Name`, `Level` y `DailyRate`. | El servidor/cliente detiene el registro y resalta los campos inválidos. | **Aprobado** |
| **KT-03** | Edición válida | Editar `Id = 4` (`Kit de Fotografía`): actualizar `Quantity=5` | El estado del kit pasa de `OUT OF STOCK` a `AVAILABLE` tras actualizar la cantidad. | En `/Kits` el kit `#4` muestra `5 kits` y badge verde `AVAILABLE`. | **Aprobado** |
| **KT-04** | Edición inválida | Editar `Id = 5`: cambiar `Quantity=500` (máximo permitido: 200) | Se bloquea la actualización mostrando el error de `[Range(0, 200)]`. | El cambio no se aplica y se informa al usuario el rango permitido. | **Aprobado** |
| **KT-05** | Eliminación confirmada | Acceder a `/Kits/Delete/7` (`Kit de Streaming`) y confirmar eliminación | Se elimina el kit `Id = 7` de `_kitsList` y redirige a `/Kits`. | El kit `#7` se elimina satisfactoriamente de la colección estática. | **Aprobado** |
| **KT-06** | Consulta de ID inexistente | Solicitar `/Kits/Delete/888` | El método `Delete(int id)` devuelve `NotFound()`. | Se obtiene código HTTP 404 sin errores de ejecución. | **Aprobado** |
| **KT-07** | Vista especializada (`ByLevel`) | Navegar a `/Kits/ByLevel` | Ordena y agrupa visualmente los kits en secciones según su propiedad `Level`. | Los kits aparecen organizados bajo los encabezados `// AVANZADO`, `// INTERMEDIO` y `// PRINCIPIANTE`. | **Aprobado** |

#### 5.2.4. Módulo 4: `Accessories`
| N° | Caso de Prueba | Datos de Entrada / Acción | Resultado Esperado | Resultado Obtenido | Estado |
| :-: | :--- | :--- | :--- | :--- | :-: |
| **AC-01** | Creación válida | `Name="Stream Deck MK.2"`, `Brand="Elgato"`, `Category="Video"`, `Price=149.99`, `Quantity=7` | Se calcula `Id = 16`, se añade a `_accessoriesList` y redirige a `/Accessories`. | El accesorio `#16` se muestra en el catálogo con estado `IN STOCK`. | **Aprobado** |
| **AC-02** | Creación inválida | `Name="AB"`, `Brand=""`, `Price=0`, `Quantity=-1` | Se rechaza el formulario por incumplir `[StringLength]`, `[Required]` y `[Range]`. | Se presentan los 4 mensajes de error de validación en el formulario `Create`. | **Aprobado** |
| **AC-03** | Edición válida | Editar `Id = 15` (`Studio Monitor Speakers`): cambiar `Quantity=0` | Se actualiza el stock a `0` y el estado cambia automáticamente a `OUT OF STOCK`. | El accesorio `#15` muestra `0 uds.` y badge rojo `OUT OF STOCK`. | **Aprobado** |
| **AC-04** | Edición inválida | Editar `Id = 1`: asignar `Price=5000.00` (supera el límite de \$2000.00) | `ModelState.IsValid` impide guardar el cambio e informa el rango válido. | Se mantiene en la vista `Edit.cshtml` mostrando la alerta correspondiente. | **Aprobado** |
| **AC-05** | Eliminación confirmada | Acceder a `/Accessories/Delete/14` y confirmar en el formulario POST | Se elimina el accesorio `Id = 14` de `_accessoriesList` y redirige al índice. | El accesorio desaparece del listado general y de la agrupación por categoría. | **Aprobado** |
| **AC-06** | Consulta de ID inexistente | Navegar a `/Accessories/Details/404` | Retorna `NotFound()` al no existir el ID en `_accessoriesList`. | Se recibe respuesta HTTP 404 de forma segura. | **Aprobado** |
| **AC-07** | Vista especializada (`ByCategory`) | Navegar a `/Accessories/ByCategory` | Agrupa los accesorios alfabéticamente por `Category` (`Audio`, `Computing`, `Connectivity`, etc.). | Se renderizan las secciones por categoría con sus tarjetas y enlaces a `Details`. | **Aprobado** |

---

## 6. Discusión

Al contrastar los fundamentos teóricos de ASP.NET Core MVC con el comportamiento observado durante las pruebas prácticas, se evidenció de manera directa el impacto del ciclo de vida *Transient* de los controladores. Cuando una lista se define sin el modificador `static`, cada petición `POST` seguida del patrón *Post-Redirect-Get (PRG)* reinicia la colección a sus valores semilla, dando la falsa impresión de que las operaciones CRUD no surten efecto. Al aplicar `private static readonly List<T>`, todas las operaciones de creación, edición y borrado persistieron inmediatamente al redirigir hacia `Index`, `Available`, `ByLevel` y `ByCategory`.

Por otra parte, la combinación de validaciones del lado del cliente (`_ValidationScriptsPartial.cshtml` con jQuery Unobtrusive) y del lado del servidor (`ModelState.IsValid`) demostró ser complementaria: mientras que la validación en el navegador ofrece retroalimentación instantánea sin recargar la página, la verificación en el controlador garantiza que ninguna petición manipulada pueda corromper la integridad de las listas en memoria ni provocar fallos en métodos de negocio como `CalculateEstimatedCost()` o `CalculateBulkCost()`.

---

## 7. Conclusiones

1. **Integridad mediante `ModelState.IsValid`:** La verificación explícita de `ModelState.IsValid` dentro de las acciones `POST` (`Create` y `Edit`) constituye una barrera de seguridad indispensable en el servidor, ya que impide almacenar objetos incompletos o con valores numéricos incoherentes incluso si el usuario deshabilita JavaScript en el navegador.
2. **Persistencia en Memoria con `List<T>` Estáticas:** El uso de `private static readonly List<T>` resuelve eficazmente el reinicio de datos entre peticiones HTTP en entornos académicos o de prototipado sin base de datos, manteniendo una única instancia compartida en memoria y evitando colisiones de identificadores gracias al cálculo dinámico `lista.Any() ? lista.Max(x => x.Id) + 1 : 1`.
3. **Desacoplamiento y Reutilización con `DataAnnotations`:** La centralización de las reglas de validación y etiquetas de visualización (`[Required]`, `[StringLength]`, `[Range]`, `[Display]`) directamente en las clases del Modelo evita duplicar condiciones `if/else` en los controladores y sincroniza automáticamente los mensajes de error en todas las vistas Razor (`Create.cshtml` y `Edit.cshtml`).

---

## 8. Bibliografía

- Freeman, A. (2024). *Pro ASP.NET Core 8/10 MVC: Develop Cloud-Ready Web Applications Using Microsoft's Latest Framework* (10th ed.). Apress.
- Lock, A. (2023). *ASP.NET Core in Action* (3rd ed.). Manning Publications.
- Microsoft Corporation. (2026). *Model validation in ASP.NET Core MVC and Razor Pages*. Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
- Microsoft Corporation. (2026). *Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core*. Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
