"""
FastAPI server for the task management system.
Provides REST API endpoints for task CRUD operations and agenda functionality.
"""
from datetime import datetime
from typing import List, Optional
from fastapi import FastAPI, HTTPException, Query
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
from pydantic import ValidationError

from schemas import Task, TaskCreate, TaskUpdate, TaskQuery, TaskReschedule, TaskStatus, TaskPriority, DateParseRequest
from storage import storage
from date_parser import parse_date_string


app = FastAPI(
    title="Task Management API",
    description="Focused scheduling API for agenda functionality",
    version="1.0.0"
)

# Serve static files (web directory)
app.mount("/static", StaticFiles(directory="../web"), name="static")


@app.get("/")
async def root():
    """Serve the main agenda page."""
    return FileResponse("../web/agenda.html")


@app.get("/tasks", response_model=List[Task])
async def get_tasks(
    start_date: Optional[str] = Query(None, description="Start date (ISO format)"),
    end_date: Optional[str] = Query(None, description="End date (ISO format)"),
    status: Optional[TaskStatus] = Query(None, description="Filter by status"),
    priority: Optional[TaskPriority] = Query(None, description="Filter by priority")
):
    """Get tasks with optional filtering by date range, status, and priority."""
    try:
        # Parse date strings
        start_dt = None
        end_dt = None
        
        if start_date:
            try:
                start_dt = datetime.fromisoformat(start_date.replace('Z', '+00:00'))
            except ValueError:
                raise HTTPException(status_code=400, detail=f"Invalid start_date format: {start_date}")
        
        if end_date:
            try:
                end_dt = datetime.fromisoformat(end_date.replace('Z', '+00:00'))
            except ValueError:
                raise HTTPException(status_code=400, detail=f"Invalid end_date format: {end_date}")
        
        query = TaskQuery(
            start_date=start_dt,
            end_date=end_dt,
            status=status,
            priority=priority
        )
        
        return storage.get_tasks(query)
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/tasks", response_model=Task)
async def create_task(task_data: TaskCreate):
    """Create a new task."""
    try:
        return storage.create_task(task_data)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/tasks/{task_id}", response_model=Task)
async def get_task(task_id: int):
    """Get a specific task by ID."""
    task = storage.get_task(task_id)
    if not task:
        raise HTTPException(status_code=404, detail="Task not found")
    return task


@app.patch("/tasks/{task_id}", response_model=Task)
async def update_task(task_id: int, task_update: TaskUpdate):
    """Update a task."""
    task = storage.update_task(task_id, task_update)
    if not task:
        raise HTTPException(status_code=404, detail="Task not found")
    return task


@app.patch("/tasks/{task_id}/reschedule", response_model=Task)
async def reschedule_task(task_id: int, reschedule_data: TaskReschedule):
    """Reschedule a task with quick actions."""
    task = storage.reschedule_task(
        task_id, 
        reschedule_data.action, 
        reschedule_data.new_date
    )
    if not task:
        raise HTTPException(status_code=404, detail="Task not found or invalid action")
    return task


@app.patch("/tasks/{task_id}/done", response_model=Task)
async def mark_task_done(task_id: int):
    """Mark a task as done."""
    task = storage.mark_done(task_id)
    if not task:
        raise HTTPException(status_code=404, detail="Task not found")
    return task


@app.delete("/tasks/{task_id}")
async def delete_task(task_id: int):
    """Delete a task."""
    success = storage.delete_task(task_id)
    if not success:
        raise HTTPException(status_code=404, detail="Task not found")
    return {"message": "Task deleted successfully"}


@app.post("/tasks/parse-date")
async def parse_date(request: DateParseRequest):
    """Parse a natural language date string and return the parsed datetime."""
    try:
        parsed_date = parse_date_string(request.date_string)
        if parsed_date is None:
            raise HTTPException(
                status_code=400, 
                detail=f"Could not parse date string: '{request.date_string}'. Try formats like 'Mon 9a', 'in 45m', 'tomorrow evening'"
            )
        return {
            "original": request.date_string,
            "parsed": parsed_date.isoformat(),
            "success": True
        }
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.get("/health")
async def health_check():
    """Health check endpoint."""
    return {"status": "healthy", "timestamp": datetime.now().isoformat()}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)