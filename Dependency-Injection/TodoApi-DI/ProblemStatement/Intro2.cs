#define Intro2
#if Intro2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.ProblemStatement2
{
    // ********************************************************************************
    // #2 Example phase 2 - service class with manual dependency injection
    // ********************************************************************************
    //
    // The the functional requirements are unchanged.
    // We improved out previous solution in the following points:
    // * The NotificationService class no longer instantiates its dependencies itself, but receives 
    //   them in constructor parameters.
    // * Interfaces(abstractions) have been introduced to manage dependencies
    //   The NotificationService class gets its dependencies in the form of interfaces.When a 
    //   class receives its dependencies externally(e.g.via constructor parameters), it is called
    //   DEPENDENCY INJECTION(DI).
    // * In our case, the classes get their class dependencies in constructor parameters: this 
    //   specific form of DI is called CONSTRUCTOR INJECTION.This is the most common - and most 
    //   recommended - way to inject dependency. (Alternatively, for example, we could use property 
    //    injection, which is based on a public property setter to set a specific dependency of a class).
    // * NotificationService dependencies are instantiated by the (direct) USER of the class
    // Most of the downsides of our previous solution have been eliminated.

    // We are still facing with a few problems:
    // * The user of NotificationService objects, which is the ToDoService class, is still dependent
    //   on the implementation types (since it has to instantiate the Logger,EMailSender and 
    //   ContactRepository classes).
    // * If we use the Logger, EMailSender and ContactRepository classes at multiple places in your 
    //   application, we must instatiate them explicitely. In other words: at each and every place 
    //   where have to create an ILogger, IEMailSender or IContactRepository implementation class, 
    //   we have to make a decision which implementation to choose. This is essentially a special case 
    //   of code duplication, the decision should appear only once in our code.
    //
    // We will further improve our solution implemented in this source file later (see TodoController):
    // * We will utilized the dependency injection (IoC container) services of ASP.NET Core.
    //   These will be the basics of our transformation:
    //   * We will register dependency mappings (e.g. ILogger-> Logger, IMailSender-> EMailSender) into an IoC 
    //     container, once, at a centralized location, at application startup. This is the REGISTER 
    //     step of the DI process.
    //   * When we need an implementation object at runtime in our application, we ask the container for an 
    //     implementation by specifying the abstraction (interface) type (e.g., by providing ILogger 
    //     as a key, the container returns an object of class Logger).
    //   * The resolve step is typically done at the "entry point" of the application (e.g. in case of 
    //     WebApi on the receival of web requests, we will look into this later). The resolve step is 
    //     performed only for the ROOT OBJECT (e.g. for the appropriate Controller class in case of WebApi).
    //     The container creates and returns a root object and all its dependencies and all its indirect 
    //     dependencies: an entire object graph is generated. This process is called AUTOWIRING.
    //     Note: In case of Web API calls, the Resolve step is executed by the Asp.Net framework and is mostly
    //     hidden from the developer: all we see is that our controller class is automatically instantiated 
    //     and all constructor parameters are automatically populated(with the help of the IoC container
    //     based on the mappings of the REGISTER step).


    // Class for managing todo items
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
            throw new NotImplementedException();
        }
        // ...
    }

    #endregion
}

#endif