# EasyFurnace
[EasyFurnace](http://oxidemod.org/plugins/1191/) creates a joy out of using furnaces.

## What is EasyFurnace used for?
Is the filling of furnaces to much of a pain? Then EasyFurnace is for you, all you have to do is drag some metal or sulfur into the furnace and sit back to watch the magic.

## Config
The config file for the plugin is located in Oxide's default config file folder ([serverroot]/server/[serveridentity]/oxide/config).

The default configuration:
```json
{
  "Furnace": {
    "Metal": {
      "Ores": 4,
      "Output": 1,
      "Wood": 1
    },
    "Sulfur": {
      "Ores": 4,
      "Output": 1,
      "Wood": 1
    }
  },
  "LargeFurnace": {
    "Metal": {
      "Ores": 12,
      "Output": 1,
      "Wood": 5
    },
    "Sulfur": {
      "Ores": 12,
      "Output": 3,
      "Wood": 3
    }
  }
}
```
The sum of **"Ores"**, **"Output"** and **"Wood"** has to be **6** for normal furnaces and **18** for large furnaces.
* **"Ores": {int}** - The amount of slots in the furnace to be used by ores.
* **"Output": {int}** - The amount of slots in the furnace to be used by the output material.
* **"Wood": {int}** - The amount of slots in the furnace to be used by wood.
