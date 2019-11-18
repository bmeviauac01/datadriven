using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    // ********************************************************************************
    // # Example phase 3 - dependency injection based on .NET Core Dependency Injection
    // ********************************************************************************

    /* 
    In our example TodoController is the "root object", it's resolved by the framework 
    the a Web API call is received with a call to serviceProvider.GetService<TodoController>().
    This call is hidden from us, it's handled by the framework pipeline. How is the
    object graph created during this resolve step? The GetService call resolves all
    direct and indirect dependencies of TodoController, recursively. It does so by checking
    the constructor parameters of TodoController:
    * TodoContext context: a TodoContext is created based on the TodoContext->TodoContext 
      mapping registration (scoped).
    * INotificationService notificationService: a NotificationService instance is returned based on the 
      INotificationService->NotificationService  mapping registration. This has three constructor
      parameters, these are also resolved based in the container type registrations:
      * ILogger logger: Logger (singleton)
      * IEMailSender emailSender: EMailSender (transient), which has one constructor parameter:
            * ILogger logger: Logger (singleton)
      * IContactRepository contactRepository: ContactRepository (scoped)
    By the end, a complete object graph is created, with TodoController as the root, which can
    server incoming requests, e.g. MessageToLinkedContact in our case.
    This process of wiring objects together is called Autowiring.
     */
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        // The dependencies of TodoController
        private readonly TodoContext _context; // This is a DbContext
        private readonly INotificationService _notificationService;
        
        // Dependencies are received as constructor parameters
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

        // API call handling function for sending an e-mail notification
        // Example for use: a http post request to this url (e.g. via using PostMan):
        //     http://localhost:58922/api/todo/2/reminder
        // , which sends an e-mail notif to the e-mail address appointed of the 
        // contact person referenced by the todo item.
        [HttpPost("{id}/reminder")]
        public IActionResult ReminderMessageToLinkedContact(long id)
        {
            // Todo item kikeresése, használja a _context DbContext objektumot
            var item = _context.TodoItems.Find(id);
            if (item == null)
                return NotFound();

            // Look up todo item
            _notificationService.SendEmailReminder(item.LinkedContactId, item.Name);

            // Actually we don't create anything here, simply return an OK
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