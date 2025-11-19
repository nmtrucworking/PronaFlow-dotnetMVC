using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class CalendarController : Controller
    {
        /// <summary>
        /// Calendar view | GET: /Calendar
        /// </summary>
        /// <returns>View: Index.cshtml</returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AgendaCalendar()
        {
            return PartialView("_AgendaCalendar");
        }
    }
}