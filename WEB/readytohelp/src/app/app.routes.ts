import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login.component';
import { MapComponent } from './pages/map/map.component';
import { AuthGuard } from './guards/auth.guard';

export const appRoutes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: MapComponent },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];

