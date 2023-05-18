# Battleship Melee Console

This is a version of the game Battleship that can be run in the console.

You win by sinking all of your opponents fleet.

## Input
At any time the `Esc` key will finish the game

### Entering coordinates
Coordinates are made up of a single letter `A`-`J` followed by a number `1`-`10`.

When you are finished entering the coordinate press `Enter` to submit.


### When placing ships
- `UpArrow` or `DownArrow` will set the ship's orientation to Vertical.
- `LeftArrow` or `RightArrow` will set the ship's orientation to Horizontal.
 
## Currently implemented and future plans
- ***[x] 1-player game vs stupid computer opponent***
- [ ] Game types
	- [x] ***BigBangTheory game type that replaces the Carrier with
      a Romulan Battle Bagel***
	- [ ] Multi-Attack firing where if you score a hit you get another go
	- [ ] Salvo firing where you get 1 shot for each ship you still have afloat
- [ ] Play vs computer over the network
- [ ] Play vs another human over the network
- [ ] Play vs multiple human over the network, Melee style

## Command-line syntax

``` text
DESCRIPTION:
The game of Battleship

USAGE:
    ... [TYPE] [OPTIONS]

ARGUMENTS:
    [TYPE]    Battleship game type - classic, bigbang, or melee (not yet implemented)

OPTIONS:
                    DEFAULT
    -h, --help                 Prints help information
    -r, --random               Place the ships randomly
    -u, --user      Human      Name of the player

COMMANDS:
    battleship    The game of Battleship
```

