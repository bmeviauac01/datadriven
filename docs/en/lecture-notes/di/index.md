# Dependency Injection ASP.NET Core environment

Zoltán Benedek, 11.19.2022

!!! abstract "Definition"
    __Dependency Injection__ (DI) is programming technique that makes a class independent of its dependencies. It's a key enabler for decomposing an application into loosely coupled components. More precisely: __Dependency Injection is a mechanism to decouple the creation of dependency graphs for a class from its class definition__.

Of course, the above definition is very abstract, and based on the short definition it's hard to understand what problems DI is trying to solve, and how DI is trying to solve them.

In the following chapters, we will use an example to put DI into context and to learn the basics of the DI related services build into ASP.NET Core.

Goals of DI

* Facilitated extensibility and maintainability
* Improved unit testability
* Facilitated code reuse

!!! example "Sample application"
    The sample C# code is available here: <https://github.com/bmeviauac01/todoapi-di-sample>

## Example phase 1 - service class with wired in dependencies

In this example, based on code snippets we look at parts of a to-do list (TODO) application that sends to-do item related email notifications. Note: The code is minimalistic for succinctness.

The "entry point" of our example is the `SendReminderIfNeeded` operation of the `ToDoService` class.

```csharp
// Class for managing todo items
public class ToDoService
{
    const string smtpAddress = "smtp.myserver.com";

    // It checks the todoItem object received as a parameter and sends an e-mail
    // notification about the to-do item to the contact person specified by the
    // todo item.
    public void SendReminderIfNeeded(TodoItem todoItem)
    {
        if (checkIfTodoReminderIsToBeSent(todoItem))
        {
            NotificationService notificationService = new NotificationService(smtpAddress);
            notificationService.SendEmailReminder(todoItem.LinkedContactId, todoItem.Name);
        }
    }

    bool checkIfTodoReminderIsToBeSent(TodoItem todoItem)
    {
        bool send = true;
        /* ... */
        return send;
    }
    // ...
}

// Entity class, encapsulates information about a todo task
public class TodoItem
{
    // Database key
    public long Id { get; set; }
    // Name/description of the task
    public string Name { get; set; }
    // Indicates if the task has been completed
    public bool IsComplete { get; set; }
    // It's possible to assign a contact person to a task: -1 indicated no contact
    // person is assigned, otherwise the id of the contact person
    public int LinkedContactId { get; set; } = -1;
}
```

In the code above (`ToDoService.SendReminderIfNeeded`) we see that the essential logic of sending an e-mail is to be found in the `NotificationService` class. Indeed, this class is at the center of our investigation. The following code snippet describes the code for the `NotificationService` class and its dependencies:

```csharp
// Class for sending notifications
class NotificationService
{
    // Dependencies of the class
    EMailSender _emailSender;
    Logger _logger;
    ContactRepository _contactRepository;

    public NotificationService(string smtpAddress)
    {
        _logger = new Logger();
        _emailSender = new EMailSender(_logger, smtpAddress);
        _contactRepository = new ContactRepository();
    }

    // Sends an email notification to the contact with the given ID
    // (contactId is a key in the Contacts table)
    public void SendEmailReminder(int contactId, string todoMessage)
    {
        string emailTo = _contactRepository.GetContactEMailAddress(contactId);
        string emailSubject = "TODO reminder";
        string emailMessage = "Reminder about the following todo item: " + todoMessage;
        _emailSender.SendMail(emailTo, emailSubject, emailMessage);
    }
}

// Class supporting loggin
public class Logger
{
    public void LogInformation(string text) { /* ...*/ }
    public void LogError(string text) { /* ...*/ }
}

// Class for sending e-mail notifications
public class EMailSender
{
    Logger _logger;
    string _smtpAddress;

    public EMailSender(Logger logger, string smtpAddress)
    {
        _logger = logger;
        _smtpAddress = smtpAddress;
    }
    public void SendMail(string to, string subject, string message)
    {
        _logger.LogInformation($"Sendding e-mail. To: {to} Subject: {subject} Body: {message}");

        // ...
    }
}

// Class for Contact entity persistence
public class ContactRepository
{
    public string GetContactEMailAddress(int contactId)
    {
        // ...
    }
    // ...
}
```

A few general thoughts:

* The `NotificationService` class has several dependencies (`EMailSender`, `Logger`, `ContactRepository` classes) and it implements its services based on these dependency classes.
* Dependency classes may have additional dependencies: `EMailSender` is a great example of this, it's dependent on the `Logger` class.
* Note: `NotificationService`, `EMailSender`, `Logger`, `ContactRepository` classes are considered __service classes__ because they contain business logic, not just encapsulate data, such as `TodoItem`.

As we could see the `SendEmailReminder` operation is actually served by an object graph, where `NotificationService` is the root object, it has three dependencies, and its dependencies have further dependencies. The following figure illustrates this object graph:

![Object graph 1](images/object-graph-1.svg)

!!! note "Note"
    One may ask why we considered `NotificationService`, and not `ToDoService` as the root object. Actually it just depends on our viewpoint: for simplicity we considered ToDoService as an entry point (a "client") for fulfilling a request, so that we have less classes to put under scrutiny. In a real life application we probably would consider `ToDoService` as part of the dependency graph as well.
  
Let's review the key features of this solution:

* The class instantiates its dependencies itself
* Class depends on the specific type of its dependencies (and not on interfaces, "abstractions")

This approach has a couple of significant and rather painful drawbacks:

1. __Rigidity, lack of extensibility__. `NotificationService` (without modification) cannot work with other mailing, logging and contact repository implementations (but only with the the wired in `EMailSender`, `Logger` and `ContactRepository` classes). That is, e.g. we can't use it with any other logging component, or e.g. use it with a contact repository that operates via a different data source/storage mechanism.
2. __Lack of unit testability__. The `NotificationService` (without modification) cannot be unit tested. This would require replacing the `EMailSender`, `Logger` and `ContactRepository` dependencies with variants that provide fixed/expected responses  for a given input. Keep in mind that unit testing is about testing the behavior of a class independently from its dependencies. In our example, instead of using the database base ContactRepository, we would need a ContactRepository implementation that could serve requests very quickly from memory with values supporting the specific test cases.
3. There is one more subtle inconvenience that is hard to notice at first sight. In our example we had to provide the `smtpAddress` parameter to the `NotificationService` constructor, so that it can forward it to its `EMailSender` dependency. However, `smtpAddress` is a parameter completely meaningless for `NotificationService`, it has nothing to do with this piece of information. Unfortunately, we are forced to pass `smtpAddress` thorough `NotificationService`, as `NotificationService` is the class instantiating the `EMailSender` object. **We could eliminate this by somehow instantiating `EMailSender` independently of `NotificationService`**.

In the next steps, we redesign our solution so that we can eliminate most of the downsides of the current rigid approach.

## Example phase 2 - service class with manual dependency injection

Redesign our former solution the functional requirements are unchanged.
The most important principles of transformation are the following:

* __Dependencies will be based on abstractions/interfaces__
* __Classes will no longer instantiate their dependencies themselves__
  
Let's jump right into the code of the improved solution and then analyze the differences:

```csharp
public class ToDoService
{
    const string smtpAddress = "smtp.myserver.com";

    // Checks the todoItem object received as a parameter and sends an e-mail
    // notification about the to-do item to the contact person specified by the
    // todo item.
    public void SendReminderIfNeeded(TodoItem todoItem)
    {
        if (checkIfTodoReminderIsToBeSent(todoItem))
        {
            var logger = new Logger();
            var emailSender = new EMailSender(logger, smtpAddress);
            var contactRepository = new ContactRepository();

            NotificationService notificationService
                = new NotificationService(logger, emailSender, contactRepository);
            notificationService.SendEmailReminder(todoItem.LinkedContactId,
                todoItem.Name);
        }
    }

    bool checkIfTodoReminderIsToBeSent(TodoItem todoItem)
    {
        bool send = true;
        /* ... */
        return send;
    }
}

// Class for sending notifications
class NotificationService
{
    // Dependencies of the class
    IEMailSender _emailSender;
    ILogger _logger;
    IContactRepository _contactRepository;

    public NotificationService(ILogger logger, IEMailSender emailSender,
        IContactRepository contactRepository)
    {
        _logger = logger;
        _emailSender = emailSender;
        _contactRepository = contactRepository;
    }

    // Sends an email notification to the contact with the given ID
    // (contactId is a key in the Contacts table)
    public void SendEmailReminder(int contactId, string todoMessage)
    {
        string emailTo = _contactRepository.GetContactEMailAddress(contactId);
        string emailSubject = "TODO reminder";
        string emailMessage = "Reminder about the following todo item: " + todoMessage;
        _emailSender.SendMail(emailTo, emailSubject, emailMessage);
    }
}

#region Contracts (abstractions)

// Interface for logging
public interface ILogger
{
    void LogInformation(string text);
    void LogError(string text);
}

// Interface for sending e-mail
public interface IEMailSender
{
    void SendMail(string to, string subject, string message);
}

// Interface for Contact entity persistence
public interface IContactRepository
{
    string GetContactEMailAddress(int contactId);
}

#endregion

#region Implementations

// Class for logging
public class Logger: ILogger
{
    public void LogInformation(string text) { /* ...*/  }
    public void LogError(string text) {  /* ...*/  }
}

// Class for sending e-mail
public class EMailSender: IEMailSender
{
    ILogger _logger;
    string _smtpAddress;

    public EMailSender(ILogger logger, string smtpAddress)
    {
        _logger = logger;
        _smtpAddress = smtpAddress;
    }
    public void SendMail(string to, string subject, string message)
    {
        _logger.LogInformation($"Sendding e-mail. To: {to} Subject: {subject} Body: {message}");

        // ...
    }
}

// Class for Contact entity persistence
public class ContactRepository: IContactRepository
{
    public string GetContactEMailAddress(int contactId)
    {
        // ...
    }
    // ...
}

#endregion
```

We improved out previous solution in the following points:

* The `NotificationService` class no longer instantiates its dependencies itself, but receives them in constructor parameters.
* Interfaces (abstractions) have been introduced to manage dependencies
* The `NotificationService` class gets its dependencies in the form of interfaces. When a class receives its dependencies externally (e.g. via constructor parameters), it is called __DEPENDENCY INJECTION__ (DI).
* In our case, the classes get their class dependencies in constructor parameters: this specific form of DI is called __CONSTRUCTOR INJECTION__. This is the most common - and most recommended - way to inject dependency. (Alternatively, for example, we could use property injection, which is based on a public property setter to set a specific dependency of a class).
  
In our current solution, `NotificationService` dependencies are instantiated by the (direct) USER of the class (which is the `ToDoService` class). Primarily this is the reason why we are still facing with a few problems:

1. The user of `NotificationService` objects, which is the `ToDoService` class, is still dependent on the implementation types (since it has to instantiate the `Logger`,`EMailSender` and `ContactRepository` classes).
2. If we use the `Logger`, `EMailSender` and `ContactRepository` classes at multiple places in your application, we must instantiate them explicitly. In other words: at each and every place where have to create an `ILogger`, `IEMailSender` or `IContactRepository` implementation class, we have to make a decision which implementation to choose. This is essentially a special case of code duplication, the decision should appear only once in our code.
    * Our goal, in contrast, would be to determine __at a single central location__ what type implementation to use for an abstraction (interface type) __everywhere__ in the application (e.g. for `ILogger` create an `Logger` instance everywhere, for `IMailSender` create an `EMailSender` everywhere).
    * This would allow us to easily review our abstraction-to-implementation mappings at one place.
    * Moreover, if we want to change one of the mappings (e.g. using `AdvancedLogger` instead of `Logger` for `ILogger`) we could achieve that by making a single change at a central location.

## Example phase 3 - dependency injection based on .NET Dependency Injection

We need some extra help from our framework to solve the two problems we concluded the previous chapter with: an __Inversion of Control (IoC)__ container (also called as Dependency Injection container). __Dependency Injection container__ is a widely used alternative name for the same tool/technique. In an IoC container we can store abstraction type -> implementation type mappings, such as ILogger->Logger, IMailSender->EMailSender, etc. This is called the REGISTER step. And then based on these mappings create an implementation type for a specific abstraction type (e.g. `Logger` for an `ILogger`). This is called the RESOLVE step. In more detail:

1. __REGISTER__: Register dependency mappings (e.g. `ILogger`-> `Logger`, `IMailSender`-> `EMailSender`) into an IoC container, once, at a centralized location, at application startup. This is the __REGISTER__ step of the DI process.
   * Note: This solves "problem 2" pointed out at the end of the previous chapter: the mappings are centralized, and not scattered all over the application code base.
2. __RESOLVE__: When we need an implementation object at runtime in our application, we ask the container for an implementation by specifying the abstraction (interface) type (e.g., by providing `ILogger` as a key, the container returns an object of class `Logger`).
    * The resolve step is typically done at the "__entry point__" of the application (e.g. in case of WebApi on the receival of web requests, we will look into this later). The __resolve step is performed only for the ROOT OBJECT__ (e.g. for the appropriate Controller class in case of WebApi). The container creates and returns a root object and all its dependencies and all its indirect dependencies: an entire object graph is generated. This process is called __AUTOWIRING__.
    * Note: In case of Web API calls, the Resolve step is executed by the Asp.Net framework and is mostly hidden from the developer: all we see is that our controller class is automatically instantiated and all constructor parameters are automatically populated (with the help of the IoC container based on the mappings of the REGISTER step).

Fortunately, .NET has a built in IoC container based dependency injection service.
Now we elucidate and illustrate the complete mechanism (register and resolve steps) using our enhanced e-mail notification solution as an example.

### 1) REGISTER step (registering dependencies)

In an Asp.Net Core environment, dependencies are registered in the 'Program.cs' file. This file has code that is executed at application startup. The code parts located here and which are relevant for us:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ...
builder.Services.AddSingleton<ILogger, Logger>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddSingleton<IEMailSender, EMailSender>(
    sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );
// ...
```

The first line creates a `builder` object, whose `Services` property is an object implementing the `IServiceCollection`  interface. This represents the IoC container created by the framework, this can be used to register our dependency mappings as well, namely the  __AddSingleton__, __AddTransient__ and __AddScoped__ operations of `IServiceCollection` interface can be used to register them.

!!! note "Note"
    In .NET versions prior to .NET 6 the instead of `Program.cs` the `ConfigureServices` operation of the `Startup` class was used to register these dependencies.

The

```csharp
builder.Services.AddSingleton<ILogger, Logger>();
```

line registers an `ILogger`-> `Logger` type mapping, and the `Logger` is registered as a singleton, as we used the __AddSingleton__ operation for registration. This means that if we later ask the container for an `ILogger` object (provide `ILogger` as key at the resolve step), we will get a `Logger` object from the container, and always __the same instance__. The

```csharp
builder.Services.AddTransient<INotificationService, NotificationService>();
```

line registers an `INotificationService`-> `NotificationService` transient type mapping, as we used the __AddTransient__ operation for registration. This means that if we later ask the container for an `INotificationService` object (provide `INotificationService` as key at the resolve step), we will get a __separate newly created instance__ of `NotificationService` object from the container, for each query/resolve.

```csharp
builder.Services.AddScoped<IContactRepository, ContactRepository>();
```

line registers an `IContactRepository`-> `ContactRepository` scoped type mapping, as we used the __AddScoped__ operation for registration. This means that if we later ask the container for an `IContactRepository` object (provide `IContactRepository` as key at the resolve step), we will get a `NotificationService` object, which will be the  __same instance for the same scope__, and a different instance for different scopes. For a Web API based application one web request is handled within one scope. Consequently, we receive the same instance of a class turning to the container multiple times within the same web request, but different ones when the web requests are different.

We can see additional registrations in the sample application, which we will return to later.

### 2) RESOLVE step (resolving dependencies)

#### The basics

Let's sum up where we are now: we have our abstraction to implementation type mappings registered into the ASP.NET Core IoC container at application startup. Our mappings are the following:

* ILogger -> Logger as singleton
* INotificationService -> NotificationService as transient
* IContactRepository -> ContactRepository as scoped
* IEMailSender -> EMailSender as singleton

From now on, whenever we need an instance of an implementation type for an abstraction, we can ask the container for it using the abstraction type as the key. How do we specifically do it in a .NET Core application? .NET Core provides an `IServiceProvider` reference to us, and we can use different forms of the `GetService` operation of this interface. E.g.:

```csharp
void SimpleResolve(IServiceProvider sp)
{
    // Returns an instance of the Logger class, as we have
    // registered the Logger implementation type for our ILogger abstraction.
    var logger1 = sp.GetService(typeof(ILogger));

    // Same as the previous example. The difference is that we have provided
    // the type as a generic parameter. This is a more convenient approach.
    // To use this we have to import the Microsoft.Extensions.DependencyInjection
    // namespace via the using statement.
    // Returns an instance of the Logger class, see explanation above.
    var logger2 = sp.GetService<ILogger>();

    // GetService returns null if no type mapping is found for the specific type (ILogger)
    // GetRequiredService throws an exception instead.
    var logger3 = sp.GetRequiredService<ILogger>();
    // ...
}
```

In the above example, code comments explain the behavior in detail. In each case, an abstraction type is to be provided for the `GetService`/`GetRequiredService` operation (either via the `typeof` operator, or via a generic parameter), and the operation returns with an instance of an implementation type based on the type mappings registered in the container.

#### Object graph resolution, autowiring

In the previous example, the container was able to instantiate the `Logger` class at the resolve step without any major 'headaches', since it has no additional dependencies: it has a single default constructor. Now consider the resolution of `INotificationService`:

```csharp
public void ObjectGraphResolve(IServiceProvider sp)
{
    var notifService = sp.GetService<INotificationService>();
    // ...
}
```

At the resolve step (GetService call), the container must create a `NotificationService` object. In doing so, it has to provide valid values for its constructor parameters, which actually means that has to resolve the class's direct and indirect dependencies, recursively:

* The NotificationService class has a three-parameter constructor (that is, it has three dependencies): `NotificationService (ILogger logger, IEMailSender emailSender, IContactRepository contactRepository)`. The `GetService` resolves constructor parameters one by one based on IoC container mapping registrations:
    * `ILogger` logger: a `Logger` object is provided by the container, always the same instance (as ILogger->Logger mapping is registered as singleton)
    * `IEMailSender` emailSender: an `EMailSender` object is provided by the container, a different instance in each case (as mapping is registered as transient)
        * The `EMailSender` constructor has an `ILogger logger` parameter, that has to be resolved as well: a `Logger` object is provided by the container, always the same instance (as registered as singleton)
    * `IContactRepository` contactRepository: a `ContactRepository` object is provided by the container, a different instance for different scopes (Web API e.g. for different Web API calls), as mapping is registered as scoped.

Summing up: the `GetService<INotificationService>()` call above creates a fully parameterized `NotificationService` object with all of its direct and indirect dependencies, the call returns an __object graph__ for us:

![Object graph 2](images/object-graph-2.svg)

As we have seen in this example, IoC containers/DI frameworks are capable of determining the dependency requirements of objects (by examining at their constructor parameters), and then creating entire object graphs based on upfront abstraction->implementation container type mappings. This process is called __autowiring__.

#### Dependency resolution  for ASP.NET Web API classes

Besides making our solution IoC container based, we make a few further changes to our todo app. We eliminate our `ToDoService` class, and move its functionality in a slightly different form into  an Asp.Net Core based `ControllerBase` derived class. This controller class will serve as our entry point and also as a root object, bringing our solution very close to a real life example (let it be a Web API, Web MVC app or a Web Razor Pages app). We could also have kept `ToDoService` in the middle of our call/dependency chain, but we try to keep things as simple as possible for our demonstration purposes. Furthermore, we also introduce an Entity Framework `DbContext` derived class called `TodoContext` to be able to demonstrate how it can be injected into repository classes in a typical application. Our new object graph will look like this:

![Object graph 3](images/object-graph-3.svg)

In the previous two chapters, we have assumed that a `IServiceProvider` object is available to call `GetService`. If we create a container ourselves, then this assumption is valid. However, only in the rarest cases do we create a container directly. In a typical ASP.NET Web API application, the container is created by the framework and is not directly accessible to us. Consequently, access to `IServiceProvider ', with the exception of a few startup and configuration points, is not available. The good news is that actually we don't need access to the container. __The core concept of DI is that we perform dependency resolution only at the application entry point for the "root object".__ In case of Web API apps, the entry point is a call to an operation of a Controller class serving the specific API request. When a request is received, the framework determines and creates the Controller / ControllerBase child class based on the Url and routing rules. If the controller class has dependencies (has constructor parameters), they are also resolved based on the container registration mappings, including indirect dependencies. The complete object graph is created, __the root object is the controller class__.

Let's take a look at this in practice by refining our previous example with the addition of a `TodoController` class:

```csharp
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    // Dependencies of the TodoController class
    private readonly TodoContext _context; // this is a DbContext
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
        // Look up todo item
        var item = _context.TodoItems.Find(id);
        if (item == null)
            return NotFound();

        // Rend reminder e-mail
        _notificationService.SendEmailReminder(item.LinkedContactId, item.Name);

        // Actually we don't create anything here, simply return an OK
        return Ok();
    }

    // ... further operations
}
```

Requests under the `http://<base_address>/api/todo` url are routed to the `TodoController` class based on the routing rules. The mail sending request (`http://<base_address>/api/todo/<todo-id>/reminder`) is routed to its `TodoController.ReminderMessageToLinkedContact` operation. A `TodoController` object is instantiated by the framework, creating a new instance for each request. The `TodoController` class has two dependencies provided as constructor parameters. The first is a `TodoContext` object, which is a `DbContext` derived class. The other is an `INotificationService`, (which we already covered in our previous example). As we saw in the previous section, the DI framework can create these objects based on the container registered mappings (with all their indirect dependencies), and then pass them to the `TodoController` as constructor parameter, where they are stored in member variables. The entire object graph is created, with `TodoController` as the root object. This object graph is to serve the specific web API request.

!!! note "Note"
    The resolution of `TodoContext` is only possible if it's pre-registered in the IoC container. We will discuss this in the next chapter.

## Entity Framework DbContext container registration and resolution

In applications, especially in Asp.Net Core based ones, there are two ways to use DbContext:

* Each time it is needed, we create and dispose it with the help of a using block. This can result in the creation of multiple DbContext instances serving an incoming request (which is absolutely OK).
* We create one `DbContext`  for a specific incoming request and share it for the classes involved in serving the request. In this case, we think of the `DbContext` instance as a unit of work serving the request.

To accomplish this latter approach, ASP.NET Core provides a handy built-in DI based solution: when we configure our container with the type mappings at startup, we also register our DbContext class, which is then later automatically injected for our __Controller__ and other (typically repository) dependencies.

Let's see how our `TodoContext` (`DbContext` derived) class is registered in our example. The place of the registration is the usual `Program.cs` file (`Startup.ConfigureServices` for .NET versions prior to .NET 6):

```csharp
// ...
builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
// ...
```

`AddDbContext` is an extension method defined by the framework for the `IServiceCollection` interface. This allows convenient registration of our `DbContext` class. We do not see into the implementation of `AddDbContext`, but actually it simply performs a scoped registration of our context type into the container:

```csharp
services.AddScoped<TodoContext, TodoContext>();
```

As shown in the example, `TodoContext` __is not registered via an abstraction__ (no `ITodoContext` interface exists) __, but via the TodoContext implementation type itself. DI frameworks / IoC containers support the key part of a mapping to be a specific type, e.g. the implementation type itself__. Use this approach only when justified, e.g. when we don't need extensibility for the specific type, and introducing an abstraction (interface) would only complicate the solution.

In an Asp.Net Core environment, we don't introduce an interface for our `DbContext` derived class: instead, we always register it with the type of its class to the IoC container (in our example `TodoContext`-> `TodoContext` mapping). `DbContext` itself can work with many persistent providers (e.g. MSSQL, Oracle, in-memory, etc.), so in many cases it does not make sense to put it behind further abstractions. In those cases when we need to abstract data access, we do not introduce an interface to access `DbContext`. Instead, we use the Repository design pattern, and we introduce  interfaces for each repository implementations classes, and then register their mappings to the IoC container (e.g. `ITodoRepository`-> `TodoRepository`). The repository classes either instantiate the `DbContext` objects themselves or the `DbContext` is injected as constructor parameter).

!!! note "Note"
    This document does not intend to make a standpoint over the often disputed question, whether it makes or does not make sense introducing a repository layer in an Entity Framework based application. For illustration purposes, our TodoApi application uses a mixed solution in this sense: controller/service classes use DbContext directly to persist TodoItem objects, and use the Repository pattern to handle Contacts. Don't mix the two approaches in a real-life application.

The example above also shows that you can also provide a lambda expression when registering `DbContext` (in case `TodoContext`) using `AddDbContext`:

```csharp
opt => opt.UseInMemoryDatabase("TodoList")
```

This lambda expression is called by the container later at the resolve step - that is, every time when a `TodoContext` is instantiated. An option object is provided as a parameter (in this example, the `opt` argument): this allows us to configurate the instance created by the container. In our example, calling the `UseInMemoryDatabase` operation creates an in-memory based database called "TodoList".

## Advanced dependency injection registration example

!!! note ""
    Not compulsory material.

Let's cover code service registration related parts of `Program.cs` we skipped previously.

The registration of `EMailSender` looks quite tricky:

```csharp
builder.Services.AddSingleton<IEMailSender, EMailSender>(
    sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );
```

Let's take a look at the constructor of `EMailSender` to be able to better understand the situation:

```csharp
public EMailSender(ILogger logger, string smtpAddress)
{
    _logger = logger;
    _smtpAddress = smtpAddress;
}
```

`EMailSender` will need to be instantiated by the container when resolving `IEMailSender`, and the constructor parameters must be specified appropriately. The logger parameter is completely "OK", and the container can resolve it based on the ILogger-> Logger container mapping registration. However, there is no way to find out the value of the `smtpAddress` parameter. To solve this problem, ASP.NET Core proposes an "options" mechanism for the framework, which allows us to retrieve the value from some configuration. Covering the "options" topic would be a far-reaching thread for us, so for simplification we applied another approach. The `AddSingleton` (and other Add ... operations) have an overload in which we can specify a lambda expression. This lambda is called by the container later at the resolve step (that is, when we ask the container for an `IEMailSender` implementation) for each instance. With the help of this lambda we manually create the `EMailSender` object, so we have the chance to provide the necessary constructor parameters. In fact, the container is really "helpful" with us:  it provides an `IServiceCollection` object as the lambda parameter for us (in this example it's called `sp`), and based on container registrations we can conveniently resolve types with the help of the already covered `GetRequiredService` and `GetService` calls.

## Further topics

### Dependency Injection/IoC containers in general

The particularities of the DI container built in ASP.NET Core:

* It provides basic services required by most applications (e.g., does not support property injection).
    * If you need more DI related functionality, you can use another IoC container Asp.Net Core can work with.
    * Several Dependecy Injection / IoC container class libraries exist that can be used with .NET, with .NET Framework, or with both. A few examples: AutoFac, DryIoc, LightInject, Castle Windsor, Ninject, StructureMap, SimpleInjector, MEF, ...
* It's implemented in the __Microsoft.Extensions.DependencyInjection__ NuGet package.
    * For Asp.Net Core applications, it is automatically installed when the Asp.Net project is created. In fact, as we have seen, Asp.Net Core middleware heavily relies on it, it's a key pillar of runtime configuration and extensibility.
    * For other .NET applications (e.g. a simple .NET Core based console app), you need to add it manually by installing the Microsoft.Extensions.DependencyInjection NuGet package for the project.
    * Note: the NuGet package can be used with the (full) .NET Framework as well as it supports .NET Standard.

### The Service Locator antipattern

Dependency injection is not the only way of using an IoC container. Another technique called __Service Locator__ exists. Dependency Injection is based on the mechanism of passing the dependencies of a class as constructor parameters. Service Locator uses another approach: the classes directly access the IoC container in their methods to resolve their dependencies. Keep in mind that this approach is considered an __anti-pattern__. The reason is simple: every time time a class needs a dependency, it has to turn to a container, so much of our code will depend on the container itself! In contrast, when dependency injection is used, dependency resolution is performed "once" at the application entry point for "root objects" (e.g. for the controller class in case of a Web API call), the rest of our code is completely independent of the container. Note that in our previous example, in our TodoController, NotificationService, EMailSender, Logger, and ContactRepository classes, we did not refer the container (neither via an IServiceProvider, nor by any other means).

### Asp.Net Core framework services

Asp.Net Core has several built in services. E.g. it has support for Web API, and support for Razor Pages or MVC based web applications. These all rely on the DI services of Asp.Net Core.

In case of an Asp.Net Web API application at application startup we have to run this piece of code (this is automatically added by VS at project creation):

```csharp
builder.Services.AddControllers();
```

!!! note
    In case of .NET version preceding .NET 6 `services.AddMvc()` had to be called from the `ConfigureServices` operation of our `Startup` class.

`AddControllers` is a built in extension method for the `IServiceProvider` interface, which registers numerous (far more than 100!) service and configuration classes into the container required by the internals of the Web API middleware/pipeline.



### Disposing service objects

The container calls `Dispose` for the objects it creates if the object implements the `IDisposable` interface.

### Resources

* <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection>
* <https://stackify.com/net-core-dependency-injection/amp>
* <https://medium.com/volosoft/asp-net-core-dependency-injection-best-practices-tips-tricks-c6e9c67f9d96>
