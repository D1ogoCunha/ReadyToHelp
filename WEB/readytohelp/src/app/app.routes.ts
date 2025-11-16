import { Routes } from '@angular/router';
import { MapComponent } from './pages/map/map.component';

export const appRoutes: Routes = [
    // main route
    { 
        path: '', 
        component: MapComponent 
    },
    // Safety: redirect any unknown paths to the main route
    { 
        path: '**', 
        redirectTo: '', 
        pathMatch: 'full' 
    }
];