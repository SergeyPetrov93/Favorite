using System;
using System.Web.Mvc;
using AmgPerformanceTour.Services;
using AmgPerformanceTour.ViewModels;

namespace AmgPerformanceTour.Controllers
{
    public class EventController : Controller
    {
        private readonly Lazy<IEventService> _eventService;
        private readonly Lazy<IMailingService> _mailingService;
        private readonly Lazy<IMailingsLogsService> _mailingsLogsService;

        public EventController(Lazy<IEventService> eventService, Lazy<IMailingService> mailingService, Lazy<IMailingsLogsService> mailingsLogsService)
        {
            _eventService = eventService;
            _mailingService = mailingService;
            _mailingsLogsService = mailingsLogsService;
        }

        [HttpGet]
        public ActionResult Denial(Guid id)
        {
            return View(_eventService.Value.GetDenyData(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Denial(DenialViewModel denialData)
        {
            if (!ViewData.ModelState.IsValid)
                return View(denialData);

            try
            {
                _eventService.Value.Deny(denialData);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Global", e.Message);

                return View(denialData);
            }

            return View("DenialResult");
        }

        [HttpGet]
        public HttpStatusCodeResult SendNoticesForManagers(string appKey)
        {
            try
            {
                if (appKey != "AmgPerformanceTour.MailingReminder")
                    return new HttpStatusCodeResult(400);

                _mailingService.Value.NoticesForManagers();

                return new HttpStatusCodeResult(200);
            }
            catch
            {
                return new HttpStatusCodeResult(500);
            }
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public FileContentResult TrackMailing(int mailingLogId, Guid participantId)
        {
            _mailingsLogsService.Value.TrackMailing(mailingLogId, participantId);

            const string clearGif1X1 = "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==";

            return new FileContentResult(Convert.FromBase64String(clearGif1X1), "image/gif");
        }

        [HttpGet]
        public ActionResult Registration(int? sId, string pId, bool iframe = false)
        {
            return iframe ? View("Iframe", _eventService.Value.Registration(sId, pId,null, true,false)) : View(_eventService.Value.Registration(sId, pId,null, false,false));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(RegistrationFormViewModel entity)
        {
            if (!ViewData.ModelState.IsValid)
            {
                return entity.Iframe != null && entity.Iframe.Value ? View("Iframe", _eventService.Value.FillDictionaries(entity)) : View(_eventService.Value.FillDictionaries(entity));
            }

            try
            {
                _eventService.Value.Registration(entity);

                ViewBag.Message = "Благодарим за заявку! В ближайшее время Вы получите приглашение на ваш адрес электронной почты.";
            }
            catch
            {
                ViewBag.Message = "Ошибка отправки формы. Обновите страницу и повторно заполните форму.";
            }

            return entity.Iframe != null && entity.Iframe.Value ? View("Send") : View("RegistrationResult");
        }
    }
}
