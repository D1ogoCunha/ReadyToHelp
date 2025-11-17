import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import * as mapboxgl from 'mapbox-gl';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceMap } from '../../models/occurrenceMap.model';
import { OccurrenceType } from '../../models/occurrence-type.enum';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { PriorityLevel } from '../../models/priority-level.enum';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapComponent implements OnInit {
  private map?: mapboxgl.Map;
  private occurrenceService = inject(OccurrenceService);
  private router = inject(Router);

  constructor() {}

  ngOnInit(): void {
    this.map = new mapboxgl.Map({
      accessToken:
        'pk.eyJ1IjoidG1zMjYiLCJhIjoiY21pMXk3MW9qMTVnZjJqc2ZkMDVmbGF0NCJ9.ud2aOuGC2KH9YyNbJJM8Yg', // <-- ADICIONADO AQUI
      container: 'map',
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-9.1393, 38.7223],
      zoom: 6,
    });

    this.map.addControl(new mapboxgl.NavigationControl());

    this.map.on('load', () => {
      this.loadOccurrences();
    });
  }

  /**
   * Vai buscar as ocorrências à API e adiciona-as ao mapa
   */
  private loadOccurrences(): void {
    this.occurrenceService.getActiveOccurrences().subscribe({
      next: (occurrences) => {
        console.log(`Carregadas ${occurrences.length} ocorrências`);
        this.addMarkersToMap(occurrences);
      },
      error: (err) => {
        console.error('Error loading occurrences', err);
      },
    });
  }

  private addMarkersToMap(occurrences: OccurrenceMap[]): void {
    if (!this.map) return;

    for (const occ of occurrences) {
      // Create the main container for the popup
      const popupContent = document.createElement('div');
      popupContent.className = 'w-64 p-3 space-y-2';

      // Title
      const title = document.createElement('h3');
      title.className = 'font-bold text-lg text-blue-600';
      title.innerText = occ.title;

      // Type
      const type = document.createElement('p');
      type.className = 'text-sm text-gray-800';
      type.innerHTML = `<strong>Type:</strong> ${this.formatEnum(occ.type)}`;

      // Priority
      const priority = document.createElement('p');
      priority.className = 'text-sm text-gray-800';
      priority.innerHTML = `<strong>Priority:</strong> ${this.formatEnum(
        occ.priority
      )}`;

      // Status
      const status = document.createElement('p');
      status.className = 'text-sm text-gray-800';
      status.innerHTML = `<strong>Status:</strong> ${this.formatEnum(
        occ.status
      )}`;

      //"View Details" button
      const button = document.createElement('button');
      button.className =
        'bg-blue-600 hover:bg-blue-700 text-white text-sm py-1 px-3 rounded-md w-full mt-3 transition-colors duration-150';
      button.innerText = 'View Details';

      // Add the event listener
      button.addEventListener('click', (e) => {
        e.stopPropagation();
        this.onViewDetails(occ.id);
      });

      // Add all elements to the container
      popupContent.appendChild(title);
      popupContent.appendChild(type);
      popupContent.appendChild(priority);
      popupContent.appendChild(status);
      popupContent.appendChild(button);

      // Create the popup and set its content
      const popup = new mapboxgl.Popup({ offset: 25 }).setDOMContent(
        popupContent
      );

      // Create a custom marker element
      const el = document.createElement('div');
      el.className = 'custom-marker';

      const pinPath = this.getPinForType(occ.type);
      console.log('A tentar carregar o pin:', pinPath);
      el.style.backgroundImage = `url(${pinPath})`;

      // Create the marker and add it to the map
      new mapboxgl.Marker({ element: el, anchor: 'bottom' })
        .setLngLat([occ.longitude, occ.latitude])
        .setPopup(popup)
        .addTo(this.map);
    }
  }

  private getPinForType(type: OccurrenceType): string {
    const basePath = 'assets/pins/';

    switch (type) {
      case OccurrenceType.FOREST_FIRE:
        return `${basePath}FOREST_FIRE.png`;
      case OccurrenceType.URBAN_FIRE:
        return `${basePath}URBAN_FIRE.png`;
      case OccurrenceType.FLOOD:
        return `${basePath}FLOOD.png`;
      case OccurrenceType.LANDSLIDE:
        return `${basePath}LANDSLIDE.png`;
      case OccurrenceType.ROAD_ACCIDENT:
        return `${basePath}ROAD_ACCIDENT.png`;
      case OccurrenceType.VEHICLE_BREAKDOWN:
        return `${basePath}VEHICLE_BREAKDOWN.png`;
      case OccurrenceType.ANIMAL_ON_ROAD:
        return `${basePath}ANIMAL_ON_ROAD.png`;
      case OccurrenceType.ROAD_OBSTRUCTION:
        return `${basePath}ROAD_OBSTRUCTION.png`;
      case OccurrenceType.TRAFFIC_CONGESTION:
        return `${basePath}TRAFFIC_CONGESTION.png`;
      case OccurrenceType.PUBLIC_LIGHTING:
        return `${basePath}PUBLIC_LIGHTING.png`;
      case OccurrenceType.SANITATION:
        return `${basePath}SANITATION.png`;
      case OccurrenceType.ELECTRICAL_NETWORK:
        return `${basePath}ELECTRICAL_NETWORK.png`;
      case OccurrenceType.ROAD_DAMAGE:
        return `${basePath}ROAD_DAMAGE.png`;
      case OccurrenceType.TRAFFIC_LIGHT_FAILURE:
        return `${basePath}TRAFFIC_LIGHT_FAILURE.png`;
      case OccurrenceType.CRIME:
        return `${basePath}CRIME.png`;
      case OccurrenceType.PUBLIC_DISTURBANCE:
        return `${basePath}PUBLIC_DISTURBANCE.png`;
      case OccurrenceType.DOMESTIC_VIOLENCE:
        return `${basePath}DOMESTIC_VIOLENCE.png`;
      case OccurrenceType.LOST_ANIMAL:
        return `${basePath}LOST_ANIMAL.png`;
      case OccurrenceType.INJURED_ANIMAL:
        return `${basePath}INJURED_ANIMAL.png`;
      case OccurrenceType.POLLUTION:
        return `${basePath}POLLUTION.png`;
      case OccurrenceType.MEDICAL_EMERGENCY:
        return `${basePath}MEDICAL_EMERGENCY.png`;
      case OccurrenceType.WORK_ACCIDENT:
        return `${basePath}WORK_ACCIDENT.png`;

      default:
        console.warn(
          `Pin não encontrado para o tipo: ${type}, a usar default.`
        );
        return `${basePath}DEFAULT.png`;
    }
  }

  private formatEnum(
    value: string | OccurrenceType | PriorityLevel | OccurrenceStatus
  ): string {
    if (!value) return '';
    return value
      .toString()
      .split('_')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }

  private onViewDetails(id: number): void {
    console.log('Button clicked! Navigating to details for ID:', id);
    this.router.navigate(['/occurrence', id]);
  }
}
