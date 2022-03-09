# example_hotshot
A collection of C# codes used in the Hot Shot prototype.

<b>PlayerControl</b> is the controller for the player character. It uses an enum state system to respond appropriately to inputs and change physics, health, etc. This script also handles the 360-degree aiming, pointing the player's arm and weapon toward the cursor while factoring in the 3D difference in position.

<b>LevelManager</b> is a static, persistent script that namely handles the checkpoint data of the player on respawning, but also realigns the camera to the non-static player object.

<b>NPC_Wander</b> is a basic AI script that simply wanders from side to side between given X coordinates. If it finds a drop or obstacle before reaching this coordinate, it will turn around early after a short delay.
