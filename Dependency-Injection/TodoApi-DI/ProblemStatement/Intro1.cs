#define Intro1
#if Intro1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.ProblemStatement1
{
    // #1 NotificationService PÉLDA, BEÉGETETT FÜGGŐSÉGEKKEL
    // A NotificationService több függőséggel rendelkezik (EMailSender, Logger, ContactRepository)
    // Van egy közvetett függőség is (EmailSender függ a Loggertől)
    // Két probléma:
    // 1. Az osztály a függőségeit maga példányosítja
    // 2. Az osztály a függőségei konkrét típusától függ (nem interfészektől)
    // Negatív következmények:
    // * A NotificationService nem tud más EMailSender, Logger és ContactRepository implementációkkal
    //   működni, rugalmatlanság (laza csatolás hiánya)
    // * A NotificationService osztály nem unit tesztelhető: ehhez le kellene cserélni az EMailSender, Logger, 
    //   ContactRepository függőségeket olyanokra, melyek fake (tesztelést segítő) rögzített válaszokat adnak.
    // * Kellemetlen, hogy a NotificationService-nek a függőségei paramétereit is át kell adni (smtpAddress).


    // Teendők kezelésére szolgáló osztály
    public class ToDoService
    {
        const string smtpAddress = "smtp.myserver.com";

        // Megvizsgálja a paraméterként kapott todoItem objektumot, és ha szükséges,
        // e-mail értesítést küld a teendőről a teendőben szereplő kontakt személynek.
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

    // Entitásosztály, egy végrehajtandó feladat adatait zárja egységbe
    public class TodoItem
    {
        // Adatbázis kulcs
        public long Id { get; set; }
        // Teendő neve/leírása
        public string Name { get; set; }
        // Jelzi, hogy a teendő elvégésre került-e
        public bool IsComplete { get; set; }
        // Egy teendőhöz lehetőség van kontakt személy hozzárendelésére:  ha -1, nincs
        // kontakt személy hozzárendelve, egyébként pedig a kontakt személy azonosítója.
        public int LinkedContactId { get; set; } = -1;
    }

    // Értesítések küldésére szolgáló osztály
    class NotificationService 
    {
        // Az osztály függőségei
        EMailSender _emailSender;
        Logger _logger;
        ContactRepository _contactRepository;

        public NotificationService(string smtpAddress)
        {
            _logger = new Logger();
            _emailSender = new EMailSender(_logger, smtpAddress);
            _contactRepository = new ContactRepository();
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

    // Naplózást támogató osztály
    public class Logger
    {
        public void LogInformation(string text) { /* ...*/ }
        public void LogError(string text) { /* ...*/ }
    }

    // E-mail küldésre szolgáló osztály
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

    // Contact-ok perzisztens kezelésére szolgáló osztály
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