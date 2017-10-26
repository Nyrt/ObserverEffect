Changelog:

Updated timeline to give much more quantifiable information

Reworked guard pathing system to use physical waypoints for ease of level creation

Created new test level based on real-life office floor plan. The end marker is in the room to the left of the start room, past the two stationary guards. Best time at the moment is 101 seconds, though I didn't try very hard. Current level has too few options, making improvement difficult. Future levels should have many more paths to choose.

Added vertical shift key: 'E' (debug purposes only right now)

Notes: 
Level layout may become pretty confusing. Add minimap?
Noticed lag when recording and playing with large numbers of nearby guards. Fell through the floor once. Going to have to stress-test pretty rigorously at some point and see how much I can optimize stuff.

Controls:
WASD to move. Shift sprints, space jumps.
While paused, White knob on slider scrolls observed time. Grey knob controls target time.

When paused, hold right mouse to look around. Press r to resume recording from the end of the timeline. Press o to optimize the time between the white and grey knobs. C clears the timeline after the observed time. P plays through the timelineine at original speed.

Being spotted by a guard automatically stops recording or optimization. If you are spotted in the timeline due to a change in the past, the period after you are spotted will become paradoxed until you fix the problem area.

In one of the rooms is a capsule that serves as a placeholder level-end marker. try to see how fast you can get there.