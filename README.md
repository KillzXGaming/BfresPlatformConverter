# Bfres Platform Converter
An automatic bfres converter to convert between Wii U and Switch platforms.

Keep in mind BFRES has many signifcant changes between the 2. The most common change being materials and shaders.

Individual games have to have their own conversion handler to change materials. Some games have signifcant changes, some have only minor adjustments.

**Currently supports converting Wii U to Switch**

## Supported Games
- Breath of the Wild

## Planned
- Switch to Wii U conversion. (Currently needs better material animation handling and tex1/tex2 support for botw)
- MK8 support. Materials don't fully work well enough to consistenly convert properly atm. Tracks also have online issues.

## Development
For those interesting in adding another game to be supported, all that needs to be done is adding a material handler for the game you are converting from. These presets are done [here](https://github.com/KillzXGaming/BfresLibrary/tree/master/BfresLibrary/PlatformConverters/Presets).
