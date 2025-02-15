# Floating Clock

A minimalist, always-on-top floating clock for Windows written in C#. This application displays the current time in a semi-transparent window that stays on top of other applications.

## Features

- Always-on-top display
- Semi-transparent window (60% opacity)
- Dark theme with light gray text
- Draggable window
- Hidden from taskbar
- Tool window style (no minimize/maximize buttons)
- Centered time display
- 24-hour format (HH:mm:ss)

## Usage

- **Move the clock**: Click and drag anywhere on the window
- **Exit**: Press ESC key
- **Default position**: Top right corner of the screen

## Building from Source

```cmd
csc /target:winexe .\jam.cs
.\jam.exe
```

## Screenshots

![alt text](./assets/screenshot.png)

## Technical Details

- Window size: 120x40 pixels
- Font: Arial 14pt Bold
- Background color: RGB(40, 40, 40)
- Text color: Light Gray
- Update interval: 1 second