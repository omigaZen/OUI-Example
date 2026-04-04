using SheetForge;
using System.Diagnostics;

namespace GameConfig
{
    public partial class TbTestTable : TableBase<TbTest>
    {
        public override string TableName => "test";

        public override void Load(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            using var reader = new SheetReader(filePath);
            _items.Clear();
            _itemList.Clear();

            for (var index = 0; index < reader.RowCount; index++)
            {
                var item = TbTest.Load(reader);
                _items[item.Id] = item;
                _itemList.Add(item);
            }

            stopwatch.Stop();
            LoadTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        }

        public static TbTestTable LoadFromFile(string filePath)
        {
            var table = new TbTestTable();
            table.Load(filePath);
            return table;
        }
    }
}
