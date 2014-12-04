# Dota 2 ModKit

A GUI comprised of useful tools to help people make Dota 2 mods.

## Features:
* **Particle Forker**. Allows easy copying of decompiled particles into your addons. This will automatically change the child references after the particles are copied over.
* **Tooltips Generator**. Parses all the files in the scripts\npc folder of your addon, and creates tooltips for abilities, items, modifiers, units, and heroes, which you can easily copy over to addon_english.txt or another language file.
* **Copy Addon to another folder**. Copies the game and content directories of your addon to another folder, so it's neatly structured.

![Alt text](http://i.imgur.com/ilZLHSI.png)
![Alt text](http://i.imgur.com/S4ldj9r.png)

## Installation

1. Download D2ModKit.rar and extract it, or you can build from the .sln. If you're building from the .sln, you need to copy over the .dll's from the .rar over to the Debug folder for it to compile.

2. If you already have the decompiled particles, move them over to the D2ModKit folder. **Rename the decompiled particles folder to decompiled_particles**. If you don't have the decompiled particles, this program comes with decompiled_particles.rar.

3. Open D2ModKit.exe

**After you copy particles to an addon, you may have to restart the Workshop Tools to see them in the asset browser. I sometimes have to do this.**

## Credits

* Thanks to ToraxXx for decompiling all of the particles. https://github.com/toraxxx
* Thanks to RoyAwesome for making KVLib, which allows easy parsing of Valve KV files. https://github.com/RoyAwesome
* Thanks to Noya for giving me the idea of a Tooltips generator. https://github.com/MNoya

## Notes

Have ideas for new features? Let me know! Best way to contact me is through the Dota 2 modding IRC.
