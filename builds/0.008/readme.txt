Changelog:

Reworked camera management to allow additional mobility options
added lean left/right (q and e respectively) and crouch (lCtrl). 
Crouch moves slower, but quieter and allows hiding behind shorter objects.

Added multiple level support- completing each level opens up the next level.
Implemented basic menu. First four levels are minimalist test level, fifth is larger test level.

The path through each level is saved and will persist if the level is reloaded.

Notes:
Need to add a system for displaying whether a level has been completed and how long it took in the menu.

Controls:
WASD to move. Shift sprints, space jumps. Ctrl crouches, Q and E lean left and right.
While paused, White knob on slider scrolls observed time. Grey knob controls target time.

When paused, hold right mouse to look around. Press r to resume recording from the end of the timeline. Press o to optimize the time between the white and grey knobs. C clears the timeline after the observed time. P plays through the timelineine at original speed.

Being spotted by a guard automatically stops recording or optimization. If you are spotted in the timeline due to a change in the past, the period after you are spotted will become paradoxed until you fix the problem area.

In one of the rooms is a capsule that serves as a placeholder level-end marker. try to see how fast you can get there.