﻿namespace StardewValleyMods.CategorizeChests.Framework.Persistence
{
    interface ISaveManager
    {
        void Save(string path);
        void Load(string path);
    }
}