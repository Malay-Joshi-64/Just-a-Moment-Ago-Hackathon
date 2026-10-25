# Just a Moment Ago

**A pixel-art puzzle platformer where your past is chasing you.**

Every move you make is recorded. After a set number of steps, a shadow of your past self appears and repeats those exact moves, in the same order. Reach the exit before your past catches you, because if the two of you ever meet, the paradox ends the run.

> This repository contains the **demo** build.

## How It Works

1. **You act.** Each walk or jump uses up one of your limited **actions**, shown as dots in the top-left HUD.
2. **The past arrives.** After *n* steps (the demo says "Your Past shall arrive after 8 steps"), a shadow spawns at the start.
3. **The shadow replays you.** It performs the same *n* actions, in the same sequence, that you performed.
4. **Avoid the paradox.** If you and your shadow collide, **Your Past Found You** and the level is lost. A collision of past and present is a paradox.
5. **Clear the level.** Get through without meeting your past and you win: **The Past Yields**.

The puzzle is planning. Every move you make now is a move your shadow will repeat, so you have to think about where it will be before you decide where to go.

## Controls

| Key | Action |
| --- | ------ |
| `W` `A` `S` `D` | Walk |
| `Space` | Jump |
| `R` | Restart the level |

## Game Screens

- **Main menu:** Play, Settings, Quit
- **Pause menu:** Resume, Settings, Main Menu
- **Settings:** separate Master Volume and SFX Volume sliders
- **Win and lose screens:** *The Past Yields* and *Your Past Found You*, each with a Main Menu option

## Features

- Time-echo mechanic: your own recorded moves come back to hunt you
- Hand-crafted pixel-art environments with a moody ruined-castle atmosphere
- Action counter HUD that shows how many moves you have left before your past arrives
- Fully styled menus: main menu, pause menu, settings and end screens
- Adjustable master and SFX volume

## Built With

- Unity
- C#

## Getting Started

### Prerequisites

- Unity Hub with a compatible Unity Editor installed
- Git

### Run from source

1. Clone the repository:
   ```bash
   git clone https://github.com/Malay-Joshi-64/Just-a-Moment-Ago-Hackathon.git
   ```
2. Open Unity Hub, choose **Add > Add project from disk**, and select the `Project Files` folder.
3. Let Unity import the project. The first import takes a few minutes because the `Library` folder is regenerated locally.
4. Open the main menu scene and press **Play**.

### Play the build

Download the latest Windows build from the [Releases](https://github.com/Malay-Joshi-64/Just-a-Moment-Ago-Hackathon/releases) page, unzip it, and run the `.exe`.

## Repository Layout

```
Project Files/     Unity project (scenes, scripts, art, audio, settings)
README.md          This file
.gitignore         Excludes generated Unity folders and large downloads
```
- [Malay Joshi](https://github.com/Malay-Joshi-64)
- [Dev](https://github.com/devvardhansomvanshi)
