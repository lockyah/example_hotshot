# example_hotshot
A collection of C# codes used in the Hot Shot prototype.

<b>PlayerControl</b> is the controller for the player character. It uses an enum state system to respond appropriately to inputs and change physics, health, etc. This script also handles the 360-degree aiming, pointing the player's arm and weapon toward the cursor while factoring in the 3D difference in position.
