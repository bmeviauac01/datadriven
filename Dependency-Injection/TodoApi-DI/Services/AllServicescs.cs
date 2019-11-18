using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Services
{
    // Class for sending notifications
    class NotificationService : INotificationService
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

    public interface INotificationService
    {
        void SendEmailReminder(int contactId, string todoMessage);
    }

    public interface ILogger
    {
        void LogInformation(string text);
        void LogError(string text);
    }

    public interface IEMailSender
    {
        void SendMail(string to, string subject, string message);
    }

    public interface IContactRepository
    {
        string GetContactEMailAddress(int contactId);
    }

    #endregion

    #region Implementations

    // Class for logging
    public class Logger : ILogger
    {
        public void LogInformation(string text) { Debug.WriteLine("Info - " + text); }
        public void LogError(string text) { Debug.WriteLine("Error - " + text); }
    }

    // Class for sending e-mail
    public class EMailSender : IEMailSender
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
    public class ContactRepository : IContactRepository
    {
        TodoContext _db;

        // We can inject a TodoContext dependency as constructor parameter
        public ContactRepository(TodoContext db) => _db = db;

        public string GetContactEMailAddress(int contactId)
        {
            // TODO implement
            // We could use the TodoContext _db; object here
            return "name@gmail.com";
        }
        // ...       
    }

    #endregion

 
}
