# RTFunctions
Core mod for all of my mods. They all require this one otherwise they won't work.
It's HIGHLY recommended to install this and other mods via [Project Launcher](https://github.com/RTMecha/ProjectLauncher/releases/latest/).

Object optimization and animation system from [Catalyst](https://github.com/Reimnop/Catalyst).
Updated Steam API dll from [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET).
Full Steamworkshop integration from [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks).

Base mod includes:
- Added new shapes, such as diamonds, more circles, chevron arrow, Image Shapes, pentagons, PA logo and more.
- All themes now have 18 object color slots allowing for extra creativity. If you still want to use simplistic color themes, then just use whatever number of the slots you want.
- Relative keyframes, allowing you to add onto the previous keyframe's value or set the value to absolute. For example, rotation keyframes usually add onto the previous keyframe, now that can be toggled with relative.
- Parallax Parenting. Multiplies by the parent's position / scale / rotation.
- Additive Parenting.
- Parent Desyncing. Object acknowledges the parent values once then ignores afterwards.
- LDM for objects (Low Detail Mode, objects that have LDM on don't get rendered with the Low Detail Mode config enabled).
- Two types of homing keyframes: Static Homing and Dynamic Homing. Static Homing targets the player once on keyframe activation. Dynamic Homing always targets the player, with an optional range, a toggle for fleeing and a delay. Position, Rotation and Color all have this, except scale. Idk what to do with scale homing.
- BeatmapObject Z Axis Position for position keyframes. Works with parenting, so you don't need to make a new model every time you want to change the render depth.
- BeatmapObject Color keyframes now have opacity, hue, saturation and value.
- Debug features such as opening persistent data folder and app folder with configurable keybinds.
- Master and Animation difficulties added.
- Catalyst optimization system that works in the editor (EditorManagement is recommended).
- Default themes prefixed with "PA ".
- Game Version number is 4.1.16 based on where the old version format was going.
- Mod handlers.
- Player boost quickly bug is fixed.
- Custom data for level Metadata, such as Creator links, level tags, etc.
- Theme features from newer versions of PA; Effect colors and GUI Accent (Tail) color.
- Additions to background objects (new reactive channels, such as position, rotation and color, shade iterations and more)
- Custom QuickElements (animated text) system.
- Level loading system, allowing you to load a level from anywhere for any reason without any problems. Includes customizable end function. This is purely for mod developers / ObjectModifiers coders.
- Extra languages (verrry wip).
- Tons of new fonts to use!
- Tons of bug fixes and optimizations.