using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskSevice.Data;
using TaskSevice.Models;

namespace TaskSevice.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tasks = _context.Tasks.Where(t => !t.ParentTaskId.HasValue).ToList();
            return View(tasks);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var allTasks = _context.Tasks
                .Include(t => t.SubTasks)
                .ToList();

            var resultTask = allTasks.FirstOrDefault(t => t.Id == id);

            if (resultTask == null)
            {
                return NotFound();
            }

            return View(resultTask);
        }

        public IActionResult CreateSubTask(int? parentId)
        {
            if (parentId == null)
            {
                return NotFound();
            }

            var parentTask = _context.Tasks.Find(parentId);
            if (parentTask == null)
            {
                return NotFound();
            }

            ViewBag.ParentTaskId = parentId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSubTask(int parentId, Tasks subTask)
        {
            subTask.ParentTaskId = parentId;

            if (ModelState.IsValid)
            {
                _context.Tasks.Add(subTask);
                _context.SaveChanges();

                UpdateParentTasksEfforts(parentId);

                var rootTaskId = GetRootTaskId(parentId);

                return RedirectToAction("Edit", new { id = rootTaskId });
            }

            ViewBag.ParentTaskId = parentId;
            return View(subTask);
        }

        private int? GetRootTaskId(int? taskId)
        {
            if (taskId == null)
                return null;

            var task = _context.Tasks.Find(taskId);

            // Если у задачи нет родителя, это корневая задача
            if (task.ParentTaskId == null)
                return task.Id;

            // Иначе, ищем корневую задачу рекурсивно
            return GetRootTaskId(task.ParentTaskId);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Tasks task)
        {
            if (ModelState.IsValid)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskDetails(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return PartialView("TaskDetailsPartial", task);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _context.SaveChangesAsync();

            // Рекурсивное удаление задачи и всех её подзадач
            await DeleteTaskWithSubTasks(id);

            await _context.SaveChangesAsync();

            var task = await _context.Tasks.FindAsync(id);
            var parentId = task?.ParentTaskId; 

            if (parentId.HasValue)
            {
                return Json(new { redirectUrl = Url.Action("Edit", new { id = parentId.Value }) });
            }
            else
            {
                return Json(new { redirectUrl = Url.Action("Index") });
            }
        }

        private async Task DeleteTaskWithSubTasks(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return;
            }

            var subTasks = _context.Tasks.Where(t => t.ParentTaskId == taskId).ToList();
            foreach (var subTask in subTasks)
            {
                await DeleteTaskWithSubTasks(subTask.Id); // Рекурсивный вызов
            }

            _context.Tasks.Remove(task);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateTask([FromBody] Tasks updatedTask)
        {
            if (updatedTask == null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingTask = await _context.Tasks.FindAsync(updatedTask.Id);

                if (existingTask == null)
                {
                    return NotFound();
                }

                // Обновляем поля только если они предоставлены
                if (!string.IsNullOrEmpty(updatedTask.Title))
                    existingTask.Title = updatedTask.Title;

                if (!string.IsNullOrEmpty(updatedTask.Description))
                    existingTask.Description = updatedTask.Description;

                if (!string.IsNullOrEmpty(updatedTask.Executors))
                    existingTask.Executors = updatedTask.Executors;

                if (updatedTask.PlannedEffort != 0)
                    existingTask.PlannedEffort = updatedTask.PlannedEffort;

                if (updatedTask.ActualEffort != 0)
                    existingTask.ActualEffort = updatedTask.ActualEffort;

                _context.Update(existingTask);
                await _context.SaveChangesAsync();

                // Теперь расчет трудозатрат для родительской задачи
                if (existingTask.ParentTaskId.HasValue)
                {
                    UpdateParentTasksEfforts(existingTask.ParentTaskId.Value);
                }

                return Ok();
            }
            return BadRequest();
        }



        public async void UpdateParentTasksEfforts(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                CalculateTaskEfforts(task);
                _context.Update(task);
                await _context.SaveChangesAsync();

                if (task.ParentTaskId.HasValue)
                {
                    UpdateParentTasksEfforts(task.ParentTaskId.Value);
                }
            }
        }

        private void CalculateTaskEfforts(Tasks task)
        {
            double initialPlannedEffort = task.PlannedEffort;
            double initialActualEffort = task.ActualEffort;

            task.PlannedEffort = initialPlannedEffort + task.SubTasks.Sum(st => st.PlannedEffort);
            task.ActualEffort = initialActualEffort + task.SubTasks.Sum(st => st.ActualEffort);
        }

    }


}
