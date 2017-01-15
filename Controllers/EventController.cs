using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Worker.Services;
using Microsoft.Extensions.Options;
using Models.Implementations;
using Models.ViewModels;
using Models;
using Models.Models;
using Worker.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Web.Models;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Web.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly EventService _eventService;
        private readonly CourseService _courseService;
        private readonly PartyService _partyService;


        public EventController(IOptions<JRSettings> options)
        {
            _eventService = new EventService(options.Value);
            _courseService = new CourseService(options.Value);
            _partyService = new PartyService(options.Value);
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {

            var currentUser = await GetUserAccount();
            var eventRoot = (await _eventService.GetAllEventsForToday());
            var courseEventViewModel = new List<CourseEventViewModel>();
            if (eventRoot != null && eventRoot.Event != null)
            {
                foreach (var @event in eventRoot.Event.Where(x=>x.enabled=="true" && x.trainer == currentUser.customData.trainer
                ))
                {
                    var course = await _courseService.Get(@event.coursenumber);
                    var courseEventRoot = new CourseEventRoot { Course = course, Event = @event };
                    courseEventViewModel.Add(new CourseEventViewModel { CourseEventRoot = courseEventRoot });
                }
            }
            return View(courseEventViewModel);
        }

        public async Task<IActionResult> ClassList(string eventId, string courseName)
        {
            var invitees = await _eventService.GetAllInviteesForTheEvent(courseName, eventId);
            var @event = await _eventService.GetEvent(eventId);
            List<ClassListViewModel> clvm = new List<ClassListViewModel>();
            var attendees = await _eventService.GetEventAttendees(courseId: courseName, eventId: eventId);
            foreach (var student in invitees.Invitees.invitee)
            {
                var party = await _partyService.Get(student.partyidentifier);
                var classListVm = new ClassListViewModel();
                if (student.partyidentifier == @event.trainer)
                {
                    classListVm.IsTrainer = true;
                }
                classListVm.IsPresent = false;
                if (attendees.Attendee != null)
                {
                    if (attendees.Attendee.Exists(x => x.partyId == student.partyidentifier))
                    {
                        var partyAttendance = attendees.Attendee.Find(x => x.partyId == student.partyidentifier);
                        classListVm.IsPresent = partyAttendance.attended == "true";
                        classListVm.IsMarked = true;
                        classListVm.Notes = partyAttendance.notes;
                    }
                }
                classListVm.FullName = $"{party.Party.firstname} {party.Party.surname}";
                classListVm.AvetmissComplete = string.IsNullOrEmpty(party.Party.usinumber) ? false : true;
                classListVm.CourseName = courseName;
                classListVm.EventId = eventId;
                classListVm.PartyId = student.partyidentifier;
                clvm.Add(classListVm);
            }
            return View(clvm);
        }

        public async Task<IActionResult> GetDefaultAttendance(string courseName, string eventId, string partyId)
        {
            var attendees = await _eventService.GetEventAttendees(courseId: courseName, eventId: eventId);
            var @event = await _eventService.GetEvent(eventId);
            var partyAttendance = new PartyAttendance();
            partyAttendance = new PartyAttendance
            {
                ArrivedAt = Convert.ToDateTime(@event.starttime).ToString("t"),
                LeftAt = Convert.ToDateTime(@event.endtime).ToString("t"),
                PartyId = partyId,
                Attened = true,

            };
            return Json(partyAttendance);
        }


        public async Task<IActionResult> MarkAttendance(PostAttendanceViewModel postAttendanceModel)
        {
            Attendee attendee = new Attendee();
            attendee.attended = postAttendanceModel.Attended.ToString().ToLower();
            if (postAttendanceModel.Attended)
            {
                attendee.arrivedAt = postAttendanceModel.InTime;
                attendee.leftAt = postAttendanceModel.OutTime;
                var arrivedDatetime = Convert.ToDateTime(attendee.arrivedAt).Add(new TimeSpan(0,15,0));
                var leftDatetime = Convert.ToDateTime(attendee.leftAt).Add(new TimeSpan(0, 15, 0));
                attendee.duration = leftDatetime.Subtract(arrivedDatetime).ToString();
                if (attendee.duration.Substring(0, 1).Equals("-") || attendee.arrivedAt == null || attendee.leftAt == null)
                {
                    return Json("NegativeTime");
                }

            }
            else
            {
                attendee.duration = "00:00:00";
                attendee.absencereason = postAttendanceModel.AbsentReason;
            }
            attendee.notes = postAttendanceModel.Notes;
            attendee.partyId = postAttendanceModel.PartyId;

            var postdata = await _eventService.MarkAttendance(postAttendanceModel.CourseNumber, postAttendanceModel.EventId, attendee);

            return Json(true);
        }

        public async Task<Account> GetUserAccount()
        {
            var access_token = Request.Cookies["access_token"];

            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{Request.Host.Value}")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);
            var user = await _httpClient.GetStringAsync("me");
            return JsonConvert.DeserializeObject<AccountRoot>(user).account;
        }
    }
}
