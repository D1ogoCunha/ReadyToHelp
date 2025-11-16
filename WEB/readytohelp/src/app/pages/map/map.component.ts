import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import * as mapboxgl from 'mapbox-gl';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceMap } from '../../models/occurrenceMap.model';

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

  constructor() {}

  ngOnInit(): void {
    this.map = new mapboxgl.Map({
      accessToken:
        'pk.eyJ1IjoidG1zMjYiLCJhIjoiY21pMXk3MW9qMTVnZjJqc2ZkMDVmbGF0NCJ9.ud2aOuGC2KH9YyNbJJM8Yg', // <-- ADICIONADO AQUI
      container: 'map',
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-9.1393, 38.7223],
      zoom: 12,
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
      }
    });
  }

  /**
   * Adiciona os marcadores ao mapa
   */
  private addMarkersToMap(occurrences: OccurrenceMap[]): void {
    if (!this.map) return; // Safety check in case the map is not ready

    for (const occ of occurrences) {
      // 1. Create the HTML for the popup
      const popupHtml = `
        <div class="p-2">
          <h3 class="font-bold text-lg text-blue-600">${occ.title}</h3>
          <p class="text-sm text-gray-700">Tipo: ${occ.type}</p>
          <p class="text-sm text-gray-700">Prioridade: ${occ.priority}</p>
        </div>
      `;

      // 2. Create the Popup
      const popup = new mapboxgl.Popup({ offset: 25 })
        .setHTML(popupHtml);

      // 3. Create the Marker [Image of a map pin]
      new mapboxgl.Marker()
        .setLngLat([occ.longitude, occ.latitude])
        .setPopup(popup) // Add the popup to the marker
        .addTo(this.map);
    }
  }
}
