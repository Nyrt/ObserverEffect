Changelog:

Reworked time controls slightly for simplicity- c no longer clears the timeline, instead 'r' starts recording from the observed time.
Optimization now prompts the player to keep/discard a new path if it is longer than the original.

Reworked guard patrols so that speed is detrmined from the path time rather than path time from the speed so that manyguard patrols can be easily synchronized.
Added "swivel" type guards that act like stationary cameras and turn sinusoidally.
Added "circle" type guards that patrol in smooth, even circles.

Created new, multi-floor test level showcasing the new types of guards and the more precise controls over guard patrols. The objective is at the top of the staircase.

Reworked volume system so that moving is loudest when footsteps occur, when the player lands from falling.
Added footstep sounds to path playback. Added distortion to paradoxed sounds.

Notes:
Need to add a system for displaying whether a level has been completed and how long it took in the menu, in addition to the total time elapsed.

Consider adding ability to cut the power for a limited time- disables cameras, reduces guard fov/range.

Future level designs will have to be much larger and more open to allow for many more possible avenues of approach.

Controls:
WASD to move. Shift sprints, space jumps. Ctrl crouches, Q and E lean left and right. R stops recording.
While paused, White knob on slider scrolls observed time. Grey knob controls target time.
P plays back the timeline at actual speed.
When paused, hold right mouse to look around. Press r to start recording from the current position on the timeline. Press o to optimize the time between the white and grey knobs. C clears the timeline after the observed time. P plays through the timelineine at original speed.

Being spotted by a guard automatically stops recording or optimization. If you are spotted in the timeline due to a change in the past, the period after you are spotted will become paradoxed until you fix the problem area.

In one of the rooms is a capsule that serves as a placeholder level-end marker. try to see how fast you can get there.