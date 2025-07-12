using PCG_ENTITIES.PCG_FDF.BookingEntities;

namespace PCG_FDF.Data.Entities
{
    public class StatefulSection : SectionInitializer
    {
        private bool Data_Complete = false;
        public StatefulSection(SectionInitializer section)
        {
            Section_ID = section.Section_ID;
            From_Service = section.From_Service;
            Section_GUID = section.Section_GUID;
            Has_Chips = section.Has_Chips;
            Section_Name = section.Section_Name;
            Section_Icon = section.Section_Icon;
            Icon_ViewBox = section.Icon_ViewBox;
            Section_Sequence = section.Section_Sequence;
            Multiply_By_Quantity = section.Multiply_By_Quantity;
        }

        public StatefulSection(StatefulSectionMiddleware section)
        {
            Section_ID = section.Section_ID;
            From_Service = section.From_Service;
            Section_GUID = section.Section_GUID;
            Has_Chips = section.Has_Chips;
            Section_Name = section.Section_Name;
            Section_Icon = section.Section_Icon;
            Icon_ViewBox = section.Icon_ViewBox;
            Section_Sequence = section.Section_Sequence;
            Multiply_By_Quantity = section.Multiply_By_Quantity;
            Data_Complete = section.Data_Complete;
        }

        public void SetValidationState(bool validation)
        {
            Data_Complete = validation;
        }

        public bool GetSectionValidated()
        {
            return Data_Complete;
        }
    }
}
