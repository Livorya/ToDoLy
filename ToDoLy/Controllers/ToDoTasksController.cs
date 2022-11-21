using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoLy.Data;
using ToDoLy.Models;

namespace ToDoLy.Controllers
{
    [Authorize]
    [Route("ToDo")]
    public class ToDoTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        // The account used for special privileges
        private readonly string _adminAccount = "admin@todoly.se";

        public ToDoTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ToDo
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.AdminAccount = _adminAccount;

            // Sorting parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["ProjectSortParm"] = sortOrder == "Project" ? "project_desc" : "Project";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            
            // Searching parameters
            ViewData["CurrentFilter"] = searchString;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            // The list of tasks to send to the View
            IQueryable<ToDoTask> toDoTasks;
            // This controller needs authorization to be used therefore User.Identity will not be null
            // The admin account is for testing purposes and gets access to all info
            if (User.Identity.Name == _adminAccount)
            {
                toDoTasks = _context.ToDoTasks;
            }
            else
            {
                toDoTasks = _context.ToDoTasks.Where(t => t.UserName == User.Identity.Name);
            }

            // Selection of the tasks based on search result
            if (!string.IsNullOrEmpty(searchString))
            {
                toDoTasks = toDoTasks.Where(t => t.Title.Contains(searchString) || t.Project.Contains(searchString));
            }

            // Order  of the list set by the sorting parameters
            switch (sortOrder)
            {
                case "title_desc":
                    toDoTasks = toDoTasks.OrderByDescending(t => t.Title);
                    break;
                case "Project":
                    toDoTasks = toDoTasks.OrderBy(t => t.Project).ThenBy(t => t.Title);
                    break;
                case "project_desc":
                    toDoTasks = toDoTasks.OrderByDescending(t => t.Project).ThenByDescending(t => t.Title);
                    break;
                case "Date":
                    toDoTasks = toDoTasks.OrderBy(t => t.DueDate.Date).ThenBy(t => t.Project).ThenBy(t => t.Title);
                    break;
                case "date_desc":
                    toDoTasks = toDoTasks.OrderByDescending(t => t.DueDate.Date).ThenByDescending(t => t.Project).ThenByDescending(t => t.Title);
                    break;
                default:
                    toDoTasks = toDoTasks.OrderBy(t => t.Title);
                    break;
            }

            int pageSize = 5;
            // Returns the tasks as a paginated list to provide paging
            return View(await PaginatedList<ToDoTask>.CreateAsync(toDoTasks.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: ToDo/Details/5
        [HttpGet("Details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            ViewBag.AdminAccount = _adminAccount;

            if (id == null || _context.ToDoTasks == null)
            {
                return NotFound();
            }

            var toDoTask = await _context.ToDoTasks.FirstOrDefaultAsync(m => m.Id == id);
            if (toDoTask == null)
            {
                return NotFound();
            }

            return View(toDoTask);
        }

        // GET: ToDo/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Creates a empty task and sets the start and due date to today to give to the View
            ToDoTask toDoTask = new ToDoTask();
            toDoTask.StartDate = DateTime.Today;
            toDoTask.DueDate = DateTime.Today; 
            return View(toDoTask);
        }

        // POST: ToDo/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Project,Status,StartDate,DueDate,Notes,UserName")] ToDoTask toDoTask)
        {
            if (ModelState.IsValid)
            {
                toDoTask.Id = Guid.NewGuid();
                // Sets the user name here
                toDoTask.UserName = User.Identity?.Name;
                _context.Add(toDoTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toDoTask);
        }

        // GET: ToDo/Edit/5
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ToDoTasks == null)
            {
                return NotFound();
            }

            var toDoTask = await _context.ToDoTasks.FindAsync(id);
            if (toDoTask == null)
            {
                return NotFound();
            }
            return View(toDoTask);
        }

        // POST: ToDo/Edit/5
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Project,Status,StartDate,DueDate,Notes,UserName")] ToDoTask toDoTask)
        {
            if (id != toDoTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Makes sure the user name is not lost since its made optional
                    toDoTask.UserName = User.Identity?.Name;
                    _context.Update(toDoTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ToDoTaskExists(toDoTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(toDoTask);
        }

        // GET: ToDo/Delete/5
        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ToDoTasks == null)
            {
                return NotFound();
            }

            var toDoTask = await _context.ToDoTasks.FirstOrDefaultAsync(m => m.Id == id);
            if (toDoTask == null)
            {
                return NotFound();
            }

            return View(toDoTask);
        }

        // POST: ToDo/Delete/5
        [HttpPost("Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ToDoTasks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ToDoTasks'  is null.");
            }
            var toDoTask = await _context.ToDoTasks.FindAsync(id);
            if (toDoTask != null)
            {
                _context.ToDoTasks.Remove(toDoTask);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ToDoTaskExists(Guid id)
        {
          return _context.ToDoTasks.Any(e => e.Id == id);
        }
    }
}
