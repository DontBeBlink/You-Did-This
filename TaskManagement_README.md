# Task Management System - Agenda Feature

A focused scheduling UI that provides Day/Week views, quick reschedule actions, and improved date parsing for task management.

## Features

### Backend API
- **GET /tasks** - Retrieve tasks with optional filtering by date range, status, and priority
- **PATCH /tasks/{id}** - Update task details  
- **PATCH /tasks/{id}/reschedule** - Quick reschedule actions (+1h, +1d, next week, pick date)
- **PATCH /tasks/{id}/done** - Mark task as completed
- **POST /tasks/parse-date** - Parse natural language date strings

### Frontend Interface
- **Day View** - Tasks grouped by hour for detailed daily planning
- **Week View** - Tasks grouped by day for weekly overview
- **Quick Actions** - One-click buttons for Done, +1h, +1d, Next week, Pick date
- **Natural Date Input** - Support for phrases like "Mon 9a", "in 45m", "tomorrow evening"

### Date Parser
Supports common scheduling phrases:
- **Day + Time**: "Mon 9a", "Tuesday 2:30pm", "Wed 10am"
- **Relative Time**: "in 45m", "in 2h", "in 3d"
- **Relative Days**: "tomorrow", "tomorrow morning", "tomorrow evening"
- **Specific Times**: "9am", "2:30pm", "14:30"

## Installation

### Prerequisites
- Python 3.8+
- Modern web browser

### Quick Start

1. **Install Dependencies**
   ```bash
   cd server
   pip install -r requirements.txt
   ```

2. **Start the Server**
   ```bash
   # Using the startup script
   ./start.sh
   
   # Or manually
   python main.py
   ```

3. **Access the Interface**
   Open http://localhost:8000 in your browser

## Usage

### Adding Tasks
1. Enter a task title in the "What needs to be done?" field
2. Specify a due date using natural language (e.g., "Mon 9a", "in 2h", "tomorrow evening")
3. Select priority level
4. Click "Add Task"

### Managing Tasks
- **Mark Done**: Click the "Done" button
- **Reschedule**: Use quick action buttons (+1h, +1d, Next week, Pick date)
- **View Options**: Switch between Day and Week views
- **Navigate**: Use Previous/Next buttons or "Today" to navigate dates

### Date Input Examples
- `Mon 9a` - Next Monday at 9 AM
- `in 45m` - 45 minutes from now
- `tomorrow evening` - Tomorrow at 6 PM
- `2pm` - Today at 2 PM (or tomorrow if past)
- `next week` - Next Monday at 9 AM

## API Endpoints

### Tasks
- `GET /tasks?start_date=2024-01-01&end_date=2024-01-07&status=pending&priority=high`
- `POST /tasks` - Create new task
- `PATCH /tasks/{id}` - Update task
- `DELETE /tasks/{id}` - Delete task

### Quick Actions
- `PATCH /tasks/{id}/reschedule` - Body: `{"action": "+1h"}` or `{"action": "pick_date", "new_date": "2024-01-15T09:00:00"}`
- `PATCH /tasks/{id}/done` - Mark as completed

### Utilities
- `POST /tasks/parse-date` - Body: `"tomorrow 9am"` - Returns parsed datetime
- `GET /health` - Server health check

## Data Storage

Tasks are stored in a JSON file (`tasks.json`) for persistence. The storage system supports:
- Automatic file creation and loading
- Atomic writes to prevent data corruption
- Task filtering and sorting
- Date range queries

## Error Handling

The system provides clear error messages for:
- Invalid date formats with suggested alternatives
- Network connectivity issues
- Server errors with user-friendly descriptions
- Date parsing failures with format hints

## Development

### File Structure
```
server/
├── main.py           # FastAPI server and endpoints
├── storage.py        # Task storage and persistence
├── schemas.py        # Pydantic data models
├── date_parser.py    # Natural language date parsing
├── requirements.txt  # Python dependencies
└── start.sh         # Startup script

web/
├── agenda.html      # Main interface
└── js/
    └── api.js       # Frontend API client
```

### Architecture
- **Backend**: FastAPI with Pydantic for data validation
- **Frontend**: Vanilla JavaScript with modern async/await patterns
- **Storage**: JSON file-based persistence with in-memory caching
- **Date Parsing**: Regex-based natural language processing

This implementation provides a complete task management solution with intuitive scheduling capabilities and a modern, responsive interface.