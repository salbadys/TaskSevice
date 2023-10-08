using TaskSevice.EnumTask;

namespace TaskSevice.Service
{
    public class GetStatus
    {
        public static string GetInRussian(TasksStatus status)
        {
            return status switch
            {
                TasksStatus.Assigned => "Назначена",
                TasksStatus.InProgress => "Выполняется",
                TasksStatus.Paused => "Приостановлена",
                TasksStatus.Completed => "Завершена",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
