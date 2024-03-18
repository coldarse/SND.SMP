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
import { CustomerPostalsComponent } from './customerpostals/customerpostals.component';
import { PostalCountriesComponent } from "./postalcountries/postalcountries.component";
import { PostalsComponent } from "./postals/postals.component";
import { CurrenciesComponent } from "./currencies/currencies.component";
import { EWalletTypesComponent } from "./ewallettypes/ewallettypes.component";
import { CustomersComponent } from "./customers/customers.component";
import { WalletsComponent } from "./wallets/wallets.component";
import { RateItemsComponent } from "./rate-items/rate-items.component";
import { CustomerTransactionsComponent } from "./customer-transactions/customer-transactions.component";

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
          {
            path: "postalcountries",
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
            path: "customertransactions",
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
