import { ChangeDetectionStrategy, Component, inject, OnInit, OnDestroy } from '@angular/core'; 
import { Router } from '@angular/router';
import * as mapboxgl from 'mapbox-gl';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceMap } from '../../models/occurrenceMap.model';
import { OccurrenceType } from '../../models/occurrence-type.enum';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { PriorityLevel } from '../../models/priority-level.enum';

/**
 * MapComponent
 * Displays a Mapbox map with markers for active occurrences.
 * Handles marker creation, popups, navigation to occurrence details,
 * and updates the data automatically every 30 seconds.
 */
@Component({
  selector: 'app-map',
  standalone: true,
  imports: [],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapComponent implements OnInit, OnDestroy { 
  private map?: mapboxgl.Map;
  private occurrenceService = inject(OccurrenceService);
  private router = inject(Router);

  // Array to store marker references so they can be removed later
  private markers: mapboxgl.Marker[] = [];
  
  // Variable to store the refresh interval ID
  private refreshInterval: any;

  constructor() {}

  /**
   * Angular lifecycle hook.
   * Initializes the Mapbox map and loads active occurrences when the map is ready.
   */
  ngOnInit(): void {
    this.map = new mapboxgl.Map({
      accessToken:
        'pk.eyJ1IjoidG1zMjYiLCJhIjoiY21pMXk3MW9qMTVnZjJqc2ZkMDVmbGF0NCJ9.ud2aOuGC2KH9YyNbJJM8Yg',
      container: 'map',
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-9.1393, 38.7223],
      zoom: 6,
    });

    this.map.addControl(new mapboxgl.NavigationControl());

    this.map.on('load', () => {
      // 1. Load for the first time
      this.loadOccurrences();
      // 2. Start the update cycle
      this.startAutoRefresh();
    });
  }

  /**
   * Cleanup when the component is destroyed (e.g., changing pages).
   * Stops the timer to prevent errors and memory leaks.
   */
  ngOnDestroy(): void {
    this.stopAutoRefresh();
  }

  /**
   * Starts the 30-second refresh interval.
   */
  private startAutoRefresh(): void {
    this.refreshInterval = setInterval(() => {
      console.log('Auto-refreshing occurrences...');
      this.loadOccurrences();
    }, 30000); // 30 seconds
  }

  /**
   * Stops the refresh interval.
   */
  private stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  /**
   * Fetches active occurrences from the service.
   */
  private loadOccurrences(): void {
    this.occurrenceService.getActiveOccurrences().subscribe({
      next: (occurrences) => {
        console.log(`Loaded ${occurrences.length} occurrences`);
        this.addMarkersToMap(occurrences);
      },
      error: (err) => {
        console.error('Error loading occurrences', err);
      },
    });
  }

  /**
   * Adds markers to map, clearing existing ones first to avoid duplicates.
   * @param occurrences Array of occurrences to display.
   */
  private addMarkersToMap(occurrences: OccurrenceMap[]): void {
    if (!this.map) return;

    // 1. IMPORTANT: Remove old markers from the map
    this.markers.forEach(marker => marker.remove());
    // Clear the array
    this.markers = [];

    for (const occ of occurrences) {
      // Create popup container
      const popupContainer = document.createElement('div');
      popupContainer.className = 'card border-0 shadow-none';
      popupContainer.style.width = '260px';

      // Header section
      const headerClass = 'card-header bg-primary text-white py-3 px-3';
      const header = document.createElement('div');
      header.className = headerClass;
      header.innerHTML = `
        <h5 class="mb-0 font-weight-bold text-truncate" style="padding-right: 25px; font-size: 1.2rem; line-height: 1.2;">
          ${occ.title}
        </h5>
      `;

      // Body section
      const body = document.createElement('div');
      body.className = 'card-body p-3';

      const infoRow = document.createElement('div');
      infoRow.className =
        'd-flex justify-content-between align-items-start mb-3';

      // Type column
      const typeCol = document.createElement('div');
      typeCol.innerHTML = `
        <div>
          <small class="text-muted d-block text-uppercase font-weight-bold" style="font-size: 0.75rem; letter-spacing: 0.5px; margin-bottom: 2px;">Type</small>
          <span class="font-weight-bold text-dark" style="font-size: 1.2rem;">${this.formatEnum(
            occ.type
          )}</span>
        </div>
      `;

      // Priority column
      let priorityClass = 'badge-light';
      if (occ.priority === PriorityLevel.HIGH) priorityClass = 'badge-danger';
      if (occ.priority === PriorityLevel.MEDIUM) priorityClass = 'badge-warning';
      if (occ.priority === PriorityLevel.LOW) priorityClass = 'badge-info';

      const priorityCol = document.createElement('div');
      priorityCol.className = 'text-right';
      priorityCol.innerHTML = `
        <div>
           <small class="text-muted d-block text-uppercase font-weight-bold" style="font-size: 0.75rem; letter-spacing: 0.5px; margin-bottom: 4px;">Priority</small>
           <span class="badge ${priorityClass} px-3 py-2" style="font-size: 1rem;">${this.formatEnum(
        occ.priority
      )}</span>
        </div>
      `;

      infoRow.appendChild(typeCol);
      infoRow.appendChild(priorityCol);
      body.appendChild(infoRow);

      // Footer with details button
      const footer = document.createElement('div');
      footer.className = 'card-footer bg-white border-0 p-3 pt-0';

      const button = document.createElement('button');
      button.className =
        'btn btn-outline-primary btn-block font-weight-bold btn-sm';
      button.innerHTML = 'View Details <i class="fas fa-arrow-right ml-1"></i>';

      button.addEventListener('click', (e) => {
        e.stopPropagation();
        this.onViewDetails(occ.id);
      });

      footer.appendChild(button);

      // Assemble popup
      popupContainer.appendChild(header);
      popupContainer.appendChild(body);
      popupContainer.appendChild(footer);

      const popup = new mapboxgl.Popup({
        offset: 35,
        closeButton: true,
        closeOnClick: false,
        maxWidth: '280px',
      }).setDOMContent(popupContainer);

      // Create custom marker element
      const el = document.createElement('div');
      el.className = 'custom-marker';

      const pinPath = this.getPinForType(occ.type);
      el.style.backgroundImage = `url(${pinPath})`;

      // Create the marker
      const marker = new mapboxgl.Marker({ element: el, anchor: 'bottom' })
        .setLngLat([occ.longitude, occ.latitude])
        .setPopup(popup)
        .addTo(this.map);
      
      // 2. Save the reference in the array to clear on the next update
      this.markers.push(marker);
    }
  }

  /**
   * Returns the file path for the pin image based on the occurrence type.
   */
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
          `Pin not found for type: ${type}, using default.`
        );
        return `${basePath}DEFAULT.png`;
    }
  }

  /**
   * Formats enum values to a more readable string.
   * E.g., "FOREST_FIRE" -> "Forest Fire"
   */
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

  /**
   * Handles the "View Details" button click in the popup.
   * Navigates to the occurrence details page.
   * @param id Occurrence ID
   */
  private onViewDetails(id: number): void {
    console.log('Button clicked! Navigating to details for ID:', id);
    this.router.navigate(['/occurrence', id]);
  }
}