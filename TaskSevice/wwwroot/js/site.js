
    function toggleSubTasks(event) {
        const taskElement = event.target.closest('li'); // находим ближайший элемент li от места клика
    if (!taskElement) return;

    const subTasksList = taskElement.querySelector('ul');

    if (subTasksList) {
            if (subTasksList.style.display === "none") {
        subTasksList.style.display = "block";
            } else {
        subTasksList.style.display = "none";
            }
        }

    const taskId = taskElement.getAttribute('data-task-id');
    if (taskId) {
        loadTaskDetails(taskId);
        }
    }

    function loadTaskDetails(taskId) {
        // Используем fetch для выполнения AJAX-запроса
        fetch(`/Tasks/GetTaskDetails?id=${taskId}`)
            .then(response => {
                // Проверяем, успешно ли выполнен запрос
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.text(); // Предполагаем, что ответ сервера - это HTML
            })
            .then(html => {
                // Вставляем HTML с деталями задачи в блок .task-details
                document.querySelector('.task-details').innerHTML = html;
            })
            .catch(error => {
                console.log('There was a problem with the fetch operation:', error.message);
                document.querySelector('.task-details').innerHTML = 'Error loading task details.';
            });
    }

    function deleteTask(taskId) {
        if (!confirm('Are you sure you want to delete this task?')) {
            return;
        }

    fetch(`/Tasks/DeleteTask?id=${taskId}`, {
        method: 'DELETE',
        })
            .then(response => response.json())  // Обработать ответ как JSON
            .then(data => {
                if (data && data.redirectUrl) {
        window.location.href = data.redirectUrl;  // Переадресовать на URL, полученный от сервера
                }
            })
            .catch(error => {
        console.log('There was a problem with the delete operation:', error.message);
            });
    }

    function submitTaskChanges(taskId) {
        const title = document.getElementById('taskTitle').value;
    const description = document.getElementById('taskDescription').value;
    const executors = document.getElementById('taskExecutors').value;
    const plannedEffort = document.getElementById('taskPlannedEffort').value;
    const actualEffort = document.getElementById('taskActualEffort').value;

    const data = {
        Id: taskId,
    Title: title,
    Description: description,
    Executors: executors,
    PlannedEffort: plannedEffort,
    ActualEffort: actualEffort
        };

    fetch(`/Tasks/UpdateTask`, {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json'
            },
    body: JSON.stringify(data)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
    showNotification('Задание успешно обновлено!');
            })
            .catch(error => {
        console.log('There was a problem with the fetch operation:', error.message);
            });
    }

    function showNotification(message) {
        const notification = document.getElementById('notification');
    notification.textContent = message;
    notification.style.display = 'block';
    notification.style.opacity = 1;
    notification.style.bottom = '50px';

        setTimeout(() => {
        notification.style.opacity = 0;
    notification.style.bottom = '20px';
        }, 3000);  // Скрываем уведомление после 3 секунд
    }

    function createSubTask(parentId) {
        window.location.href = `/Tasks/CreateSubTask?parentId=${parentId}`;
    }

    document.addEventListener("DOMContentLoaded", function () {
        const firstTask = document.getElementById("firstTask");
    if (firstTask) {
            const taskId = firstTask.getAttribute("data-task-id");
    loadTaskDetails(taskId);

    // Открываем подзадачи первой задачи, если они есть
    const subTasksList = firstTask.querySelector('ul');
    if (subTasksList) {
        subTasksList.style.display = "block";
            }
        }
    });