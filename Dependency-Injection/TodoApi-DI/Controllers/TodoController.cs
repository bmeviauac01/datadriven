using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    /* #3.2 FÜGGŐSÉGEK INJEKTÁLÁSA .NET CORE DI ALAPOKON: RESOLVE, ROOT OBJECT, AUTOWIRING
    A példánkban a TodoController a "root object", a keretrendszer ezt oldja fel (RESOLVE)
    a kérések beérkezésekor, egy ServiceProvider.GetService<TodoController>() hívással. 
    Ez a RESOLVE lépés előlünk el van rejtve (a kiszolgáló pipeline kezeli). 
    Mi történik ennek során?
    A GetService hívás feloldja a TodoController közvetett és közvetlen függőségeit, rekurzívan.
    Megnézi, milyen konstruktor paraméterei vannak a TodoController-nek:
    * TodoContext context: ez a TodoContext->TodoContext regisztrációnak megfelelően TodoContext
      objektumra képződik le (scope-olt, minden API kiszolgálási kérésre más)
    * INotificationService notificationService: ez a INotificationService->NotificationService regisztrációnak megfelelően
      NotificationService objektumra képződik le. Ennek van három konstruktor paramétere, ezeket is
      feloldja a DI regisztrációk alapján: 
        * ILogger logger: Logger (singleton)
        * IEMailSender emailSender: EMailSender (transient), aminek van egy konstruktor parammétere:
            * ILogger logger: Logger (singleton)
        * IContactRepository contactRepository: ContactRepository (scoped)
    A feloldás végére előáll a teljesen felparaméterezett TodoController "root object", és ki tudja
    szolgálni a kéréseket (pl. MessageToLinkedContact művelet). Ez a folyamat az Autowiring.
     */
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        // A TodoController osztály függőségei
        private readonly TodoContext _context; // ez egy DbContext
        private readonly INotificationService _notificationService;
        
        // A függőségeket konstruktor paraméterben kapja meg.
        public TodoController(TodoContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;

            // Fill wit some initial data
            if (_context.TodoItems.Count() == 0)
            {
                _context.TodoItems.Add(new TodoItem { Name = "Item1" });
                _context.TodoItems.Add(new TodoItem { Name = "Item2", LinkedContactId = 2});
                _context.SaveChanges();
            }
        }

        // API kezelőfüggvény e-mail emlékeztető értesítés kiküldésére.
        // Példa: http post erre a címre (pl. PostMan-nel): http://localhost:58922/api/todo/2/reminder
        // Ez a 2-es azonosítójú todo item kontakt személyének értesítést küld a todo itemről.
        [HttpPost("{id}/reminder")]
        public IActionResult ReminderMessageToLinkedContact(long id)
        {
            // Todo item kikeresése, használja a _context DbContext objektumot
            var item = _context.TodoItems.Find(id);
            if (item == null)
                return NotFound();

            // Emlékeztető e-mail kiküldése
            _notificationService.SendEmailReminder(item.LinkedContactId, item.Name);

            // Valójában nem hozunk létre semmit, egyszerű OK a válasz
            return Ok();
        }

        #region snippet_GetAll
        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetAll()
        {
            return _context.TodoItems.ToList();
        }
        #endregion

        #region snippet_GetByID
        [HttpGet("{id}", Name = "GetTodo")]
        public ActionResult<TodoItem> GetById(long id)
        {
            var item = _context.TodoItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }
        #endregion

        #region snippet_Create
        [HttpPost]
        public IActionResult Create(TodoItem item)
        {
            _context.TodoItems.Add(item);
            _context.SaveChanges();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }
        #endregion

        #region snippet_Update
        [HttpPut("{id}")]
        public IActionResult Update(long id, TodoItem item)
        {
            var todo = _context.TodoItems.Find(id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _context.TodoItems.Update(todo);
            _context.SaveChanges();
            return NoContent();
        }
        #endregion

        #region snippet_Delete
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var todo = _context.TodoItems.Find(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todo);
            _context.SaveChanges();
            return NoContent();
        }
        #endregion
    }
}