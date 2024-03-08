import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientJsonpModule } from "@angular/common/http";
import { HttpClientModule } from "@angular/common/http";
import { ModalModule } from "ngx-bootstrap/modal";
import { BsDropdownModule } from "ngx-bootstrap/dropdown";
import { CollapseModule } from "ngx-bootstrap/collapse";
import { TabsModule } from "ngx-bootstrap/tabs";
import { NgxPaginationModule } from "ngx-pagination";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { ServiceProxyModule } from "@shared/service-proxies/service-proxy.module";
import { SharedModule } from "@shared/shared.module";
import { HomeComponent } from "@app/home/home.component";
import { AboutComponent } from "@app/about/about.component";
// tenants
import { TenantsComponent } from "@app/tenants/tenants.component";
import { CreateTenantDialogComponent } from "./tenants/create-tenant/create-tenant-dialog.component";
import { EditTenantDialogComponent } from "./tenants/edit-tenant/edit-tenant-dialog.component";
// roles
import { RolesComponent } from "@app/roles/roles.component";
import { CreateRoleDialogComponent } from "./roles/create-role/create-role-dialog.component";
import { EditRoleDialogComponent } from "./roles/edit-role/edit-role-dialog.component";
// users
import { UsersComponent } from "@app/users/users.component";
import { CreateUserDialogComponent } from "@app/users/create-user/create-user-dialog.component";
import { EditUserDialogComponent } from "@app/users/edit-user/edit-user-dialog.component";
import { ChangePasswordComponent } from "./users/change-password/change-password.component";
import { ResetPasswordDialogComponent } from "./users/reset-password/reset-password.component";
// layout
import { HeaderComponent } from "./layout/header.component";
import { HeaderLeftNavbarComponent } from "./layout/header-left-navbar.component";
import { HeaderLanguageMenuComponent } from "./layout/header-language-menu.component";
import { HeaderUserMenuComponent } from "./layout/header-user-menu.component";
import { FooterComponent } from "./layout/footer.component";
import { SidebarComponent } from "./layout/sidebar.component";
import { SidebarLogoComponent } from "./layout/sidebar-logo.component";
import { SidebarUserPanelComponent } from "./layout/sidebar-user-panel.component";
import { SidebarMenuComponent } from "./layout/sidebar-menu.component";
/* Insert Import */
import { PostalCountriesComponent } from "./postalcountries/postalcountries.component";
import { CreateUpdatePostalCountryComponent } from "./postalcountries/create-update-postalcountry/create-update-postalcountry.component";
import { PostalCountryService } from "@shared/service-proxies/postalcountries/postalcountry.service";

import { PostalsComponent } from "./postals/postals.component";
import { CreateUpdatePostalComponent } from "./postals/create-update-postal/create-update-postal.component";
import { PostalService } from "@shared/service-proxies/postals/postal.service";

import { CurrenciesComponent } from "./currencies/currencies.component";
import { CreateUpdateCurrencyComponent } from "./currencies/create-update-currency/create-update-currency.component";
import { CurrencyService } from "@shared/service-proxies/currencies/currency.service";

import { EWalletTypesComponent } from "./ewallettypes/ewallettypes.component";
import { CreateUpdateEWalletTypeComponent } from "./ewallettypes/create-update-ewallettype/create-update-ewallettype.component";
import { EWalletTypeService } from "@shared/service-proxies/ewallettypes/ewallettype.service";

import { CustomersComponent } from "./customers/customers.component";
import { CreateUpdateCustomerComponent } from "./customers/create-update-customer/create-update-customer.component";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";

import { WalletsComponent } from "./wallets/wallets.component";
import { CreateUpdateWalletComponent } from "./wallets/create-update-wallet/create-update-wallet.component";
import { WalletService } from "@shared/service-proxies/wallets/wallet.service";

import { RateItemsComponent } from "./rate-items/rate-items.component";
import { RateItemService } from "@shared/service-proxies/rate-items/rate-item.service";
import { UploadRateItemComponent } from "./rate-items/upload-rate-item/upload-rate-item.component";
import { RateService } from "@shared/service-proxies/rates/rate.service";

import { CustomerTransactionsComponent } from "./customer-transactions/customer-transactions.component";
import { CreateUpdateCustomerTransactionComponent } from "./customer-transactions/create-update-customer-transaction/create-update-customer-transaction.component";
import { CustomerTransactionService } from "@shared/service-proxies/customer-transactions/customer-transaction.service";

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    AboutComponent,
    // tenants
    TenantsComponent,
    CreateTenantDialogComponent,
    EditTenantDialogComponent,
    // roles
    RolesComponent,
    CreateRoleDialogComponent,
    EditRoleDialogComponent,
    // users
    UsersComponent,
    CreateUserDialogComponent,
    EditUserDialogComponent,
    ChangePasswordComponent,
    ResetPasswordDialogComponent,
    // layout
    HeaderComponent,
    HeaderLeftNavbarComponent,
    HeaderLanguageMenuComponent,
    HeaderUserMenuComponent,
    FooterComponent,
    SidebarComponent,
    SidebarLogoComponent,
    SidebarUserPanelComponent,
    SidebarMenuComponent,
    /* Insert Component */
    PostalCountriesComponent,
    CreateUpdatePostalCountryComponent,
    PostalsComponent,
    CreateUpdatePostalComponent,
    CurrenciesComponent,
    CreateUpdateCurrencyComponent,
    EWalletTypesComponent,
    CreateUpdateEWalletTypeComponent,
    CustomersComponent,
    CreateUpdateCustomerComponent,
    WalletsComponent,
    CreateUpdateWalletComponent,
    RateItemsComponent,
    UploadRateItemComponent,
    CustomerTransactionsComponent,
    CreateUpdateCustomerTransactionComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    HttpClientJsonpModule,
    ModalModule.forChild(),
    BsDropdownModule,
    CollapseModule,
    TabsModule,
    AppRoutingModule,
    ServiceProxyModule,
    SharedModule,
    NgxPaginationModule,
  ],
  providers: [
    /* Insert Service */
    PostalCountryService,
    PostalService,
    CurrencyService,
    EWalletTypeService,
    CustomerService,
    WalletService,
    RateItemService,
    RateService,
    CustomerTransactionService,
  ],
})
export class AppModule {}
