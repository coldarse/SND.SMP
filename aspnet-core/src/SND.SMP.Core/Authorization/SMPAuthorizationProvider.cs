using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace SND.SMP.Authorization
{
    public class SMPAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            /* Define your permissions here */
            context.CreatePermission(PermissionNames.Pages_PostalCountry, L("PostalCountries"));
            context.CreatePermission(PermissionNames.Pages_PostalCountry_Create, L("PostalCountriesCreate"));
            context.CreatePermission(PermissionNames.Pages_PostalCountry_Edit, L("PostalCountriesEdit"));
            context.CreatePermission(PermissionNames.Pages_PostalCountry_Delete, L("PostalCountriesDelete"));

            context.CreatePermission(PermissionNames.Pages_PostalOrg, L("PostalOrgs"));
            context.CreatePermission(PermissionNames.Pages_PostalOrg_Create, L("PostalOrgsCreate"));
            context.CreatePermission(PermissionNames.Pages_PostalOrg_Edit, L("PostalOrgsEdit"));
            context.CreatePermission(PermissionNames.Pages_PostalOrg_Delete, L("PostalOrgsDelete"));

            context.CreatePermission(PermissionNames.Pages_Postal, L("Postals"));
            context.CreatePermission(PermissionNames.Pages_Postal_Create, L("PostalsCreate"));
            context.CreatePermission(PermissionNames.Pages_Postal_Edit, L("PostalsEdit"));
            context.CreatePermission(PermissionNames.Pages_Postal_Delete, L("PostalsDelete"));

            context.CreatePermission(PermissionNames.Pages_RateItem, L("RateItems"));
            context.CreatePermission(PermissionNames.Pages_RateItem_Create, L("RateItemsCreate"));
            context.CreatePermission(PermissionNames.Pages_RateItem_Edit, L("RateItemsEdit"));
            context.CreatePermission(PermissionNames.Pages_RateItem_Delete, L("RateItemsDelete"));

            context.CreatePermission(PermissionNames.Pages_Rate, L("Rates"));
            context.CreatePermission(PermissionNames.Pages_Rate_Create, L("RatesCreate"));
            context.CreatePermission(PermissionNames.Pages_Rate_Edit, L("RatesEdit"));
            context.CreatePermission(PermissionNames.Pages_Rate_Delete, L("RatesDelete"));

            context.CreatePermission(PermissionNames.Pages_Wallet, L("Wallets"));
            context.CreatePermission(PermissionNames.Pages_Wallet_Create, L("WalletsCreate"));
            context.CreatePermission(PermissionNames.Pages_Wallet_Edit, L("WalletsEdit"));
            context.CreatePermission(PermissionNames.Pages_Wallet_Delete, L("WalletsDelete"));

            context.CreatePermission(PermissionNames.Pages_Currency, L("Currencies"));
            context.CreatePermission(PermissionNames.Pages_Currency_Create, L("CurrenciesCreate"));
            context.CreatePermission(PermissionNames.Pages_Currency_Edit, L("CurrenciesEdit"));
            context.CreatePermission(PermissionNames.Pages_Currency_Delete, L("CurrenciesDelete"));

            context.CreatePermission(PermissionNames.Pages_EWalletType, L("EWalletTypes"));
            context.CreatePermission(PermissionNames.Pages_EWalletType_Create, L("EWalletTypesCreate"));
            context.CreatePermission(PermissionNames.Pages_EWalletType_Edit, L("EWalletTypesEdit"));
            context.CreatePermission(PermissionNames.Pages_EWalletType_Delete, L("EWalletTypesDelete"));

            context.CreatePermission(PermissionNames.Pages_Customer, L("Customers"));
            context.CreatePermission(PermissionNames.Pages_Customer_Create, L("CustomersCreate"));
            context.CreatePermission(PermissionNames.Pages_Customer_Edit, L("CustomersEdit"));
            context.CreatePermission(PermissionNames.Pages_Customer_Delete, L("CustomersDelete"));

            context.CreatePermission(PermissionNames.Pages_CustomerTransaction, L("CustomerTransactions"));
            context.CreatePermission(PermissionNames.Pages_CustomerTransaction_Create, L("CustomerTransactionsCreate"));
            context.CreatePermission(PermissionNames.Pages_CustomerTransaction_Edit, L("CustomerTransactionsEdit"));
            context.CreatePermission(PermissionNames.Pages_CustomerTransaction_Delete, L("CustomerTransactionsDelete"));



        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SMPConsts.LocalizationSourceName);
        }
    }
}
