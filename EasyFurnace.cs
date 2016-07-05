using System.Collections.Generic;
using System.Linq;
using System;

namespace Oxide.Plugins
{
    [Info("EasyFurnace", "oskar3123", "1.1.1", ResourceId = 1191)]
    class EasyFurnace : RustPlugin
    {
        class Cfg
        {
            public static int
                furnaceMetalOres,
                furnaceMetalWood,
                furnaceMetalOutput,
                furnaceSulfurOres,
                furnaceSulfurWood,
                furnaceSulfurOutput,
                largeFurnaceMetalOres,
                largeFurnaceMetalWood,
                largeFurnaceMetalOutput,
                largeFurnaceSulfurOres,
                largeFurnaceSulfurWood,
                largeFurnaceSulfurOutput;
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            Config.Clear();
            Config["Furnace", "Metal", "Ores"] = 4;
            Config["Furnace", "Metal", "Wood"] = 1;
            Config["Furnace", "Metal", "Output"] = 1;
            Config["Furnace", "Sulfur", "Ores"] = 4;
            Config["Furnace", "Sulfur", "Wood"] = 1;
            Config["Furnace", "Sulfur", "Output"] = 1;
            Config["LargeFurnace", "Metal", "Ores"] = 12;
            Config["LargeFurnace", "Metal", "Wood"] = 5;
            Config["LargeFurnace", "Metal", "Output"] = 1;
            Config["LargeFurnace", "Sulfur", "Ores"] = 12;
            Config["LargeFurnace", "Sulfur", "Wood"] = 3;
            Config["LargeFurnace", "Sulfur", "Output"] = 3;
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Cfg.furnaceMetalOres = (int)Config["Furnace", "Metal", "Ores"];
            Cfg.furnaceMetalWood = (int)Config["Furnace", "Metal", "Wood"];
            Cfg.furnaceMetalOutput = (int)Config["Furnace", "Metal", "Output"];
            Cfg.furnaceSulfurOres = (int)Config["Furnace", "Sulfur", "Ores"];
            Cfg.furnaceSulfurWood = (int)Config["Furnace", "Sulfur", "Wood"];
            Cfg.furnaceSulfurOutput = (int)Config["Furnace", "Sulfur", "Output"];
            Cfg.largeFurnaceMetalOres = (int)Config["LargeFurnace", "Metal", "Ores"];
            Cfg.largeFurnaceMetalWood = (int)Config["LargeFurnace", "Metal", "Wood"];
            Cfg.largeFurnaceMetalOutput = (int)Config["LargeFurnace", "Metal", "Output"];
            Cfg.largeFurnaceSulfurOres = (int)Config["LargeFurnace", "Sulfur", "Ores"];
            Cfg.largeFurnaceSulfurWood = (int)Config["LargeFurnace", "Sulfur", "Wood"];
            Cfg.largeFurnaceSulfurOutput = (int)Config["LargeFurnace", "Sulfur", "Output"];
        }

        int GetStackSize(string shortname) { return ItemManager.FindItemDefinition(shortname).stackable; }

        Dictionary<BaseOven, BasePlayer> furnaceCache = new Dictionary<BaseOven, BasePlayer>();

        void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            BaseOven furnace = entity as BaseOven;
            if (!furnace) return;

            furnaceCache[furnace] = player;
        }

        int RemoveItemsFromInventory(BasePlayer player, string shortname, int amount)
        {
            ItemDefinition itemToRemove = ItemManager.FindItemDefinition(shortname);
            List<Item> foundItems = player.inventory.FindItemIDs(itemToRemove.itemid);
            int numberFound = foundItems == null ? 0 : foundItems.Sum(item => item.amount);
            if (numberFound < amount) amount = numberFound;
            int numberRemoved = player.inventory.Take(foundItems, itemToRemove.itemid, amount);
            return numberRemoved;
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (item.info.shortname != "metal.ore" && item.info.shortname != "sulfur.ore") return;

            if (item.amount < 100) return;

            if (container.itemList.Count() > 1) return;

            int cap = container.capacity;
            if (cap != 6 && cap != 18) return;

            int oresize = cap == 6 ?
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalOres : Cfg.furnaceSulfurOres) :
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalOres : Cfg.largeFurnaceSulfurOres);
            int woodsize = cap == 6 ?
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalWood : Cfg.furnaceSulfurWood) :
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalWood : Cfg.largeFurnaceSulfurWood);
            int outputsize = cap == 6 ?
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalOutput : Cfg.furnaceSulfurOutput) :
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalOutput : Cfg.largeFurnaceSulfurOutput);

            if (cap == 6 &&
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalOres : Cfg.furnaceSulfurOres) +
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalWood : Cfg.furnaceSulfurWood) +
                (item.info.shortname == "metal.ore" ? Cfg.furnaceMetalOutput : Cfg.furnaceSulfurOutput)
            > 6)
                return;
            if (cap == 18 &&
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalOres : Cfg.largeFurnaceSulfurOres) +
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalWood : Cfg.largeFurnaceSulfurWood) +
                (item.info.shortname == "metal.ore" ? Cfg.largeFurnaceMetalOutput : Cfg.largeFurnaceSulfurOutput)
            > 18)
                return;

            BaseOven furnace = null;
            foreach (BaseOven key in furnaceCache.Keys)
                if (key.inventory == container)
                {
                    furnace = key;
                    break;
                }
            if (!furnace) return;

            BasePlayer player;
            if (!furnaceCache.TryGetValue(furnace, out player) || !player) return;

            int orecount = 0;
            Item[] items = player.inventory.AllItems();
            foreach (Item itm in items)
                if (itm.info.shortname == item.info.shortname)
                    orecount += itm.amount;
            orecount += item.amount;
            if (orecount > oresize * GetStackSize(item.info.shortname == "metal.ore" ? "metal.ore" : "sulfur.ore"))
                orecount = oresize * GetStackSize(item.info.shortname == "metal.ore" ? "metal.ore" : "sulfur.ore");

            int woodToRetain = (int)Math.Ceiling((orecount / oresize) * (item.info.shortname == "metal.ore" ? 5D : 2.5D));
            int woodMaxStack = GetStackSize("wood");
            if (woodToRetain > woodMaxStack * woodsize)
                woodToRetain = woodMaxStack * woodsize;

            int retainedWood = RemoveItemsFromInventory(player, "wood", woodToRetain);
            if (retainedWood < woodToRetain)
            {
                ItemManager.Create(ItemManager.FindItemDefinition("wood"), retainedWood).MoveToContainer(player.inventory.containerMain);
                return;
            }

            int retainedAmount;
            retainedAmount = RemoveItemsFromInventory(player, item.info.shortname == "metal.ore" ? "metal.fragments" : "sulfur", outputsize);
            if (retainedAmount < outputsize)
            {
                ItemManager.Create(ItemManager.FindItemDefinition("wood"), retainedWood).MoveToContainer(player.inventory.containerMain);
                return;
            }

            item.MoveToContainer(player.inventory.containerMain, -1, false);

            int extraWood = retainedWood % woodsize;
            int perstack = (int)Math.Floor((double)retainedWood / woodsize);
            for (int i = 0; i < woodsize; i++)
            {
                ItemManager.Create(ItemManager.FindItemDefinition("wood"), perstack + (extraWood > 0 ? 1 : 0)).MoveToContainer(container, -1, false);
                extraWood--;
            }

            for (int i = 0; i < outputsize; i++)
                ItemManager.Create(ItemManager.FindItemDefinition(item.info.shortname == "metal.ore" ? "metal.fragments" : "sulfur"), 1).MoveToContainer(container, -1, false);

            RemoveItemsFromInventory(player, item.info.shortname == "metal.ore" ? "metal.ore" : "sulfur.ore", orecount);

            int amountPerStack = orecount / oresize;
            Item[] oresToAdd = new Item[oresize];
            int extras = orecount % oresize;
            for (int i = 0; i < oresize; i++)
            {
                int tmpCnt = 0;
                if (extras > 0)
                    tmpCnt++;
                tmpCnt += amountPerStack;
                extras--;
                oresToAdd[i] = ItemManager.Create(ItemManager.FindItemDefinition(item.info.shortname == "metal.ore" ? "metal.ore" : "sulfur.ore"), tmpCnt);
            }

            foreach (Item oreToAdd in oresToAdd)
                oreToAdd.MoveToContainer(container, -1, false);

            furnace.Invoke("StartCooking", 0);
        }
    }
}