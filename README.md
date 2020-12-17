FreezeTag
=========

[![AppVeyor](https://ci.appveyor.com/api/projects/status/2xv760q7g0payy74/branch/master?svg=true)](https://ci.appveyor.com/project/LorenzoPapi/freezetag/branch/master)

*FreezeTag* is a plugin for the Among Us private server called [Impostor](https://github.com/Impostor/Impostor) that adds the Freeze Tag gamemode to Among Us.

### **Note: This plugin uses a modified Impostor server version, you need it to make the plugin work.**

## Installation
1. Download the [latest modified server](https://ci.appveyor.com/project/LorenzoPapi/freezetag/branch/master/artifacts) and the [latest release](https://ci.appveyor.com/project/LorenzoPapi/freezetag/branch/master/artifacts) of the plugin.
2. Set up the Impostor server by following the instructions on their [Github page](https://github.com/Impostor/Impostor).
3. Drop the FreezeTag.dll file in the `plugins` folder of your Impostor server.
4. To play on your server, see the instructions on the [Impostor](https://github.com/Impostor/Impostor) page.

## How it works
- When the game starts the impostors will be in **RED**, crewmates in **GREEN**. Watch out for the impostors if you are a crewmate!
- The goal of the impostors is to freeze every crewmate. A crewmate becomes frozen and **BLUE** if an impostor stays near him.
- A crewmate can unfreeze a frozen crewmate by standing near him for about 2-3 seconds.
- If the crewmates finish all their tasks, they win.
- In the Among Us lobby, the host can use the `/ftag on` and `/ftag off` commands in the chat to turn the FreezeTag mode on and off. Additionally, any player can write `/ftag help` to get an explanation about FreezeTag mode.
- 
- **Sabotaging is not allowed, venting is.**
- **Warning: if one impostor kills a crewmate, all of the impostors gets kicked.**

## Known issues
- *When impostors wins, all of the crewmates get kicked*; there isn't a way to enable the impostors win by the server, the only work around that exists is kicking every crewmate. May look into this in the future.
- *My friends don't follow the rules and sabotage, help!*; I'm trying to make it that if sabotage happens, impostor gets kicked, be ready for a new release.

## Contributing and reporting issues
- Before posting your issue be sure to have the latest modified impostor server and the latest plugin.
- If you want to try to solve issues or add functionalities, don't be shy and make your changes in a fork! Then make a pull request where you write what you modified and what issue should close.

## Credits
- Thanks a lot to [6pak](https://github.com/6pak), for creating the custom movement API that made this plugin possible.
- Credit to the [Impostor](https://github.com/Impostor/Impostor) for the cake and appveyor script example and for the base of the Impostor server.
- A huge thanks to my friend that gave me the idea for this.
