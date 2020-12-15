### FreezeTag

*FreezeTag* is a plugin for the Among Us private server called [Impostor](https://github.com/Impostor/Impostor) that adds a new gamemode to Among Us. When the game will start, the impostors will be coloured in **RED**, the normal crewmates in **GREEN**. When an impostor gets near a crewmate, he'll *freeze* him, and the crewmate won't be able to move and will become **BLUE**. If all the crewmates become frozen, the impostors win; if the crewmates do all the tasks they win.

**Note: This plugin uses a modified Impostor server version, you need it to make the plugin work.**

## Installation
1. Download the latest modified server from [here](https://ci.appveyor.com/project/LorenzoPapi/freezetag/build/artifacts) and download the [latest release](https://github.com/LorenzoPapi/FreezeTag/releases) of the plugin.
2. Set up the [Impostor server](https://github.com/Impostor/Impostor) by following the instructions on their Github page.
3. Drop the FreezeTag.dll file in the `plugins` folder of your Impostor server.
4. To play with your client on your private server, see the instructions on the [Impostor](https://github.com/Impostor/Impostor) page.

## How it works
- When the game starts the impostors will be in **RED**. Watch out for them if you are a crewmate!
- A crewmate can unfreeze a frozen crewmate by standing near him for about 2-3 seconds.
- If crewmates finish all their tasks, they win. If impostors kill all of the crewmate, they win.
- In the Among Us lobby, the host can use the `/ftag on` and `/ftag off` commands in the chat to turn the FreezeTag mode on and off. Additionally, any player can write `/ftag help` to get an explanation about FreezeTag mode.

**Warning: if one impostor kills a crewmate, all of the impostors gets kicked.**

## Known issues
- Issue: when impostors wins, all of the crewmates get kicked. Explanation: there isn't a way to enable the impostor win by the server, the only work around that exists is kicking every crewmate. May look into this in the future.

## Credits
- Thanks a lot to [6pak](https://github.com/6pak), for creating the custom movement API that made this plugin possible.
- Credit to the [Impostor](https://github.com/Impostor/Impostor) for the cake and appveyor script example and for the base of the Impostor server.
- A huge thanks to my friend that gave me the idea for this.
