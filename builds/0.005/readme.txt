Changelog:

Guards no longer see through walls, and now have limited vision distance
Added phase controls to guard patrols
Optimized patrol algorithm
Created catch for edge-cases on path smoothing
Created test level. My record for one full lap is 42 seconds (no sprinting).

Controls:
WASD to move. Shift sprints, space jumps.
When recording, r pauses. White knob on slider scrolls observed time. Grey knob controls target time.

When paused, hold right mouse to look around. Press r to resume recording from the end of the timeline. Press o to optimize the time between the white and grey knobs. C clears the timeline after the observed time. P plays through the timelineine at real speed.

Being spotted by a guard automatically stops recording or optimization. If you are spotted in the timeline due to a change in the past, the period after you are spotted will become paradoxed until you fix the problem area.

At the moment, there is no set goal- this is just to test guard behavior and time controls.