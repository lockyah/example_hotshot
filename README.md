# example_hotshot
A collection of C# codes used in the Hot Shot prototype.

<b>PlayerControl</b> is the controller for the player character. It uses an enum state system to respond appropriately to inputs and change physics, health, etc. This script also handles the 360-degree aiming, pointing the player's arm and weapon toward the cursor while factoring in the 3D difference in position.

<b>InvokeScripts</b> is a component that allows an editor to keep a reference to a gameobject, a script, and function names - when the Activate function is used, each function is invoked on the referenced object's scripts. This is used in various areas, but namely in environmental buttons/targets that need to trigger different effects when used.

<b>CutsceneDirector</b> is the implementation of the Ink package. BeginCutscene is called when interacting with something that requires a dialogue box or in-game cutscene, and runs through the Ink script while cooperating with the dialogue box's controller to change text speed, ability to skip the text crawl, or alter whichnames and dialogue portraits are being shown.
