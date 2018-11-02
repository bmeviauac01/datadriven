using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Services
{
    // Értesítések küldésére szolgáló osztály
    class NotificationService: INotificationService
    {
        // Az osztály függőségei
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

        // E-mail értesítést küld az adott azonosítójú kontakt személynek (a contactId
        // egy kulcs a Contacts táblában)
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

    public class Logger : ILogger
    {
        public void LogInformation(string text) { Debug.WriteLine("Info - " + text); }
        public void LogError(string text) { Debug.WriteLine("Error - " + text); }
    }

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

    // Contact-ok perzisztens kezelésére szolgáló osztály
    public class ContactRepository : IContactRepository
    {
        TodoContext _db;

        // A ContactRepository-nek a konstruktorban be tudjuk injektálni a ContactRepository objektumot.
        public ContactRepository(TodoContext db) => _db = db;

        public string GetContactEMailAddress(int contactId)
        {
            // TODO implement
            // Szükség esetén fel tudjuk használni a _db DcContext objektumot
            return "name@gmail.com";
        }
        // ...       
    }

    #endregion

 
}
