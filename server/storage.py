"""
Storage layer for the task management system.
Simple in-memory storage with optional JSON persistence.
"""
import json
import os
from datetime import datetime
from typing import Dict, List, Optional
from schemas import Task, TaskCreate, TaskUpdate, TaskStatus, TaskPriority, TaskQuery


class TaskStorage:
    """Simple in-memory task storage with JSON persistence."""
    
    def __init__(self, storage_file: str = "tasks.json"):
        """Initialize storage with optional file persistence."""
        self.storage_file = storage_file
        self.tasks: Dict[int, Task] = {}
        self.next_id = 1
        self.load_from_file()
    
    def _make_naive(self, dt: datetime) -> datetime:
        """Convert timezone-aware datetime to naive datetime for comparison."""
        if dt.tzinfo is not None:
            return dt.replace(tzinfo=None)
        return dt
    
    def load_from_file(self) -> None:
        """Load tasks from JSON file if it exists."""
        if os.path.exists(self.storage_file):
            try:
                with open(self.storage_file, 'r') as f:
                    data = json.load(f)
                    for task_data in data.get('tasks', []):
                        # Convert string dates back to datetime objects
                        if task_data.get('due_date'):
                            task_data['due_date'] = datetime.fromisoformat(task_data['due_date'])
                        if task_data.get('created_at'):
                            task_data['created_at'] = datetime.fromisoformat(task_data['created_at'])
                        if task_data.get('updated_at'):
                            task_data['updated_at'] = datetime.fromisoformat(task_data['updated_at'])
                        
                        task = Task(**task_data)
                        self.tasks[task.id] = task
                        if task.id >= self.next_id:
                            self.next_id = task.id + 1
            except (json.JSONDecodeError, KeyError, ValueError) as e:
                print(f"Error loading tasks from file: {e}")
    
    def save_to_file(self) -> None:
        """Save tasks to JSON file."""
        try:
            data = {
                'tasks': []
            }
            for task in self.tasks.values():
                task_dict = task.dict()
                # Convert datetime objects to ISO strings for JSON serialization
                if task_dict.get('due_date'):
                    task_dict['due_date'] = task_dict['due_date'].isoformat()
                if task_dict.get('created_at'):
                    task_dict['created_at'] = task_dict['created_at'].isoformat()
                if task_dict.get('updated_at'):
                    task_dict['updated_at'] = task_dict['updated_at'].isoformat()
                data['tasks'].append(task_dict)
            
            with open(self.storage_file, 'w') as f:
                json.dump(data, f, indent=2)
        except Exception as e:
            print(f"Error saving tasks to file: {e}")
    
    def create_task(self, task_data: TaskCreate) -> Task:
        """Create a new task."""
        now = datetime.now()
        task = Task(
            id=self.next_id,
            created_at=now,
            updated_at=now,
            **task_data.dict()
        )
        self.tasks[self.next_id] = task
        self.next_id += 1
        self.save_to_file()
        return task
    
    def get_task(self, task_id: int) -> Optional[Task]:
        """Get a task by ID."""
        return self.tasks.get(task_id)
    
    def update_task(self, task_id: int, task_update: TaskUpdate) -> Optional[Task]:
        """Update a task."""
        if task_id not in self.tasks:
            return None
        
        task = self.tasks[task_id]
        update_data = task_update.dict(exclude_unset=True)
        
        for field, value in update_data.items():
            setattr(task, field, value)
        
        task.updated_at = datetime.now()
        self.save_to_file()
        return task
    
    def delete_task(self, task_id: int) -> bool:
        """Delete a task."""
        if task_id in self.tasks:
            del self.tasks[task_id]
            self.save_to_file()
            return True
        return False
    
    def get_tasks(self, query: TaskQuery) -> List[Task]:
        """Get tasks with optional filtering."""
        tasks = list(self.tasks.values())
        
        # Filter by date range
        if query.start_date:
            tasks = [t for t in tasks if t.due_date and self._make_naive(t.due_date) >= self._make_naive(query.start_date)]
        if query.end_date:
            tasks = [t for t in tasks if t.due_date and self._make_naive(t.due_date) <= self._make_naive(query.end_date)]
        
        # Filter by status
        if query.status:
            tasks = [t for t in tasks if t.status == query.status]
        
        # Filter by priority
        if query.priority:
            tasks = [t for t in tasks if t.priority == query.priority]
        
        # Sort by due date (nulls last) then by priority
        def sort_key(task):
            priority_order = {"urgent": 0, "high": 1, "medium": 2, "low": 3}
            due_date = self._make_naive(task.due_date) if task.due_date else datetime.max
            priority = priority_order.get(task.priority.value, 4)
            return (due_date, priority)
        
        tasks.sort(key=sort_key)
        return tasks
    
    def reschedule_task(self, task_id: int, action: str, new_date: Optional[datetime] = None) -> Optional[Task]:
        """Reschedule a task with predefined actions."""
        if task_id not in self.tasks:
            return None
        
        task = self.tasks[task_id]
        current_date = task.due_date or datetime.now()
        
        if action == "+1h":
            from datetime import timedelta
            task.due_date = current_date + timedelta(hours=1)
        elif action == "+1d":
            from datetime import timedelta
            task.due_date = current_date + timedelta(days=1)
        elif action == "next_week":
            from datetime import timedelta
            # Move to next Monday
            days_until_monday = (7 - current_date.weekday()) % 7
            if days_until_monday == 0:  # If it's already Monday, go to next Monday
                days_until_monday = 7
            task.due_date = current_date + timedelta(days=days_until_monday)
        elif action == "pick_date" and new_date:
            task.due_date = new_date
        else:
            return None
        
        task.updated_at = datetime.now()
        self.save_to_file()
        return task
    
    def mark_done(self, task_id: int) -> Optional[Task]:
        """Mark a task as done."""
        if task_id not in self.tasks:
            return None
        
        task = self.tasks[task_id]
        task.status = TaskStatus.DONE
        task.updated_at = datetime.now()
        self.save_to_file()
        return task


# Global storage instance
storage = TaskStorage()