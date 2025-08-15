"""
Natural language date parser for common scheduling phrases.
Supports formats like "Mon 9a", "in 45m", "tomorrow evening", etc.
"""
import re
from datetime import datetime, timedelta
from typing import Optional


def parse_date_string(date_str: str) -> Optional[datetime]:
    """
    Parse a natural language date string into a datetime object.
    
    Supported formats:
    - "Mon 9a", "Tue 2p", "Wed 10:30a" - day of week with time
    - "in 45m", "in 2h", "in 3d" - relative time from now
    - "tomorrow", "tomorrow morning", "tomorrow evening" - relative day
    - "next week", "next monday" - relative week/day
    - "9am", "2:30pm", "14:30" - time today
    - "2024-01-15", "01/15/2024" - specific dates
    
    Args:
        date_str: Natural language date string
        
    Returns:
        Parsed datetime object or None if parsing fails
    """
    if not date_str or not isinstance(date_str, str):
        return None
    
    date_str = date_str.strip().lower()
    now = datetime.now()
    
    # Replace common variations
    date_str = date_str.replace("a.m.", "am").replace("p.m.", "pm")
    
    # Pattern: "in X minutes/hours/days"
    relative_match = re.match(r'in\s+(\d+)\s*(m|min|mins|minutes?|h|hr|hrs|hours?|d|days?)', date_str)
    if relative_match:
        amount = int(relative_match.group(1))
        unit = relative_match.group(2)
        
        if unit in ['m', 'min', 'mins', 'minute', 'minutes']:
            return now + timedelta(minutes=amount)
        elif unit in ['h', 'hr', 'hrs', 'hour', 'hours']:
            return now + timedelta(hours=amount)
        elif unit in ['d', 'day', 'days']:
            return now + timedelta(days=amount)
    
    # Pattern: Day of week + time (e.g., "Mon 9a", "Tuesday 2:30pm")
    dow_time_match = re.match(r'(mon|tue|wed|thu|fri|sat|sun|monday|tuesday|wednesday|thursday|friday|saturday|sunday)\s+(.+)', date_str)
    if dow_time_match:
        day_name = dow_time_match.group(1)
        time_str = dow_time_match.group(2)
        
        # Map day names to weekday numbers (Monday = 0)
        day_mapping = {
            'mon': 0, 'monday': 0,
            'tue': 1, 'tuesday': 1,
            'wed': 2, 'wednesday': 2,
            'thu': 3, 'thursday': 3,
            'fri': 4, 'friday': 4,
            'sat': 5, 'saturday': 5,
            'sun': 6, 'sunday': 6
        }
        
        target_weekday = day_mapping.get(day_name)
        if target_weekday is not None:
            time_obj = parse_time_string(time_str)
            if time_obj:
                # Calculate days until target weekday
                current_weekday = now.weekday()
                days_ahead = (target_weekday - current_weekday) % 7
                if days_ahead == 0:  # If it's the same day, assume next week
                    days_ahead = 7
                
                target_date = now + timedelta(days=days_ahead)
                return target_date.replace(hour=time_obj.hour, minute=time_obj.minute, second=0, microsecond=0)
    
    # Pattern: "tomorrow" with optional time
    if 'tomorrow' in date_str:
        tomorrow = now + timedelta(days=1)
        
        # Check for time qualifiers
        if 'morning' in date_str:
            return tomorrow.replace(hour=9, minute=0, second=0, microsecond=0)
        elif 'afternoon' in date_str:
            return tomorrow.replace(hour=14, minute=0, second=0, microsecond=0)
        elif 'evening' in date_str:
            return tomorrow.replace(hour=18, minute=0, second=0, microsecond=0)
        elif 'night' in date_str:
            return tomorrow.replace(hour=20, minute=0, second=0, microsecond=0)
        else:
            # Check if there's a specific time
            time_part = date_str.replace('tomorrow', '').strip()
            if time_part:
                time_obj = parse_time_string(time_part)
                if time_obj:
                    return tomorrow.replace(hour=time_obj.hour, minute=time_obj.minute, second=0, microsecond=0)
            
            # Default to 9 AM tomorrow
            return tomorrow.replace(hour=9, minute=0, second=0, microsecond=0)
    
    # Pattern: "next week" or "next [day]"
    if 'next week' in date_str:
        next_week = now + timedelta(days=7)
        # Default to Monday 9 AM
        days_to_monday = (0 - next_week.weekday()) % 7
        next_monday = next_week + timedelta(days=days_to_monday)
        return next_monday.replace(hour=9, minute=0, second=0, microsecond=0)
    
    # Pattern: Just a time (assume today)
    time_obj = parse_time_string(date_str)
    if time_obj:
        result = now.replace(hour=time_obj.hour, minute=time_obj.minute, second=0, microsecond=0)
        # If the time has already passed today, schedule for tomorrow
        if result <= now:
            result += timedelta(days=1)
        return result
    
    # Pattern: ISO date format
    try:
        return datetime.fromisoformat(date_str.replace('Z', '+00:00'))
    except ValueError:
        pass
    
    # Pattern: MM/DD/YYYY or DD/MM/YYYY
    date_match = re.match(r'(\d{1,2})[/\-](\d{1,2})[/\-](\d{4})', date_str)
    if date_match:
        try:
            # Assume MM/DD/YYYY format (US)
            month = int(date_match.group(1))
            day = int(date_match.group(2))
            year = int(date_match.group(3))
            return datetime(year, month, day, 9, 0)  # Default to 9 AM
        except ValueError:
            pass
    
    return None


def parse_time_string(time_str: str) -> Optional[datetime]:
    """
    Parse a time string into a datetime object (date will be today).
    
    Supported formats:
    - "9a", "9am", "9:00am"
    - "2p", "2pm", "2:30pm"
    - "14:30", "14:30:00"
    - "9", "14" (assume 24-hour if >= 12, 12-hour if < 12)
    
    Args:
        time_str: Time string to parse
        
    Returns:
        Datetime object with parsed time or None if parsing fails
    """
    if not time_str:
        return None
    
    time_str = time_str.strip().lower()
    now = datetime.now()
    
    # Pattern: "9a", "9am", "2p", "2pm"
    simple_ampm = re.match(r'(\d{1,2})\s*([ap])m?$', time_str)
    if simple_ampm:
        hour = int(simple_ampm.group(1))
        is_pm = simple_ampm.group(2) == 'p'
        
        if is_pm and hour != 12:
            hour += 12
        elif not is_pm and hour == 12:
            hour = 0
            
        if 0 <= hour <= 23:
            return now.replace(hour=hour, minute=0, second=0, microsecond=0)
    
    # Pattern: "9:30a", "9:30am", "2:45p", "2:45pm"
    ampm_with_minutes = re.match(r'(\d{1,2}):(\d{2})\s*([ap])m?$', time_str)
    if ampm_with_minutes:
        hour = int(ampm_with_minutes.group(1))
        minute = int(ampm_with_minutes.group(2))
        is_pm = ampm_with_minutes.group(3) == 'p'
        
        if is_pm and hour != 12:
            hour += 12
        elif not is_pm and hour == 12:
            hour = 0
            
        if 0 <= hour <= 23 and 0 <= minute <= 59:
            return now.replace(hour=hour, minute=minute, second=0, microsecond=0)
    
    # Pattern: "14:30", "09:00"
    military_time = re.match(r'(\d{1,2}):(\d{2})(?::(\d{2}))?$', time_str)
    if military_time:
        hour = int(military_time.group(1))
        minute = int(military_time.group(2))
        second = int(military_time.group(3) or 0)
        
        if 0 <= hour <= 23 and 0 <= minute <= 59 and 0 <= second <= 59:
            return now.replace(hour=hour, minute=minute, second=second, microsecond=0)
    
    # Pattern: Just a number (assume hour)
    if time_str.isdigit():
        hour = int(time_str)
        if 0 <= hour <= 23:
            return now.replace(hour=hour, minute=0, second=0, microsecond=0)
        elif 1 <= hour <= 12:  # Assume PM for afternoon hours
            return now.replace(hour=hour + 12 if hour != 12 else 12, minute=0, second=0, microsecond=0)
    
    return None