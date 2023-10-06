using Assignment_2.Areas.Identity.Data;
using Assignment_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Diagnostics;
using System.Net.Sockets;

namespace Assignment_2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly TaskManagementSystemContext _context;

        private readonly UserManager<SystemUser> _userManager;

        public HomeController(ILogger<HomeController> logger, TaskManagementSystemContext context, UserManager<SystemUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            SystemUser user = _context.Users
                .Include(u => u.ProjectUsers)
                .ThenInclude(pu => pu.Project)
                .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            List<ProjectUser> projectUsers = _context.ProjectUsers
                .OrderBy(pu => pu.Project.ProjectTitle)
                .Where(pu => pu.SystemUser == user)
                .ToList();

            return View(projectUsers.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> CreateProject(string id)
        {
            string developer = "Developer";

            if (id == null)
            {
                return NotFound();
            }

            SystemUser user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (id != user.Id || user == null)
            {
                return NotFound();
            }

            ProjectUser projectUser = new ProjectUser();
            projectUser.Id = Guid.NewGuid();
            projectUser.ProjectId = Guid.NewGuid();
            projectUser.SystemUserId = user.Id;

            ViewData["SystemUserId"] = new SelectList(await _userManager.GetUsersInRoleAsync(developer), "Id", "UserName");

            return View(projectUser);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateProject([Bind("Id, ProjectTitle")] Project project, [Bind("Id, ProjectId, SystemUserId")] ProjectUser projectUser, List<string> developers)
        {
            string developer = "Developer";

            if (ModelState.IsValid)
            {
                _context.Projects.Add(project);
                _context.SaveChanges();

                _context.ProjectUsers.Add(projectUser);
                _context.SaveChanges();

                foreach (string developerId in developers)
                {
                    ProjectUser newProjectUser = new ProjectUser();
                    newProjectUser.Id = Guid.NewGuid();
                    newProjectUser.ProjectId = project.Id;
                    newProjectUser.SystemUserId = developerId;
                    
                    _context.ProjectUsers.Add(newProjectUser);
                    _context.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["SystemUserId"] = new SelectList(await _userManager.GetUsersInRoleAsync(developer), "Id", "UserName");

            return View(projectUser);
        }

        public async Task<IActionResult> ProjectDetails(int? page, Guid id, bool isComplete = false)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (id == null)
            {
                return NotFound();
            }

            SystemUser user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            Project project = _context.Projects
                .Include(p => p.Tickets.Where(t => t.IsComplete == isComplete))
                .ThenInclude(t => t.TicketUsers)
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.SystemUser)
                .FirstOrDefault(p => p.Id == id);

            // Instead of Ticket, return TicketUser

            IPagedList<Ticket> tickets = project.Tickets
                .OrderBy(t => t.TicketTitle)
                .Where(t => t.TicketUsers.Any(tu => tu.SystemUser == user))
                .ToPagedList(pageNumber, pageSize);

            if (id != project.Id || project == null)
            {
                return NotFound();
            }

            TicketVM ticketVM = new TicketVM
            {
                Project = project,
                Tickets = tickets
            };

            return View(ticketVM);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        public IActionResult DeleteProject(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project project = _context.Projects
                .Include(p => p.ProjectUsers)
                .Include(p => p.Tickets)
                .FirstOrDefault(p => p.Id == id);

            if (id != project.Id || project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public IActionResult DeleteProject([Bind("Id, ProjectTitle")] Project project)
        {
            List<ProjectUser> projectUsers = _context.ProjectUsers.Where(pu => pu.Project == project).ToList();
            List<Ticket> tickets = _context.Tickets.Where(t => t.Project == project).ToList();

            List<Guid> ticketIds = tickets.Select(t => t.Id).ToList();
            List<TicketUser> ticketUsers = _context.TicketUsers.Where(tu => ticketIds.Contains((Guid)tu.TicketId)).ToList();

            if (ModelState.IsValid)
            {
                _context.ProjectUsers.RemoveRange(projectUsers);
                _context.TicketUsers.RemoveRange(ticketUsers);
                _context.Tickets.RemoveRange(tickets);
                _context.SaveChanges();

                _context.Projects.Remove(project);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> AddDeveloperToProject(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string developer = "Developer";

            ProjectUser projectUser = new ProjectUser();
            projectUser.Id = Guid.NewGuid();
            projectUser.ProjectId = id;

            IList<SystemUser> developers = await _userManager.GetUsersInRoleAsync(developer);

            //ViewData["SystemUserId"] = new SelectList(developers.OrderBy(d => d.UserName), "Id", "UserName", projectUser.SystemUserId);
            ViewBag.Developers = new SelectList(developers.OrderBy(d => d.UserName), "Id", "UserName", projectUser.SystemUserId);

            return View(projectUser);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public IActionResult AddDeveloperToProject([Bind("Id, ProjectId, SystemUserId")] ProjectUser projectUser)
        {
            if (ModelState.IsValid)
            {
                _context.ProjectUsers.Add(projectUser);
                _context.SaveChanges();

                return RedirectToAction(nameof(ProjectDetails), new { id = projectUser.ProjectId });
            }

            return View(projectUser);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        public IActionResult CreateTicket(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project project = _context.Projects.FirstOrDefault(p => p.Id == id);

            if (id != project.Id || project == null)
            {
                return NotFound();
            }

            Ticket ticket = new Ticket();
            ticket.Id = Guid.NewGuid();
            ticket.ProjectId = project.Id;
            ticket.IsComplete = false;

            return View(ticket);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public IActionResult CreateTicket([Bind("Id, TicketTitle, Priority,  RequiredHours, IsComplete, ProjectId")] Ticket ticket)
        {
            SystemUser user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            TicketUser ticketUser = new TicketUser();
            ticketUser.Id = Guid.NewGuid();
            ticketUser.TicketId = ticket.Id;
            ticketUser.SystemUser = user;
            ticketUser.SystemUserId = user.Id;

            if (ModelState.IsValid)
            {
                _context.Tickets.Add(ticket);
                _context.SaveChanges();

                _context.TicketUsers.Add(ticketUser);
                _context.SaveChanges();

                return RedirectToAction(nameof(ProjectDetails), new { id = ticket.ProjectId });
            }

            return View(ticket);
        }

        public async Task<IActionResult> TicketDetails(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            List<TicketUser> developers = new List<TicketUser>();

            Ticket ticket = _context.Tickets
                .Include(t => t.TicketUsers)
                .ThenInclude(tu => tu.SystemUser)
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == id);
           
            if(id != ticket.Id || ticket == null)
            {
                return NotFound();
            }

            foreach (TicketUser developer in ticket.TicketUsers)
            {
                if (await _userManager.IsInRoleAsync(developer.SystemUser, "Developer"))
                {
                    developers.Add(developer);
                }
            }

            ViewBag.Developers = developers;

           return View(ticket);
        }

        public IActionResult EditTicket(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            if (id != ticket.Id || ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        [HttpPost]
        public IActionResult EditTicket([Bind("Id, TicketTitle, Priority, RequiredHours, IsComplete, ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Update(ticket);
                _context.SaveChanges();

                return RedirectToAction(nameof(TicketDetails), new { id = ticket.Id });
            }

            return View(ticket);
        }

        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public IActionResult DeleteTicket(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = _context.Tickets
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == id);

            if (id != ticket.Id || ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public IActionResult DeleteTicket([Bind("Id, TicketTitle, Priority, RequiredHours, IsComplete, ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Tickets.Remove(ticket);
                _context.SaveChanges();

                return RedirectToAction(nameof(ProjectDetails), new { id = ticket.ProjectId });
            }

            return View(ticket);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> AddDeveloperToTicket(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = _context.Tickets
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == id);

            if (id != ticket.Id || ticket == null)
            {
                return NotFound();
            }

            TicketUser ticketUser = new TicketUser();
            ticketUser.Id = Guid.NewGuid();
            ticketUser.TicketId = ticket.Id;
            ticketUser.Ticket = ticket;

            string developer = "Developer";

            List<ProjectUser> projectUsers = _context.ProjectUsers
                .Include(pu => pu.SystemUser)
                .Where(pu => pu.ProjectId == ticket.ProjectId)
                .ToList();

            List<SystemUser> users = projectUsers.Select(pu => pu.SystemUser).OrderBy(u => u.UserName).ToList();
            List<SystemUser> developers = new List<SystemUser>();

            foreach (SystemUser user in users)
            {
                if (await _userManager.IsInRoleAsync(user, developer))
                {
                    developers.Add(user);
                }
            }

            ViewBag.Developers = new SelectList(developers, "Id", "UserName");

            return View(ticketUser);
        }

        [Authorize(Roles = "Administrator, Project Manager")]
        [HttpPost]
        public async Task<IActionResult> AddDeveloperToTicket([Bind("Id, TicketId, SystemUserId")] TicketUser ticketUser)
        {
            string developer = "Developer";

            Ticket ticket = _context.Tickets.FirstOrDefault(t => t.Id == ticketUser.TicketId);

            List<ProjectUser> projectUsers = _context.ProjectUsers
                .Include(pu => pu.SystemUser)
                .Where(pu => pu.ProjectId == ticket.ProjectId)
                .ToList();

            List<SystemUser> developers = projectUsers.Select(pu => pu.SystemUser).OrderBy(u => u.UserName).ToList();

            if (ModelState.IsValid)
            {
                if (_context.TicketUsers.Any(tu => tu.TicketId == ticketUser.TicketId && tu.SystemUserId == ticketUser.SystemUserId))
                {
                    ViewBag.Developers = new SelectList(developers, "Id", "UserName");

                    ModelState.AddModelError("", "This developer is already associated with the ticket.");

                    return View(ticketUser);
                }

                _context.TicketUsers.Add(ticketUser);
                _context.SaveChanges();

                return RedirectToAction(nameof(TicketDetails), new { id = ticketUser.TicketId });
            }

            ViewBag.Developers = new SelectList(developers, "Id", "UserName");

            return View(ticketUser);
        }

        [HttpPost]
        public IActionResult CompleteTicket([Bind("Id, TicketTitle, Priority, RequiredHours, IsComplete, ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.IsComplete = !ticket.IsComplete;

                _context.Update(ticket);
                _context.SaveChanges();

                return RedirectToAction(nameof(ProjectDetails), new { id = ticket.ProjectId, isComplete = !ticket.IsComplete });
            }

            return RedirectToAction(nameof(ProjectDetails), new { id = ticket.ProjectId, isComplete = !ticket.IsComplete });

        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Users()
        {
            List<SystemUser> users = _context.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Developer([Bind("Id")] SystemUser user)
        {
            string developer = "Developer";
            string projectManager = "Project Manager";

            bool isDeveloper = await _userManager.IsInRoleAsync(user, developer);
            bool isProjectManager = await _userManager.IsInRoleAsync(user, projectManager);

            if (user != null && isProjectManager)
            {
                await _userManager.RemoveFromRoleAsync(user, projectManager);
                await _userManager.AddToRoleAsync(user, developer);

                _context.SaveChanges();
            }
            else if (user != null && !isDeveloper)
            {
                await _userManager.AddToRoleAsync(user, developer);

                _context.SaveChanges();
            }
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> ProjectManager([Bind("Id")] SystemUser user)
        {
            string developer = "Developer";
            string projectManager = "Project Manager";

            bool isDeveloper = await _userManager.IsInRoleAsync(user, developer);
            bool isProjectManager = await _userManager.IsInRoleAsync(user, projectManager);

            if (user != null && isDeveloper)
            {
                await _userManager.RemoveFromRoleAsync(user, developer);
                await _userManager.AddToRoleAsync(user, projectManager);

                _context.SaveChanges();
            }
            else if (user != null && !isProjectManager)
            {
                await _userManager.AddToRoleAsync(user, projectManager);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
