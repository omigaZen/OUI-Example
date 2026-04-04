using SheetForge;
using System.Collections.Generic;

namespace GameConfig
{
    public partial class TbTest
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 生命值
        /// </summary>
        public int Hp { get; private set; }

        /// <summary>
        /// 技能列表
        /// </summary>
        public int[] Skills { get; private set; }

        internal static TbTest Load(SheetReader reader)
        {
            var item = new TbTest();
            item.Id = reader.ReadInt32();
            item.Name = reader.ReadString();
            item.Hp = reader.ReadInt32();
            item.Skills = reader.ReadInt32Array();
            return item;
        }

        private TbTest()
        {
        }
    }
}
