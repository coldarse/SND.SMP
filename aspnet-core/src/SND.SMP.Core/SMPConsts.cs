using Abp.Domain.Uow;
using SND.SMP.Debugging;

namespace SND.SMP
{
    public class SMPConsts
    {
        public const string LocalizationSourceName = "SMP";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;

        public const string DbTablePrefix = "";


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "adaf61844732422c800d250fd007eb84";
    }
}
