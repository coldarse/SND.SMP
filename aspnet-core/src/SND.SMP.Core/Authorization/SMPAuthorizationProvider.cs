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


        
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SMPConsts.LocalizationSourceName);
        }
    }
}
