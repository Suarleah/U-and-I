using FishNet.Serializing;


//need this for fishnet to serialize ItemSOs (otherwise theyd always use generic itemSO which can cause errors in subcalsses)
public static class ItemSOSerializer
{
    public static void WriteItemSO(this Writer writer, ItemSO value)
    {
        writer.WriteInt32(value == null ? -1 : value.id);
    }

    public static ItemSO ReadItemSO(this Reader reader)
    {
        int id = reader.ReadInt32();
        return id == -1 ? null : ItemDatabase.Instance.GetById(id);
    }
}