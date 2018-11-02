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
    * INotificationService notificationService: ez a TodoContext->TodoContext regisztrációnak megfelelően
      NotificationService objektumra képződik le. Ennek van három konstruktor paramétere, ezeket is
      feloldja a DI regisztrációk alapján: 
        * ILogger logger: Logger (singleton)
        * IEMailSender emailSender: EMailSender (singleton), aminek van egy konctruktor parammétere:
            * ILogger logger: Logger (singleton)
        * IContactRepository contactRepository: ContactRepository
    A feloldás végére előáll a teljesen felparaméterezett TodoController "root object", és ki tudja
    szolgálni a kéréseket (pl. MessageToLinkedContact művelet). Ez a folyamat az Autowiring.
     */
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly INotificationService _notificationService;
        
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

        // Ez nem igazán REST szemléletű, kompromisszumos megoldás
        // PostMan-nal POST (!) kérés erre a címre: http://localhost:58922/api/todo/2/reminder
        [HttpPost("{id}/reminder")]
        public IActionResult ReminderMessageToLinkedContact(long id)
        {
            // Lookup todo item 
            var item = _context.TodoItems.Find(id);
            if (item == null)
                return NotFound();

            // Send e-mail reminder
            _notificationService.SendEmailReminder(item.LinkedContactId, item.Name);

            // Nothing is actually created, we simply return ok
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