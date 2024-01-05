# RTFunctions
Core mod for all of my mods. They all require this one otherwise they won't work.

Object optimization and animation system from [Catalyst](https://github.com/Reimnop/Catalyst).

Base mod includes:
- Debug features such as opening persistent data folder and app folder with configurable keybinds.
- Extra languages (verrry wip).
- Master and Animation difficulties added.
- Catalyst optimization system that works in the editor (EditorManagement is recommended).
- Default themes prefixed with "PA ".
- All themes now have 18 object color slots allowing for extra creativity. If you still want to use simplistic color themes, then just use whatever number of the slots you want.
- Game Version number is 4.1.16 based on where the old version format was going.
- Mod handlers.
- Relative keyframes, allowing you to add onto the previous keyframe's value or set the value to absolute. For example, rotation keyframes usually add onto the previous keyframe, now that can be toggled with relative.
- Parallax Parenting. Multiplies by the parent's position / scale / rotation.
- Additive Parenting (wip)
- LDM for objects (Low Detail Mode, objects that have LDM on don't get rendered with the Low Detail Mode config enabled.)
- Two types of homing keyframes: Static Homing and Dynamic Homing. Static Homing targets the player once on keyframe activation. Dynamic Homing always targets the player, with an optional range, a toggle for fleeing and a delay. Position, Rotation and Color all have this, except scale. Idk what to do with scale homing.
- BeatmapObject Z Axis Position for position keyframes. Works with parenting, so you don't need to make a new model every time you want to change the render depth.
- BeatmapObject Color keyframes now have opacity, hue, saturation and value.
- Player boost quickly bug is fixed.
- Custom data for level Metadata, such as Creator links, level tags, etc.
- Theme features from newer versions of PA; Effect colors and GUI Accent (Tail) color.
- Additions to background objects (new reactive channels, such as position, rotation and color, shade iterations and more)
- Custom QuickElements (animated text) system.
- Level loading system, allowing you to load a level from anywhere for any reason without any problems. Includes customizable end function. This is purely for mod developers / ObjectModifiers coders.
- Added new shapes, such as diamonds, more circles, chevron arrow, Image Shapes, pentagons, PA logo and more. (currently collision doesn't work with these so I don't recommend using them as normal objects.)
- Tons of new fonts to use!
- Tons of bug fixes and optimizations.
