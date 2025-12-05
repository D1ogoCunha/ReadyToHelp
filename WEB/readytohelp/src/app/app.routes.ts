import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login.component';
import { MapComponent } from './components/map/map.component';
import { OccurrenceDetailComponent } from './components/occurence-detail/occurence-detail.component';
import { AuthGuard } from './guards/auth.guard';
import { OccurrencesHistoryComponent } from './components/occurrences-history/occurrences-history.component';
import { UserManagementComponent } from './components/user-management/user-management.component';

export const appRoutes: Routes = [
  // The login page
  {
    path: 'login',
    component: LoginComponent,
  },
  // Change homepage to redirect to /map
  {
    path: '',
    redirectTo: '/map',
    pathMatch: 'full',
  },
  // The map page
  {
    path: 'map',
    component: MapComponent,
  },
  // The details page
  {
    path: 'occurrence/:id',
    component: OccurrenceDetailComponent,
  },
  // The occurrences history page
  {
    path: 'occurrences/history',
    component: OccurrencesHistoryComponent,
  },
  // The user management page
  {
    path: 'users',
    component: UserManagementComponent,
    canActivate: [AuthGuard],
  },
  // Wildcard
  {
    path: '**',
    redirectTo: '/map',
    pathMatch: 'full',
  },
];
