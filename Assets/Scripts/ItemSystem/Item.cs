using UnityEngine;

/// <summary>
/// 物品基类
/// </summary>
[System.Serializable]
public class Item
{
    public string ItemID;
    public string ItemName;
    public string Description;
    public ItemType Type;
    public Sprite Icon;
    public int Value; // 数值（攻击力/恢复量等）

    public Item(string id, string name, string desc, ItemType type, int value)
    {
        ItemID = id;
        ItemName = name;
        Description = desc;
        Type = type;
        Value = value;
    }
}

/// <summary>
/// 物品类型
/// </summary>
public enum ItemType
{
    Weapon,     // 武器
    Armor,      // 防具
    Consumable, // 消耗品
    Key,        // 钥匙
    Material    // 材料
}

/// <summary>
/// 物品数据库
/// </summary>
public static class ItemDatabase
{
    public static Item GetItem(string itemID)
    {
        switch (itemID)
        {
            // 消耗品
            case "potion_small":
                return new Item("potion_small", "小血瓶", "恢复20点生命值", ItemType.Consumable, 20);
            case "potion_medium":
                return new Item("potion_medium", "中血瓶", "恢复50点生命值", ItemType.Consumable, 50);
            case "potion_large":
                return new Item("potion_large", "大血瓶", "恢复100点生命值", ItemType.Consumable, 100);
            
            // 武器
            case "sword_wood":
                return new Item("sword_wood", "木剑", "攻击力+5", ItemType.Weapon, 5);
            case "sword_iron":
                return new Item("sword_iron", "铁剑", "攻击力+15", ItemType.Weapon, 15);
            
            // 防具
            case "armor_leather":
                return new Item("armor_leather", "皮甲", "防御力+5", ItemType.Armor, 5);
            case "armor_iron":
                return new Item("armor_iron", "铁甲", "防御力+15", ItemType.Armor, 15);
            
            // 钥匙
            case "key_normal":
                return new Item("key_normal", "钥匙", "可以打开某些门", ItemType.Key, 0);
            
            default:
                return null;
        }
    }
}
