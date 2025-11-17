import { Routes } from '@angular/router';
import { MapComponent } from './pages/map/map.component';
import { OccurrenceDetailComponent } from './pages/occurence-detail/occurence-detail.component'; 

export const appRoutes: Routes = [
    // Route 1: Change homepage to redirect to /map
    {
        path: '',
        redirectTo: '/map', // Redirect from empty path
        pathMatch: 'full'
    },
    // Route 2: The map page (now at /map)
    { 
        path: 'map', 
        component: MapComponent 
    },
    // Route 3: The details page
    {
        path: 'occurrence/:id',
        component: OccurrenceDetailComponent
    },
    // Route 4: Wildcard (must be last)
    { 
        path: '**', 
        redirectTo: '/map',
        pathMatch: 'full' 
    }
];