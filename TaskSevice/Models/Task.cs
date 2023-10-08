using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskSevice.EnumTask.TasksStatus;

namespace TaskSevice.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string Executors { get; set; } // Список исполнителей

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public TaskStatus Status { get; set; } = TaskStatus.Assigned;

        public double PlannedEffort { get; set; } //плановая трудоемкость

        public double ActualEffort { get; set; } //факт.времени выполнения

        public DateTime? CompletionDate { get; set; } //дата завершения

        public int? ParentTaskId { get; set; } // Null для основных задач

        public Tasks? ParentTask { get; set; }

        public List<Tasks> SubTasks { get; set; } = new List<Tasks>();
    }
}
