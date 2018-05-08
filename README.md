# Super Monkey Ball 3

<iframe width="560" height="315" src="ourvideo" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>

## Project Description

This is a platform that controls a Super Monkey Ball-like game. With this platform, you tilt it in order to move a marble within a game. Your goal is to collect the cubes, while trying not to fall off the platform. However, the game will also interact with you; namely, when you collide with walls, the platform will jerk back and provide haptic feedback to the player, making this platform much more immersive.

This project uses the Novint Falcon as a base for the platform; The Novint Falcon was a haptic device that was around in the early 2000s that used three motors and potentiometers; When you tilt the platform, it tilts these potentiometers and moves the ball at the bottom of the platform, which sends it's position to the computer as mouse data. The platform itself is connected to the Falcon by three holders that are connected to the potentiometers of the platform, making it so when you tilt the platform, you also move the mouse.

On the software side, we communicate with Unity through a combination of drivers for the Novint Falcon written in C and (Peter, how did you do the haptic feedback?) When the Unity Scene starts, it starts up a C program that allows the Falcon to hijack the mouse. From there, we check the tilting of the ball by the position that the mouse currently is, which corresponds to the tilt of the mouse. To provide haptic feedback, we...

## Team Members

Chance Roberts (Hardware & Some Software/Hardware Integration)

Peter Gutenko (Software & Software/Hardware Integration)

## Resources

* PC/Mac with Unity
* The Unity scene found [here](www.github.com/pgutenko/PETERPLEASEUPLOAD)
* The Novint Falcon
* Three 3D Printed Parts [(Seen here)]()
* A Simple Foam Platform (Or a 3D Printed One)
* Various Pieces from the VEX Metal & Hardware Kit