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
import { ApplicationSettingsComponent } from './applicationsettings/applicationsettings.component';
import { CreateUpdateApplicationSettingComponent } from './applicationsettings/create-update-applicationsetting/create-update-applicationsetting.component';
import { ApplicationSettingService } from '@shared/service-proxies/applicationsettings/applicationsetting.service';

import { ItemMinsComponent } from "./item-mins/item-mins.component";
import { CreateUpdateItemMinComponent } from "./item-mins/create-update-item-min/create-update-item-min.component";
import { ItemMinService } from "@shared/service-proxies/item-mins/item-min.service";

import { ItemsComponent } from "./items/items.component";
import { CreateUpdateItemComponent } from "./items/create-update-item/create-update-item.component";
import { ItemService } from "@shared/service-proxies/items/item.service";

import { BagsComponent } from "./bags/bags.component";
import { CreateUpdateBagComponent } from "./bags/create-update-bag/create-update-bag.component";
import { BagService } from "@shared/service-proxies/bags/bag.service";

import { DispatchValidationsComponent } from "./dispatch-validations/dispatch-validations.component";
import { CreateUpdateDispatchValidationComponent } from "./dispatch-validations/create-update-dispatchvalidation/create-update-dispatch-validation.component";
import { DispatchValidationErrorComponent } from "./dispatch-validations/dipatch-validation-error/dispatch-validation-error.component";
import { DispatchValidationService } from "@shared/service-proxies/dispatch-validations/dispatch-validation.service";

import { DispatchesComponent } from "./dispatches/dispatches.component";
import { CreateUpdateDispatchComponent } from "./dispatches/create-update-dispatch/create-update-dispatch.component";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";

import { QueuesComponent } from "./queues/queues.component";
import { CreateUpdateQueueComponent } from "./queues/create-update-queue/create-update-queue.component";
import { QueueService } from "@shared/service-proxies/queues/queue.service";

import { PreAlertComponent } from "./pre-alerts/pre-alerts.component";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";

import { RateWeightBreaksComponent } from "./rate-weight-breaks/rate-weight-breaks.component";
import { UploadRateWeightBreakComponent } from "./rate-weight-breaks/upload-rate-weight-break/upload-rate-weight-break.component";
import { RateWeightBreakService } from "@shared/service-proxies/rate-weight-breaks/rate-weight-break.service";

import { CustomerPostalsComponent } from "./customer-postals/customer-postals.component";
import { CreateUpdateCustomerPostalComponent } from "./customer-postals/create-update-customer-postal/create-update-customer-postal.component";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";

import { PostalCountriesComponent } from "./postal-countries/postal-countries.component";
import { PostalCountryService } from "@shared/service-proxies/postal-countries/postal-country.service";

import { PostalsComponent } from "./postals/postals.component";
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

import { UploadPostalComponent } from "./postals/upload-postal/upload-postal.component";

import { UploadPostalCountryComponent } from "./postal-countries/upload-postal-country/upload-postal-country.component";

import { TopUpWalletComponent } from "./wallets/topup-wallet/topup-wallet.component";

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
        ApplicationSettingsComponent,
        CreateUpdateApplicationSettingComponent,
    ItemMinsComponent,
    CreateUpdateItemMinComponent,
    ItemsComponent,
    CreateUpdateItemComponent,
    BagsComponent,
    CreateUpdateBagComponent,
    DispatchValidationsComponent,
    CreateUpdateDispatchValidationComponent,
    DispatchesComponent,
    CreateUpdateDispatchComponent,
    QueuesComponent,
    CreateUpdateQueueComponent,
    PreAlertComponent,
    CustomerPostalsComponent,
    CreateUpdateCustomerPostalComponent,
    PostalCountriesComponent,
    PostalsComponent,
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
    UploadPostalComponent,
    UploadPostalCountryComponent,
    TopUpWalletComponent,
    RateWeightBreaksComponent,
    UploadRateWeightBreakComponent,
    DispatchValidationErrorComponent,
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
        ApplicationSettingService,
    ItemMinService,
    ItemService,
    BagService,
    DispatchValidationService,
    DispatchService,
    ChibiService,
    CustomerPostalService,
    PostalCountryService,
    PostalService,
    CurrencyService,
    EWalletTypeService,
    CustomerService,
    WalletService,
    RateItemService,
    RateService,
    CustomerTransactionService,
    RateWeightBreakService,
    QueueService,
  ],
})
export class AppModule {}
