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
      this.router.url !== "/" || this.router.url != undefined ? this.router.url : this.homeRoute;
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
      // new MenuItem(this.l('About'), '/app/about', 'fas fa-info-circle'),
      new MenuItem(this.l("HomePage"), "/app/home", "fas fa-home"),
      new MenuItem(
        this.l("Roles"),
        "/app/roles",
        "fas fa-theater-masks",
        "Pages.Roles"
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
        "Pages.Users"
      ),
      /* Insert Menu Path */
            new MenuItem(
                this.l('ApplicationSettings'),
                '/app/applicationsettings',
                'far fa-circle',
                'Pages.ApplicationSetting'
            ),
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
      new MenuItem(
        this.l("Dispatch Validations"),
        "/app/dispatch-validations",
        "fas fa-tasks",
        "Pages.DispatchValidation"
      ),
      // new MenuItem(
      //   this.l("Dispatches"),
      //   "/app/dispatches",
      //   "far fa-circle",
      //   "Pages.Dispatch"
      // ),
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
      new MenuItem(
        this.l("Rate Weight Breaks"),
        "/app/rate-weight-breaks",
        "fas fa-scale-unbalanced",
        "Pages.RateWeightBreak"
      ),
      new MenuItem(
        this.l("Postal Countries"),
        "/app/postal-countries",
        "fas fa-earth-asia",
        "Pages.PostalCountry"
      ),
      new MenuItem(
        this.l("Postals"),
        "/app/postals",
        "fas fa-parachute-box",
        "Pages.Postal"
      ),
      new MenuItem(
        this.l("Currencies"),
        "/app/currencies",
        "fas fa-dollar-sign",
        "Pages.Currency"
      ),
      new MenuItem(
        this.l("EWalletTypes"),
        "/app/ewallettypes",
        "fas fa-comment-dollar",
        "Pages.EWalletType"
      ),
      new MenuItem(
        this.l("Customers"),
        "/app/customers",
        "fas fa-users",
        "Pages.Customer"
      ),
      new MenuItem(
        this.l("Wallets"),
        "/app/wallets",
        "fas fa-wallet",
        "Pages.Wallet"
      ),
      new MenuItem(
        this.l("Rate Items"),
        "/app/rate-items",
        "fas fa-sitemap",
        "Pages.RateItem"
      ),
      new MenuItem(
        this.l("Transactions"),
        "/app/customer-transactions",
        "fas fa-square-poll-horizontal",
        "Pages.CustomerTransaction"
      ),
      // new MenuItem(this.l('MultiLevelMenu'), '', 'fas fa-circle', '', [
      //     new MenuItem('ASP.NET Boilerplate', '', 'fas fa-dot-circle', '', [
      //         new MenuItem(
      //             'Home',
      //             'https://aspnetboilerplate.com?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Templates',
      //             'https://aspnetboilerplate.com/Templates?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Samples',
      //             'https://aspnetboilerplate.com/Samples?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Documents',
      //             'https://aspnetboilerplate.com/Pages/Documents?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //     ]),
      //     new MenuItem('ASP.NET Zero', '', 'fas fa-dot-circle', '', [
      //         new MenuItem(
      //             'Home',
      //             'https://aspnetzero.com?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Features',
      //             'https://aspnetzero.com/Features?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Pricing',
      //             'https://aspnetzero.com/Pricing?ref=abptmpl#pricing',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Faq',
      //             'https://aspnetzero.com/Faq?ref=abptmpl',
      //             'far fa-circle'
      //         ),
      //         new MenuItem(
      //             'Documents',
      //             'https://aspnetzero.com/Documents?ref=abptmpl',
      //             'far fa-circle'
      //         )
      //     ])
      // ])
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
