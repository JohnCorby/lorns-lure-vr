# Lorn's Lure VR

Fork of [UUVR](https://github.com/Raicuparta/uuvr) tailored for [Lorn's lure](https://store.steampowered.com/app/1417930/Lorns_Lure/)

Yes, this means the code looks kind of bad. It's my first VR project, sorry!

Also Lorn's Lure has a codebase that's really annoying to mod around so I'm hesitant to work on this more. 

## Features

- 6DOF
- You have hands and they function.
- Most actions are tied to hand direction instead of camera direction.
- Crouching requires crouching irl

## Current inconveniences

- You have to wiggle the mouse a little when first on the title screen for it to register turning with the vr controllers for some reason. Probably something with Rewired, which I hate with every fiber of my being.
- No moving or rotating origin, because I couldn't be bothered to figure it out. Your right hand controls direction, but wall detection and head tilt still uses the real player forward.
- You must manually turn off bloom or else you get a weird white haze over your screen. Also turn the sensitivity down. I use 2.
- I have not tested other controllers, so the hands might be rotated weirdly if you don't have HP Reverb G2 controllers.

# Installing

idk yet raicuparta needs to set that up

# Contributing

Please go through the TODO and BUG comments and fix them. You can use AI if you want, the code that I wrote is already so bad I don't think AI slop will make it worse.

## Building

- Switch to the legacy-mono build configuration
- In Directory.Build.Props, set BepInExDir and GameManagedDir
- Build

## License

    Rai Pal
    Copyright (C) 2026  Raicuparta and JohnCorby

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
