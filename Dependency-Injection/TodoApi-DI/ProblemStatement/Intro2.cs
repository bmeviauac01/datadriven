#define Intro2
#if Intro2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.ProblemStatement2
{
    // #2 NotificationService PÉLDA, RÉSZLEGES MANUÁLIS FÜGGŐSÉG INJEKTÁLÁSSAL
    // A funkcinális követelmények változatlanok.
    // A korábbi megoldást a következő pontokban fejlesztettük tovább:
    // * A NotificationService osztály már nem maga példányosítja a függőségeit, hanem konstruktor
    //   paraméterekben kapja meg.
    // * Interfészeket (absztrakciókat) vezettünk be a függőségek kezelésére
    // * A NotificationService osztály a függőségeit interfészek formájában kapja meg.
    //   Azt, amikor egy osztály a függőségeit kívülről kapja meg, DEPENDENCY INJECTION-nek (DI), 
    //   vagyis függőséginjektálásnak nevezzük. Esetünkben konstruktor paraméterekben kapta meg, 
    //   ez a DI leggyakoribb megvakósítási módja.
    // * A NotificationService osztály függőségeit a HASZNÁLÓJA pédányosítja
    // Ezen elvek mentén szinte minden korábbi probléma megoldódott.
    // Ezek a problémák állnak fenn most:
    // * A NotificationService felhasználója, vagyis a ToDoService.SendReminderIfNeeded még mindig függ a konkrét típusoktól
    //   hiszen szükséges példányosítania a Logger, EMailSender és ContactRepository osztályokat.
    // * Ha több helyen használjuk a Logger, EMailSender és ContactRepository osztályokat, mindenhol
    //   külön-külön példányosítani kell őket. Vagyis NEM EGYETLEN KÖZPONTI HELYEN határozzuk meg, 
    //   hogy milyen interfész típus esetén milyen implementációt kell MINDENOL használni az alkalmazásban.
    //   (pl. ILogger->Logger, IMailSender->EMailSender).
    //
    // A tovább javított megoldásunk a következő lesz:
    // * Felhasználjuk a .NET Core dependency injection (más néven IoC container) szolgáltatását. 
    //   Ennek keretében:
    //   * Az alkalmazás indulásakor egyszer, központlag egy IoC konténerbe beregisztráljuk a
    //     függőségi leképezéseket (pl.  ILogger->Logger, IMailSender->EMailSender). Ez a REGISTER lépése.
    //   * Amikor szükségünk van egy implementációra, a konténertől az interfészt megadva kérünk egy
    //     implementációt (pl. ILoggert megadjuk, Loggert kapunk). Ez a RESOLVE lépés.
    //   * Ez utóbbit az alkalmazás "belépési pontjában" (pl. WebApi esetén az egyes hívások beérkezésekor)
    //     tesszük meg, a "root object"-re (pl. ez WebApi esetén a Controller osztály). A Resolve lépés
    //     legyártja a root objectet, illetve annak valamennyi függőségét, és valamennyi közvetett 
    //     függőségét, előáll egy objektumgráf. Web API esetén a RESOLVE lépést a keretrendszer végzi el
    //     előlünk el van rejtve: mi csak annyit tapasztalunk, hogy a Controller osztályunk automatikusan
    //     példányosítódik, és valamennyi kontruktor paramétere automatikusan kitöltésre kerül (a
    //     REGISTER lépés regisztrációi alapján).


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


    // Értesítések küldésére szolgáló osztály
    class NotificationService 
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

    // Naplózást támogató interfész
    public interface ILogger
    {
        void LogInformation(string text);
        void LogError(string text);
    }

    // E-mail küldésre szolgáló interfész
    public interface IEMailSender
    {
        void SendMail(string to, string subject, string message);
    }

    // Contact-ok perzisztens kezelésére szolgáló interfész
    public interface IContactRepository
    {
        string GetContactEMailAddress(int contactId);
    }

    #endregion

    #region Implementations

    // Naplózást támogató osztály
    public class Logger: ILogger
    {
        public void LogInformation(string text) { /* ...*/  }
        public void LogError(string text) {  /* ...*/  }
    }

    // E-mail küldésre szolgáló osztály
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

    // Contact-ok perzisztens kezelésére szolgáló osztály
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