import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Home } from './components/home/home';
import { Basket } from './components/basket/basket';
import { Checkout } from './components/checkout/checkout';
import { MyOrders } from './components/my-orders/my-orders';
import { OrderDetails } from './components/order-details/order-details';

export const routes: Routes = [
    { path: "", pathMatch: "full", redirectTo: "home" }, //Anasayfa rotası
    {path: "home", component: Home}, //Home rotası
    { path: "login", component: Login }, //login rotası
    {path: "basket", component: Basket},//Sepet rotası
    {path: "checkout", component: Checkout}, //Ödeme rotası
    {path: "my-orders", component: MyOrders}, //Siparişlerim rotası  
    {path: "order-details/:id", component: OrderDetails} //Sipariş Detayları rotası
];
