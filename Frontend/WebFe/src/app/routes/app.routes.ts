import { Routes } from '@angular/router';

import { languageSwitchGuard } from '../core/i18n/language-switch.guard';
import { LangRedirectComponent } from '../core/i18n/lang-redirect.component';
import { authGuard } from '../guards/auth.guard';
import { guestGuard } from '../guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'register',
    pathMatch: 'full'
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('../pages/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('../pages/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'verify-otp',
    canActivate: [guestGuard],
    loadComponent: () => import('../pages/verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent)
  },
  {
    path: 'home',
    canActivate: [authGuard],
    loadComponent: () => import('../pages/home/home.component').then((m) => m.HomeComponent)
  },
  {
    path: 'profile/:username',
    canActivate: [authGuard],
    loadComponent: () => import('../pages/profile/profile.component').then((m) => m.ProfileComponent)
  },
  {
    path: 'vn',
    canActivate: [languageSwitchGuard],
    data: { lang: 'vi' },
    component: LangRedirectComponent
  },
  {
    path: 'en',
    canActivate: [languageSwitchGuard],
    data: { lang: 'en' },
    component: LangRedirectComponent
  }
];
