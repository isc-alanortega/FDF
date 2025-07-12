using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_FDF.Utility;

namespace PCG_FDF.Data.Entities
{
    public class StatefulCard : Card
    {
        private bool Data_Complete { get; set; } = false;
        private IDictionary<int, bool> Chips_Complete { get; set; } = new Dictionary<int, bool>();

        public StatefulCard() { }

        public StatefulCard(Card source)
        {
            Card_ID = source.Card_ID;
            Card_Sequence = source.Card_Sequence;
            From_Services = source.From_Services;
            Tariff_Fields = source.Tariff_Fields;
            Card_Name_ID = source.Card_Name_ID;
            Card_Name_ES = source.Card_Name_ES;
            Card_Name_EN = source.Card_Name_EN;
            Card_Icon_ID = source.Card_Icon_ID;
            Card_Icon = source.Card_Icon;
            Icon_ViewBox = source.Icon_ViewBox;
            Elements = source.Elements;
        }

        public StatefulCard(StatefulCardMiddleware source)
        {
            Card_ID = source.Card_ID;
            Card_Sequence = source.Card_Sequence;
            Tariff_Fields = source.Tariff_Fields;
            From_Services = source.From_Services;
            Card_Name_ID = source.Card_Name_ID;
            Card_Name_ES = source.Card_Name_ES;
            Card_Name_EN = source.Card_Name_EN;
            Card_Icon_ID = source.Card_Icon_ID;
            Card_Icon = source.Card_Icon;
            Icon_ViewBox = source.Icon_ViewBox;
            Elements = source.Elements;
            Data_Complete = source.Data_Complete;
            Chips_Complete = source.Chips_Complete;
        }

        public string GetCardName()
        {
            if (LanguageUtil.Language == ELanguage.SPANISH)
            {
                return Card_Name_ES;
            }
            else
            {
                return Card_Name_EN;
            }
        }

        public void SetChipsValidation(int Element_ID, bool value)
        {
            Chips_Complete[Element_ID] = value;
        }

        public IDictionary<int, bool> GetChipsValidation()
        {
            return Chips_Complete;
        }

        public void RemoveAssociation(int Service_ID)
        {
            From_Services.Remove(Service_ID);
        }

        public void SetValidationState(bool validation)
        {
            Data_Complete = validation;
        }

        public bool GetCardValidated()
        {
            return Data_Complete;
        }
    }
}
