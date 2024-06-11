import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { AppComponent } from "./app.component";
import { AppRouteGuard } from "@shared/auth/auth-route-guard";
import { HomeComponent } from "./home/home.component";
import { AboutComponent } from "./about/about.component";
import { UsersComponent } from "./users/users.component";
import { TenantsComponent } from "./tenants/tenants.component";
import { RolesComponent } from "app/roles/roles.component";
import { ChangePasswordComponent } from "./users/change-password/change-password.component";
/* Insert Routing Import */
import { AirportsComponent } from './airports/airports.component';
import { ItemTrackingApplicationsComponent } from "./item-tracking-applications/item-tracking-application.component";
import { IMPCSComponent } from "./impcs/impcs.component";
import { RefundsComponent } from "./refunds/refunds.component";
import { WeightAdjustmentsComponent } from "./weightadjustments/weightadjustments.component";
import { ApplicationSettingsComponent } from "./applicationsettings/applicationsettings.component";
import { ItemMinsComponent } from "./item-mins/item-mins.component";
import { ItemsComponent } from "./items/items.component";
import { BagsComponent } from "./bags/bags.component";
import { DispatchValidationsComponent } from "./dispatch-validations/dispatch-validations.component";
import { DispatchesComponent } from "./dispatches/dispatches.component";
import { QueuesComponent } from "./queues/queues.component";
import { RateWeightBreaksComponent } from "./rate-weight-breaks/rate-weight-breaks.component";
import { CustomerPostalsComponent } from "./customer-postals/customer-postals.component";
import { PostalCountriesComponent } from "./postal-countries/postal-countries.component";
import { PostalsComponent } from "./postals/postals.component";
import { CurrenciesComponent } from "./currencies/currencies.component";
import { EWalletTypesComponent } from "./ewallettypes/ewallettypes.component";
import { CustomersComponent } from "./customers/customers.component";
import { WalletsComponent } from "./wallets/wallets.component";
import { RateItemsComponent } from "./rate-items/rate-items.component";
import { CustomerTransactionsComponent } from "./customer-transactions/customer-transactions.component";
import { PreAlertComponent } from "./pre-alerts/pre-alerts.component";
import { PostChecksComponent } from "./post-checks/post-checks.component";

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: "",
        component: AppComponent,
        children: [
          {
            path: "home",
            component: HomeComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "users",
            component: UsersComponent,
            data: { permission: "Pages.Users" },
            canActivate: [AppRouteGuard],
          },
          {
            path: "roles",
            component: RolesComponent,
            data: { permission: "Pages.Roles" },
            canActivate: [AppRouteGuard],
          },
          {
            path: "tenants",
            component: TenantsComponent,
            data: { permission: "Pages.Tenants" },
            canActivate: [AppRouteGuard],
          },
          // { path: 'about', component: AboutComponent, canActivate: [AppRouteGuard] },
          {
            path: "update-password",
            component: ChangePasswordComponent,
            canActivate: [AppRouteGuard],
          },
          /* Insert Path */
                    { path: 'airports', data: { permission: 'Pages.Airport' }, component: AirportsComponent, canActivate: [AppRouteGuard] },
          {
            path: "item-tracking-applications",
            data: { permission: "Pages.ItemTrackingApplication" },
            component: ItemTrackingApplicationsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "impcs",
            data: { permission: "Pages.IMPC" },
            component: IMPCSComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "refunds",
            data: { permission: "Pages.Refund" },
            component: RefundsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "weight-adjustments",
            data: { permission: "Pages.WeightAdjustment" },
            component: WeightAdjustmentsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "postchecks/:dispatchNo",
            data: { permission: "Pages.Users" },
            component: PostChecksComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "applicationsettings",
            data: { permission: "Pages.ApplicationSetting" },
            component: ApplicationSettingsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "itemmins",
            data: { permission: "Pages.ItemMin" },
            component: ItemMinsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "items",
            data: { permission: "Pages.Item" },
            component: ItemsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "bags",
            data: { permission: "Pages.Bag" },
            component: BagsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "dispatch-validations",
            data: { permission: "Pages.DispatchValidation" },
            component: DispatchValidationsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "dispatches",
            data: { permission: "Pages.Dispatch" },
            component: DispatchesComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "queues",
            data: { permission: "Pages.Queue" },
            component: QueuesComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "pre-alerts",
            component: PreAlertComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "rate-weight-breaks",
            data: { permission: "Pages.RateWeightBreak" },
            component: RateWeightBreaksComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "customer-postals",
            data: { permission: "Pages.CustomerPostal" },
            component: CustomerPostalsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "postal-countries",
            data: { permission: "Pages.PostalCountry" },
            component: PostalCountriesComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "postals",
            data: { permission: "Pages.Postal" },
            component: PostalsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "currencies",
            data: { permission: "Pages.Currency" },
            component: CurrenciesComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "ewallettypes",
            data: { permission: "Pages.EWalletType" },
            component: EWalletTypesComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "customers",
            data: { permission: "Pages.Customer" },
            component: CustomersComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "wallets",
            data: { permission: "Pages.Wallet" },
            component: WalletsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "rate-items",
            data: { permission: "Pages.RateItem" },
            component: RateItemsComponent,
            canActivate: [AppRouteGuard],
          },
          {
            path: "customer-transactions",
            data: { permission: "Pages.CustomerTransaction" },
            component: CustomerTransactionsComponent,
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ]),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
