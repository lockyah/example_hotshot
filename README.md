# example_hotshot
A collection of C# codes used in the Hot Shot prototype.

<b>PlayerControl</b> is the controller for the player character. It uses an enum state system to respond appropriately to inputs and change physics, health, etc. This script also handles the 360-degree aiming, pointing the player's arm and weapon toward the cursor while factoring in the 3D difference in position.

<b>CutsceneDirector</b> is the implementation of the Ink package. BeginCutscene is called when interacting with something that requires a dialogue box or in-game cutscene, and runs through the Ink script while cooperating with the dialogue box's controller display dialogue and portraits on screen.

<b>CanvasController</b> is the other half of the CutsceneDirector and contains functions for typing each letter of a line, skipping to the end of a typing line, setting character portraits and activating animations, as well as for in-game UI such as the health bar.
