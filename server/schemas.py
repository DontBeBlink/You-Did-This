"""
Data schemas for the task management system.
"""
from datetime import datetime
from enum import Enum
from typing import Optional
from pydantic import BaseModel, Field


class TaskStatus(str, Enum):
    """Task status enumeration."""
    PENDING = "pending"
    DONE = "done"
    SNOOZED = "snoozed"


class TaskPriority(str, Enum):
    """Task priority enumeration."""
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"
    URGENT = "urgent"


class TaskBase(BaseModel):
    """Base task model."""
    title: str = Field(..., description="Task title")
    description: Optional[str] = Field(None, description="Task description")
    due_date: Optional[datetime] = Field(None, description="Task due date")
    status: TaskStatus = Field(TaskStatus.PENDING, description="Task status")
    priority: TaskPriority = Field(TaskPriority.MEDIUM, description="Task priority")


class TaskCreate(TaskBase):
    """Task creation model."""
    pass


class TaskUpdate(BaseModel):
    """Task update model."""
    title: Optional[str] = None
    description: Optional[str] = None
    due_date: Optional[datetime] = None
    status: Optional[TaskStatus] = None
    priority: Optional[TaskPriority] = None


class Task(TaskBase):
    """Complete task model with ID and timestamps."""
    id: int = Field(..., description="Task ID")
    created_at: datetime = Field(..., description="Task creation timestamp")
    updated_at: datetime = Field(..., description="Task last update timestamp")

    class Config:
        from_attributes = True


class TaskReschedule(BaseModel):
    """Task reschedule operation model."""
    action: str = Field(..., description="Reschedule action: '+1h', '+1d', 'next_week', or 'pick_date'")
    new_date: Optional[datetime] = Field(None, description="New date for 'pick_date' action")


class TaskQuery(BaseModel):
    """Task query parameters."""
    start_date: Optional[datetime] = Field(None, description="Filter tasks from this date")
    end_date: Optional[datetime] = Field(None, description="Filter tasks until this date")
    status: Optional[TaskStatus] = Field(None, description="Filter by task status")
    priority: Optional[TaskPriority] = Field(None, description="Filter by task priority")


class DateParseRequest(BaseModel):
    """Date parse request model."""
    date_string: str = Field(..., description="Natural language date string to parse")