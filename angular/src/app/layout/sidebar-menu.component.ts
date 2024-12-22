import { Component, Injector, OnInit } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import {
  Router,
  RouterEvent,
  NavigationEnd,
  PRIMARY_OUTLET,
} from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { filter } from "rxjs/operators";
import { MenuItem } from "@shared/layout/menu-item";

@Component({
  selector: "sidebar-menu",
  templateUrl: "./sidebar-menu.component.html",
})
export class SidebarMenuComponent extends AppComponentBase implements OnInit {
  menuItems: MenuItem[];
  menuItemsMap: { [key: number]: MenuItem } = {};
  activatedMenuItems: MenuItem[] = [];
  routerEvents: BehaviorSubject<RouterEvent> = new BehaviorSubject(undefined);
  homeRoute = "/app/home";

  constructor(injector: Injector, private router: Router) {
    super(injector);
  }

  ngOnInit(): void {
    this.menuItems = this.getMenuItems();
    this.patchMenuItems(this.menuItems);

    const currentUrl =
      this.router.url !== "/" || this.router.url != undefined
        ? this.router.url
        : this.homeRoute;
    const primaryUrlSegmentGroup =
      this.router.parseUrl(currentUrl).root.children[PRIMARY_OUTLET];
    if (primaryUrlSegmentGroup) {
      this.activateMenuItems("/" + primaryUrlSegmentGroup.toString());
    }

    this.router.events.subscribe((event: NavigationEnd) => {
      console.log(event.url);
      const currentUrl =
        event.url !== "/" || event.url != undefined
          ? event.url
          : this.homeRoute;
      const primaryUrlSegmentGroup =
        this.router.parseUrl(currentUrl).root.children[PRIMARY_OUTLET];
      if (primaryUrlSegmentGroup) {
        this.activateMenuItems("/" + primaryUrlSegmentGroup.toString());
      }
    });
  }

  getMenuItems(): MenuItem[] {
    return [
      new MenuItem(this.l("HomePage"), "/app/home", "fas fa-home"),
      new MenuItem(
        this.l("Postals"),
        "/app/postals",
        "fas fa-parachute-box",
        "Pages.Postal"
      ),
      new MenuItem(
        this.l("Postal Countries"),
        "/app/postal-countries",
        "fas fa-earth-asia",
        "Pages.PostalCountry"
      ),
      new MenuItem(
        this.l("Request Responses"),
        "/app/request-responses",
        "fas fa-exchange-alt",
        "Pages.APIRequestResponse"
      ),
      new MenuItem(
        this.l("Queues"),
        "/app/queues",
        "fas fa-arrow-down-wide-short",
        "Pages.Queue"
      ),
      new MenuItem(
        this.l("Pre-Check Upload"),
        "/app/pre-alerts",
        "fas fa-bell"
      ),
      new MenuItem(this.l("Dispatch"), "", "fas fa-box", "Pages.Dispatch", [
        new MenuItem(
          this.l("Dispatch Validations"),
          "/app/dispatch-validations",
          "fas fa-tasks",
          "Pages.DispatchValidation"
        ),
        new MenuItem(
          this.l("Dispatches"),
          "/app/dispatches",
          "fas fa-boxes-packing",
          "Pages.Dispatch"
        ),
        // new MenuItem(
        //   this.l("Dispatch Tracking"),
        //   "/app/dispatch-tracking",
        //   "fas fa-thumbtack",
        //   "Pages.Dispatch"
        // )
      ]),
      new MenuItem(this.l("Items"), "", "fas fa-cubes", "Pages.Item", [
        new MenuItem(
          this.l("API Item ID"),
          "/app/api-item-ids",
          "fas fa-compress-alt",
          "Pages.APIItem"
        ),
        new MenuItem(
          this.l("Generate Item ID"),
          "/app/item-tracking-applications",
          "fas fa-barcode",
          "Pages.ItemTrackingApplication"
        ),
        new MenuItem(
          this.l("Search Item"),
          "/app/search-item",
          "fas fa-binoculars",
          "Pages.SearchItem"
        )
      ]),
      /* Insert Menu Path */
      new MenuItem(this.l("Customers"), "", "fas fa-user", "Pages.Customer", [
        new MenuItem(
          this.l("Info"),
          "/app/customers",
          "fas fa-info",
          "Pages.Customer"
        ),
        new MenuItem(
          this.l("Wallets"),
          "/app/wallets",
          "fas fa-wallet",
          "Pages.Wallet"
        ),
        new MenuItem(
          this.l("Transactions"),
          "/app/customer-transactions",
          "fas fa-square-poll-horizontal",
          "Pages.CustomerTransaction"
        ),
      ]),
      new MenuItem(
        this.l("Rate Maintenance"),
        "",
        "fas fa-percentage",
        "Pages.Rate",
        [
          new MenuItem(
            this.l("TS Rates"),
            "/app/ts-rates",
            "fas fa-angle-right",
            "Pages.RateItem"
          ),
          new MenuItem(
            this.l("DE Rates"),
            "/app/de-rates",
            "fa fa-angle-right",
            "Pages.RateWeightBreak"
          ),
        ]
      ),
      new MenuItem(this.l("System"), "", "fas fa-desktop", "Pages.System", [
        new MenuItem(
          this.l("Application Settings"),
          "/app/applicationsettings",
          "fas fa-toolbox",
          "Pages.ApplicationSetting"
        ),
        new MenuItem(
          this.l("Currencies"),
          "/app/currencies",
          "fas fa-dollar-sign",
          "Pages.Currency"
        ),
        new MenuItem(
          this.l("Airports"),
          "/app/airports",
          "fas fa-plane-arrival",
          "Pages.Airport"
        ),
        new MenuItem(
          this.l("IMPCS"),
          "/app/impcs",
          "fas fa-magnifying-glass-location",
          "Pages.IMPC"
        ),
        new MenuItem(
          this.l("EWalletTypes"),
          "/app/ewallettypes",
          "fas fa-comment-dollar",
          "Pages.EWalletType"
        ),
        new MenuItem(
          this.l("Email Contents"),
          "/app/emailcontents",
          "fas fa-mail-bulk",
          "Pages.EmailContent"
        ),
        new MenuItem(
          this.l("RateZones"),
          "/app/ratezones",
          "fas fa-warehouse",
          "Pages.RateZone"
        ),
      ]),
      new MenuItem(
        this.l("Administration"),
        "",
        "fas fa-user-shield",
        "Pages.Administration",
        [
          new MenuItem(
            this.l("Roles"),
            "/app/roles",
            "fas fa-theater-masks",
            "Pages.RolesPage"
          ),
          new MenuItem(
            this.l("Tenants"),
            "/app/tenants",
            "fas fa-building",
            "Pages.Tenants"
          ),
          new MenuItem(
            this.l("Users"),
            "/app/users",
            "fas fa-users",
            "Pages.UsersPage"
          ),
        ]
      ),
      // new MenuItem(
      //   this.l("Refunds"),
      //   "/app/refunds",
      //   "far fa-circle",
      //   "Pages.Refund"
      // ),
      // new MenuItem(
      //   this.l("WeightAdjustments"),
      //   "/app/weightadjustments",
      //   "far fa-circle",
      //   "Pages.WeightAdjustment"
      // ),

      // new MenuItem(
      //   this.l("ItemMins"),
      //   "/app/itemmins",
      //   "far fa-circle",
      //   "Pages.ItemMin"
      // ),
      // new MenuItem(
      //   this.l("Items"),
      //   "/app/items",
      //   "far fa-circle",
      //   "Pages.Item"
      // ),
      // new MenuItem(this.l("Bags"), "/app/bags", "far fa-circle", "Pages.Bag"),
      // new MenuItem(
      //   this.l("Queues"),
      //   "/app/queues",
      //   "fas fa-arrow-down-wide-short",
      //   "Pages.Queue"
      // ),
    ];
  }

  patchMenuItems(items: MenuItem[], parentId?: number): void {
    items.forEach((item: MenuItem, index: number) => {
      item.id = parentId ? Number(parentId + "" + (index + 1)) : index + 1;
      if (parentId) {
        item.parentId = parentId;
      }
      if (parentId || item.children) {
        this.menuItemsMap[item.id] = item;
      }
      if (item.children) {
        this.patchMenuItems(item.children, item.id);
      }
    });
  }

  activateMenuItems(url: string): void {
    this.deactivateMenuItems(this.menuItems);
    this.activatedMenuItems = [];
    const foundedItems = this.findMenuItemsByUrl(url, this.menuItems);
    foundedItems.forEach((item) => {
      this.activateMenuItem(item);
    });
  }

  deactivateMenuItems(items: MenuItem[]): void {
    items.forEach((item: MenuItem) => {
      item.isActive = false;
      item.isCollapsed = true;
      if (item.children) {
        this.deactivateMenuItems(item.children);
      }
    });
  }

  findMenuItemsByUrl(
    url: string,
    items: MenuItem[],
    foundedItems: MenuItem[] = []
  ): MenuItem[] {
    items.forEach((item: MenuItem) => {
      if (item.route === url) {
        foundedItems.push(item);
      } else if (item.children) {
        this.findMenuItemsByUrl(url, item.children, foundedItems);
      }
    });
    return foundedItems;
  }

  activateMenuItem(item: MenuItem): void {
    item.isActive = true;
    if (item.children) {
      item.isCollapsed = false;
    }
    this.activatedMenuItems.push(item);
    if (item.parentId) {
      this.activateMenuItem(this.menuItemsMap[item.parentId]);
    }
  }

  isMenuItemVisible(item: MenuItem): boolean {
    if (!item.permissionName) {
      return true;
    }
    return this.permission.isGranted(item.permissionName);
  }
}
