"""
Storage layer for memories data
"""
import json
import os
from datetime import datetime
from typing import List, Optional, Dict, Any
from .schemas import Memory


class MemoryStorage:
    """Simple JSON file-based storage for memories"""
    
    def __init__(self, storage_file: str = "memories.json"):
        self.storage_file = storage_file
        self._memories: Dict[int, Memory] = {}
        self._next_id = 1
        self._load_from_file()
    
    def _load_from_file(self):
        """Load memories from JSON file"""
        if os.path.exists(self.storage_file):
            try:
                with open(self.storage_file, 'r') as f:
                    data = json.load(f)
                    for item in data.get('memories', []):
                        memory = Memory(**item)
                        if memory.id:
                            self._memories[memory.id] = memory
                            self._next_id = max(self._next_id, memory.id + 1)
            except (json.JSONDecodeError, KeyError) as e:
                print(f"Error loading memories: {e}")
                # Initialize with sample data
                self._initialize_sample_data()
        else:
            self._initialize_sample_data()
    
    def _initialize_sample_data(self):
        """Initialize with sample memories data"""
        sample_memories = [
            Memory(
                id=1,
                title="First Clone Success", 
                content="Finally figured out how to coordinate two clones to solve the pressure plate puzzle!",
                tags=["puzzle", "clone", "breakthrough"],
                people=["@player1"],
                created_at=datetime.now(),
                updated_at=datetime.now()
            ),
            Memory(
                id=2,
                title="Level 7 Strategy",
                content="The key is timing - create the first clone to hit the switch, then the second clone carries the block.",
                tags=["strategy", "timing", "level7"],
                people=["@player1", "@helper2"],
                created_at=datetime.now(),
                updated_at=datetime.now()
            ),
            Memory(
                id=3,
                title="Community Speedrun",
                content="Watched an amazing speedrun that used clone stacking in ways I never thought possible.",
                tags=["speedrun", "community", "advanced"],
                people=["@speedrunner_pro"],
                created_at=datetime.now(),
                updated_at=datetime.now()
            )
        ]
        
        for memory in sample_memories:
            self._memories[memory.id] = memory
            self._next_id = max(self._next_id, memory.id + 1)
        
        self._save_to_file()
    
    def _save_to_file(self):
        """Save memories to JSON file"""
        try:
            data = {
                'memories': [memory.dict() for memory in self._memories.values()]
            }
            with open(self.storage_file, 'w') as f:
                json.dump(data, f, indent=2, default=str)
        except Exception as e:
            print(f"Error saving memories: {e}")
    
    def get_memories(self, q: Optional[str] = None, tags: Optional[List[str]] = None, 
                    people: Optional[List[str]] = None, limit: int = 10, 
                    offset: int = 0) -> tuple[List[Memory], int]:
        """Get filtered and paginated memories"""
        memories = list(self._memories.values())
        
        # Apply search filter
        if q:
            q_lower = q.lower()
            memories = [m for m in memories if 
                       q_lower in m.title.lower() or q_lower in m.content.lower()]
        
        # Apply tag filter
        if tags:
            memories = [m for m in memories if any(tag in m.tags for tag in tags)]
        
        # Apply people filter
        if people:
            memories = [m for m in memories if any(person in m.people for person in people)]
        
        # Sort by updated_at descending
        memories.sort(key=lambda x: x.updated_at or datetime.min, reverse=True)
        
        total = len(memories)
        
        # Apply pagination
        end_idx = offset + limit
        paginated_memories = memories[offset:end_idx]
        
        return paginated_memories, total
    
    def get_memory(self, memory_id: int) -> Optional[Memory]:
        """Get a specific memory by ID"""
        return self._memories.get(memory_id)
    
    def create_memory(self, memory: Memory) -> Memory:
        """Create a new memory"""
        memory.id = self._next_id
        memory.created_at = datetime.now()
        memory.updated_at = datetime.now()
        self._memories[memory.id] = memory
        self._next_id += 1
        self._save_to_file()
        return memory
    
    def update_memory(self, memory_id: int, updates: Dict[str, Any]) -> Optional[Memory]:
        """Update an existing memory"""
        if memory_id not in self._memories:
            return None
        
        memory = self._memories[memory_id]
        for key, value in updates.items():
            if value is not None and hasattr(memory, key):
                setattr(memory, key, value)
        
        memory.updated_at = datetime.now()
        self._save_to_file()
        return memory
    
    def delete_memory(self, memory_id: int) -> bool:
        """Delete a memory"""
        if memory_id in self._memories:
            del self._memories[memory_id]
            self._save_to_file()
            return True
        return False
    
    def delete_memories(self, memory_ids: List[int]) -> int:
        """Delete multiple memories, return count of deleted items"""
        deleted_count = 0
        for memory_id in memory_ids:
            if memory_id in self._memories:
                del self._memories[memory_id]
                deleted_count += 1
        
        if deleted_count > 0:
            self._save_to_file()
        
        return deleted_count