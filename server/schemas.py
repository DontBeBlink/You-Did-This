"""
Data schemas for the Memories API
"""
from datetime import datetime
from typing import List, Optional
from pydantic import BaseModel


class Memory(BaseModel):
    """Memory data model"""
    id: Optional[int] = None
    title: str
    content: str
    tags: List[str] = []
    people: List[str] = []
    created_at: Optional[datetime] = None
    updated_at: Optional[datetime] = None


class MemoryUpdate(BaseModel):
    """Memory update model for PATCH operations"""
    title: Optional[str] = None
    content: Optional[str] = None
    tags: Optional[List[str]] = None
    people: Optional[List[str]] = None


class MemoriesResponse(BaseModel):
    """Response model for paginated memories"""
    items: List[Memory]
    total: int
    limit: int
    offset: int