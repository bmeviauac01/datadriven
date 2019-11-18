#define Intro1
#if Intro1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.ProblemStatement1
{
    // ********************************************************************************
    // #1 Example phase 1 - service class with wired in dependecies
    // ********************************************************************************
    //
    // NotificationService has multiple dependencies (EMailSender, Logger, ContactRepository)
    // One indirect dependecy exists as well (EmailSender is dependent on Logger)
    // Two problems exist:
    // 1. The class instantiates its dependencies itself
    // 2. Class depends on the specific type of its dependencies (and not on interfaces, "abstractions")
    // Drawbacks:
    // * Rigidity, lack of extensibility. NotificationService (without modification) cannot work with 
    //   other mailing, logging and contact repository implementations.
    // * he NotificationService (without modification) cannot be unit tested. This would require replacing
    //   the EMailSender, Logger and ContactRepository dependencies with variants that provide fixed/expected 
    //   responses for a given input..
    // *  In our example we had to provide the smtpAddress parameter to the NotificationService constructor, 
    //    so that it can forward it to its EMailSender dependency. However, smtpAddress is a parameter
    //    completely meaningless for NotificationService, it has nothing to do with this piece of information. 


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

    // Class supporting logging
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
            throw new NotImplementedException();
        }
        // ...
    }

}

#endif