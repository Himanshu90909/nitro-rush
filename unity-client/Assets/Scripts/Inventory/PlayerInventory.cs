using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroRush.Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string itemId;
        public string itemType; // "CAR", "PART", "DECALS", "TOKEN"
        public int quantity;
    }

    /// <summary>
    /// Client-side mirror of server inventory backed by HashMap (Dictionary<string, InventoryItem>).
    /// Server-authoritative design.
    /// </summary>
    public class PlayerInventory
    {
        private readonly Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>();

        public IReadOnlyDictionary<string, InventoryItem> Items => _items;

        public void SyncInventory(List<InventoryItem> serverItems)
        {
            _items.Clear();
            if (serverItems == null) return;

            foreach (var item in serverItems)
            {
                if (!string.IsNullOrEmpty(item.id))
                {
                    _items[item.id] = item;
                }
            }
        }

        public bool HasItem(string itemId)
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value.itemId == itemId && kvp.Value.quantity > 0)
                    return true;
            }
            return false;
        }
    }
}
