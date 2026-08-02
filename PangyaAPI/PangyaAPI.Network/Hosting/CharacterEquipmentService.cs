using System;
using PangyaAPI.IFF;
using PangyaAPI.Network.Models;

namespace PangyaAPI.Network.Hosting
{
    public sealed class CharacterEquipmentService(IIffCatalogProvider catalogProvider)
    {
        private static readonly string[] Stats = ["Power", "Control", "Accuracy", "Spin", "Curve"];
        private IffCatalog Catalog => catalogProvider.Catalog;

        public bool IsPartEquipped(CharacterInfo character, uint partTypeId, int? instanceId = null)
        {
            if (partTypeId == 0 || PangyaIffQueries.GetCharacter(partTypeId) != (character._typeid & 0xFF)) return false;
            uint slot = PangyaIffQueries.GetCharacterPart(partTypeId);
            return slot < character.parts_typeid.Length && character.parts_typeid[slot] == partTypeId &&
                (instanceId is null || character.parts_id[slot] == instanceId.Value);
        }

        public void InitializeDefaultParts(CharacterInfo character)
        {
            if (character._typeid == 0) return;
            Array.Clear(character.parts_typeid);
            Array.Clear(character.parts_id);
            IffTable parts = Catalog.GetTable("Part");
            for (uint slot = 0; slot < character.parts_typeid.Length; slot++)
            {
                uint typeId = (((character._typeid << 5) | slot) << 13) | 0x0800_0400u;
                if (parts.Find(typeId) is not null) character.parts_typeid[slot] = typeId;
            }
        }

        public int GetEquippedStat(CharacterInfo character, CharacterInfo.Stats statistic)
        {
            int index = (int)statistic;
            if ((uint)index >= Stats.Length) throw new ArgumentOutOfRangeException(nameof(statistic));
            string field = Stats[index];
            int total = 0;
            IffTable parts = Catalog.GetTable("Part");
            foreach (uint typeId in character.parts_typeid)
                if (typeId != 0 && parts.Find(typeId) is { } part) total += part.GetStat(field + "Up");
            IffTable auxParts = Catalog.GetTable("AuxPart");
            foreach (uint typeId in character.auxparts)
                if (typeId != 0 && auxParts.Find(typeId) is { } auxPart) total += auxPart.GetStat(field);
            IffTable cards = Catalog.GetTable("Card");
            foreach (uint typeId in character.Card_Character)
                if (typeId != 0 && cards.Find(typeId) is { } card) total += card.GetStat(field);
            return total;
        }
    }
}
