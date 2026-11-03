namespace SmartEnums
{
    public static class RoleNames
    {
        public const string Supplier = "Supplier";

        public const string SpazaOwner = "SpazaOwner";

        public const string Driver = "Driver";

        public static readonly string[] All = { Supplier, SpazaOwner, Driver };

        public static readonly string[] BusinessUsers = { Supplier, SpazaOwner };
    }
}
