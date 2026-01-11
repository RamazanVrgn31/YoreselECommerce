import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Home } from './components/home/home';
import { Basket } from './components/basket/basket';

export const routes: Routes = [
    { path: "", pathMatch: "full", redirectTo: "home" }, //Anasayfa rotası
    {path: "home", component: Home}, //Home rotası
    { path: "login", component: Login }, //login rotası
    {path: "basket", component: Basket}, //basket rotası
];
