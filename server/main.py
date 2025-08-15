"""
Main FastAPI server for the Memories system
"""
from typing import List, Optional
from fastapi import FastAPI, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
import uvicorn

from .schemas import Memory, MemoryUpdate, MemoriesResponse
from .storage import MemoryStorage

# Initialize FastAPI app
app = FastAPI(
    title="Memories API",
    description="API for managing memories with search, filtering, and pagination",
    version="1.0.0"
)

# Enable CORS for web frontend
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify exact origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize storage
storage = MemoryStorage()

# Mount static files for web frontend
app.mount("/static", StaticFiles(directory="web"), name="static")


@app.get("/", response_class=FileResponse)
async def read_index():
    """Serve the main memories page"""
    return FileResponse("web/memories.html")


@app.get("/memories", response_model=MemoriesResponse)
async def get_memories(
    q: Optional[str] = Query(None, description="Search query"),
    tags: Optional[str] = Query(None, description="Comma-separated tags"),
    people: Optional[str] = Query(None, description="Comma-separated people"),
    limit: int = Query(10, ge=1, le=100, description="Number of items per page"),
    offset: int = Query(0, ge=0, description="Offset for pagination")
):
    """Get memories with optional filtering and pagination"""
    # Parse comma-separated values
    tags_list = [tag.strip() for tag in tags.split(",")] if tags else None
    people_list = [person.strip() for person in people.split(",")] if people else None
    
    memories, total = storage.get_memories(q, tags_list, people_list, limit, offset)
    
    return MemoriesResponse(
        items=memories,
        total=total,
        limit=limit,
        offset=offset
    )


@app.get("/memories/{memory_id}", response_model=Memory)
async def get_memory(memory_id: int):
    """Get a specific memory by ID"""
    memory = storage.get_memory(memory_id)
    if not memory:
        raise HTTPException(status_code=404, detail="Memory not found")
    return memory


@app.post("/memories", response_model=Memory, status_code=201)
async def create_memory(memory: Memory):
    """Create a new memory"""
    return storage.create_memory(memory)


@app.patch("/memories/{memory_id}", response_model=Memory)
async def update_memory(memory_id: int, updates: MemoryUpdate):
    """Update an existing memory"""
    update_dict = updates.dict(exclude_unset=True)
    memory = storage.update_memory(memory_id, update_dict)
    if not memory:
        raise HTTPException(status_code=404, detail="Memory not found")
    return memory


@app.delete("/memories/{memory_id}")
async def delete_memory(memory_id: int):
    """Delete a specific memory"""
    success = storage.delete_memory(memory_id)
    if not success:
        raise HTTPException(status_code=404, detail="Memory not found")
    return {"message": "Memory deleted successfully"}


@app.delete("/memories")
async def delete_memories(memory_ids: List[int]):
    """Delete multiple memories"""
    deleted_count = storage.delete_memories(memory_ids)
    return {"message": f"Deleted {deleted_count} memories"}


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "memories-api"}


if __name__ == "__main__":
    uvicorn.run("server.main:app", host="0.0.0.0", port=8000, reload=True)