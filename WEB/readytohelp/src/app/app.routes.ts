import { Routes } from '@angular/router';
import { MapComponent } from './pages/map/map.component';
import { OccurrenceDetailComponent } from './pages/occurence-detail/occurence-detail.component';

export const appRoutes: Routes = [
  // Route 1: The map (homepage)
  {
    path: '',
    component: MapComponent,
  },
  // Route 2: The details page
  // This now correctly points to your new component
  {
    path: 'occurrence/:id',
    component: OccurrenceDetailComponent,
  },
  // Route 3: Wildcard (must be last)
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full',
  },
];
