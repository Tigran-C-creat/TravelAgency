namespace TravelAgency.Domain.Enums
{
    public static class TravelAgencyRole
    {
        /// <summary>
        /// Чтение.
        /// </summary>
        public const string Read = "Read";

        /// <summary>
        /// Запись.
        /// </summary>
        public const string Write = "Write";

        /// <summary>
        /// Запись или чтение.
        /// </summary>
        public const string ReadOrWrite = $"{Read},{Write}";
    }
}
