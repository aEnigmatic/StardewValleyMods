using StardewValley;
using System;

namespace StardewValleyMods.CategorizeChests.Framework
{
    /// <summary>
    /// An exception to be raised when some code attempts to perform an
    /// operation on an item that's not recognized by the item repository.
    /// </summary>
    class ItemNotImplementedException : Exception
    {
        public ItemNotImplementedException(Item item)
            : base($"Chest categorization for item named {item.Name} is not implemented")
        {
        }
    }
}