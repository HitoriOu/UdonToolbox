Udon Toolbox (by "Hitori Ou").
Downloaded from www.github.com/HitoriOu/UdonToolbox

A collection of U# scripts that are easy to understand and use (for Udon worlds on "VR Chat"). 
For free use/modification allowed.

You will need U# for the code/scripts to work, download latest version from here.
https://github.com/Merlin-san/UdonSharp/releases

Many of these are designed to function as the old trigger system from the SDK2, 
meaning the component for buttons/triggers/colliders may have to be on the same game object the script is on and that it need collider/trigger components (or other required).

------------ Please Note the following: ------------
Do not edit the code it self (updated versions will overwrite it).
If you want to modify the code then make a copy of it with a unique "class name" to edit instead.
If you do edit the code/copy of it, please make a note of this in the code (a simple edited/modified by "name" in the dev notes will suffice).
Updated versions might have new variable names (will be noted in the readme file if so), if this is the case you will have to redo the setup of said variable (sorry for the inconveniance).
All codes are provided as "work in progress" i try clear all bugs and test but some issues will slip by me.
If you want to credit the use of the toolbox feel free to do so (with link/adress to github provided).

------------ Usefull Rules to be aware of: ------------
1: Udon Behaviour Scripts do not work if the "Program Source" is empty, at minimum it needs a Udon file (.asset), if for Object Synch use the provided "NULL" or the official "Empty" provided by the SDK.
2: Network synched events/scripts (global) will not work in "play mode", have to actually be inside VRchat to test those (made automated code to take care of this, so never mind this rule).
3: When a component/object parameter for a Udon script is left empty it default to the object the script is on (can cause errors if empty, depending on script design).
4: Udon Scripts are handeled locally according to object, network synch is also per object basis. Only exception is if a global variable targets a Udon script on a different Game object. So any duplicate using the same script are regarded as a seperate instance/script despite sharing a common Udon "Program Source".
5: pickup object events might not trigger if Autohold is not toggeled to "Yes"

------------ Variable naming convention: ------------
ON_is_default	= Sets the codes state on start (is the mirror on or off for example)
Global_Synched	= Is the code Global and affect all players equally
Late_Join_Synched	= Does late joining players get synched.
Event_Interact	= You want the code run when clicking/interacting with the object
Event_OnCollision	= Individual code run on Enter and Exit
Event_OnCollisionEnter	= You want the code run when hitting a collider
Event_OnCollisionExit	= You want the code run when exiting a collider
Event_OnTrigger	= Individual code run on Enter and Exit
Event_OnTrigger_Enter	= You want the code run when hitting a Trigger
Event_OnTrigger_Exit	= You want the code run when exiting a Trigger
Event_OnPickup		= Run code on object Pickup
Event_OnDrop	= Run code on object Drop
Event_OnPickupUseDown	= Run code on PickupUseDown (controller trigger/button)
Event_OnPickupUseUp	= Run code on PickupUseUp (controller trigger/button)
Event_OnEnable	= Triggers when object is set active.
Owner_Only	= Networked function calls are only made to object owner
Cooldown	= Time in seconds that must elapse before you can use/trigger again (local)
Timer		= Amount of time before code is activated
Detect_Player	= Run code if player activates it
Detect_Object	= Run code if a object activates it

------------ Changelogs (newest first) ------------
------------ Changelog (V2.2) ------------
------ Udon files changed: ------
Despawn Timer [added option for pool system (disables instead of permanently removing it)]
Custom Event Trigger [bugfix for when script array element = null]
Modify Udon Number (float) [added support for UI input Fields & removed support for late join synch (did not make sense to have)]
Vanishing Chair (pickup) [bugfix for "onstation" events (dev updates)]

------------ Changelog (V2.1) ------------
------ Udon files added: ------
Player Param Tester [debug tool for trying different movement settings safely]
Custom Event Trigger [multi event trigger with toggle event options]

------------ Changelog (V2.0) ------------
Notes:
Started adding in more complex scripts (Programs)
changed readme to list versions by newest first
added sound files for demo world

------ Added attributes: ------
Player movement (on start)
Reset Position Multiple
RunZone
Set Active Multiple
Spawn Object (clone)
Teleport Object
Teleport Player
Time And Date
Timed Relay (Shy Object)
Toggle Multiple
Toggle Udon Bool
Trigger Toggle
Trigger Toggle (Vectorized)
Vanishing Chair (pickup)
Vanishing pickup

------ Udon files changed: ------
Toggle Udon Bool [added code for calling update functions]

------ Udon files added: ------
Modify Udon Number (float) [can be used to locally set a value or update a script/program based on UI-slider (volume for example)]
Sound Player [music player with a multitude of functions]
Stopwatch [time checker, usefull for competitions and testing]

------------ Changelog (V1.9) ------------
Notes:
Remade all code to disable Global_Synched automatically if using play button in unity to test (automation for easy testing).
Started adding "attributes" to code (Headers and tooltip info) for easyer use/understanding

------ Udon files changed: ------
Time And Date [optimized code to do around 1/5 as many cycles and task divide by factor of 3 for smoother load]
Teleport Player [fixed bug in random setting (was ignoring last target)]
Teleport Object [fixed bug in random setting (was ignoring last target)]
Material Swapper [reworked to support multiple swap targets]

------ Added attributes: ------
Player Movement Stats
Time And Date
Alarmclock
Cycle Toggle
Despawn Timer
Material Cycler (cloner)
Material Swapper
MobilityZone
MobilityZone (vectorized)

------ Udon files added: ------
Reset Position Multiple [resets multiple objects according to multiple target markers]
Alarmclock [toggles objects on/off for a set duration after timer has elapsed]

------------ Changelog (V1.8) ------------
Notes:
Mistakes was made, had to rename the event variables on file "Toggle Multiple" (used wrong name by accident)
Moved folders and files (please remove old folder and use the scripts in the new folder instead)!
"Udon Toolbox/Objects/Toggle/Trigger Toggle.asset" file moved to folder "Udon Toolbox/Triggers/" 
"Udon Toolbox/Triggers" folder moved to folder "Udon Toolbox/Objects/"

------ Udon files renamed: ------
Player & Object Trigger Toggle [renamed to "Trigger Toggle"]

------ Udon files changed: ------
Vanishing Chair (pickup) [converted to U# code with improvements]
RunZone [converted to U# code with improvements]
Toggle Multiple [fixed bug, "Late_Join_Synched" was missing code]
Trigger Toggle [converted to U# code with improvements]
MobilityZone (vectorized) [converted to U# code with improvements]
Player Movement Stats [converted to U# code with improvements]

------ Udon files removed: ------
Player Oneway Gate Enter Trigger (vectorized) [replaced by "Trigger Toggle (Vectorized)"]
Player Oneway Gate Enter&Exit Trigger (vectorized) [replaced by "Trigger Toggle (Vectorized)"]
Player Oneway Gate Exit Trigger (vectorized) [replaced by "Trigger Toggle (Vectorized)"]
Clock (Local time) [replaced by "Time And Date"]
Date (Local time) [replaced by "Time And Date"]
Time in Game [replaced by "Time And Date"]
Time in world [replaced by "Time And Date"]
InputField Syncher (in progress) [it got placed in by accident (non-functioning prototype)]

------ Udon files added: ------
Trigger Toggle (Vectorized) [converted/merged 3 files (Player Oneway Gate) to U# code with mayor improvements (can detect objects as well now)]
Time And Date [converted/merged 4 files (Clock/Date/Time) to U# code with mayor improvements (Can handle multiple UI's and show UTC/GMT flexibly with +/- timezones)]

------------ Changelog (V1.7) ------------
Notes:
Added into the demo a toggle button for testing local vs global setups
Moved folders (please remove old folder and use the scripts in the new folder instead)!
"Udon Toolbox/Teleport" folder moved to "Udon Toolbox/Players/" 
"Udon Toolbox/Toggle" folder moved to "Udon Toolbox/Objects/"
Moved file "Toggle Udon Bool" to folder "Udon Toolbox/Udon Specific/"

------ Udon files changed: ------
SpawnObject(clone) [converted to U# code with improvements]
Despawn Timer [converted to U# code with improvements]
Vanishing pickup [converted to U# code with improvements]
Teleport Player [converted to U# code with improvements]
Cycle Toggle [converted to U# code with improvements]
Toggle Udon Bool [converted to U# code with improvements]
Timed Relay (Shy Object) [converted to U# code with improvements]

------ Udon files added: ------
Teleport Object [has same function as "Teleport Player" version but for objects]

------ Udon files removed: ------
Toggle Multiple (local) [replaced by "Toggle Multiple"]
Toggle Multiple (Global) [replaced by "Toggle Multiple"]
Cycle Toggle (Synched) [replaced by "Cycle Toggle"]

------------ Changelog (V1.6) ------------
Notes:
Started converting graphs into U# code
Started work on a Udon Demo world for use examples.

------ Udon files added: ------
Despawn Timer [used for population control of spawned objects]
timed relay (Shy Object) [Optimiced timer that has no idle cycle/update]
Set Active Multiple [Usefull for one time actions (or for restarting the timed relay)]

------ Udon files changed: ------
Toggle Multiple [now syncs for late joiners as well]
Toggle Multiple (Global) [fixed bug (common variable for 2 functions)]
Cycle Toggle (Synched) [fixed bug (common variable for 2 functions)]
Material Cycler (cloner) [converted to U# code with improvements]
Material Swapper [converted to U# code with improvements]
Toggle Multiple [converted to U# code with improvements]
Player movement (on start) [converted to U# code with improvements]

------------ Changelog (V1.5) ------------
------ Udon files added: ------
Vanishing Chair (pickup) [Chair that turns invisible when used and cannot be grabbed by the user (while seated)]
VRCChair3 (Pickup).prefab [prefab with the correct settings needed to make the "Vanishing Chair" script work]
Spawn Object (clone) [clones and places/relocates the clone]

------------ Changelog (V1.4) ------------
------ Udon files added: ------
Player Oneway Gate Enter&Exit Trigger (vectorized) [triggers individual response depending on what side/vector_angle of the object/trigger the player is on, usefull for one way passages]
Player Oneway Gate Enter Trigger (vectorized)
Player Oneway Gate Exit Trigger (vectorized)
Vanishing pickup [handicap aid for desktop users (i recomend using "Disallow Theft = True" and "AutoHold = Yes" for "VRC Pickup (Script)" settings)]

------------ Changelog (V1.3) ------------
------ Udon files added: ------
Material Swapper [flips the materials between 2 objects]
Material Cycler (cloner) [Cycle thru multiple mesh renderers and copy one at a time onto multiple targets]
Player & Object Trigger Toggle [Trigger and collider based special toggle]

------ Udon files changed: ------
Toggle Multiple [fixed bug that forced global toggle, fixed node bug (fucking annoying)]

------------ Changelog (V1.2) ------------
Notes:
"Variable naming convention" enforced and explained in Readme

------ Udon files added: ------
Toggle Udon Bool [used to toggle a settings on a different udon script remotely (behaves as if the variable been hijacked so only use 1 button to toggle a setting as 2 will argue against each other)]
Cycle Toggle [cycle a list of objects and sets them on/off depending on if the object is the current focus/index]
Cycle Toggle (Synched)
Player Movement Stats [UI text for debug purposes]
RunZone [increases speed based on how far from the middle of the object/trigger you are]
MobilityZone [changes player stats on entry and exit with a trigger]
MobilityZone (vectorized) [changes player stats based on what heading from the vector you exit (usefull for building entrances and alike)]

------ Udon files changed: ------
Toggle Multiple [updated with more event options, also fixed a bug]

------------ Changelog (V1.1) ------------
Notes:
Started Changelog in Readme.txt file
Renamed Readme folder (remove old one "Readme & Info" if on V1)

------ Udon files added: ------
Time in Game
Time in world
Clock (Local time)
Date (Local time)
Player movement (on start)
Toggle Multiple (local) [used to be named "Toggle Multiple"]

------ Udon files changed: ------
Toggle Multiple [was changed to support more options like toggle options for Events and local/global]

------ Tutorial: ------
Is This Udon used.png (how to check if you have used a specific Udon file in the current scenes, "unloaded" scenes are not searched)

------------ Changelog (V1) ------------
Udon files added:
Toggle Multiple
Toggle Multiple (synched)
Teleport player

Tutorial:
Debug mode.png (how to change interact text)
Getting started.png (how to set up a udon script)
Network Synching Pickup & Objects.png (how to set up network synching for a pickup object)