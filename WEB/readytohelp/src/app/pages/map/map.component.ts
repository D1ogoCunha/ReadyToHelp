import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import * as mapboxgl from 'mapbox-gl';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceMap } from '../../models/occurrenceMap.model';
import { OccurrenceType } from '../../models/occurrence-type.enum'; 

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
      }
    });
  }

  /**
   * Adiciona os marcadores ao mapa
   */
 private addMarkersToMap(occurrences: OccurrenceMap[]): void {
    if (!this.map) return; 

    for (const occ of occurrences) {
      // 1. Criar o HTML para o popup
      const popupHtml = `
        <div  class="p-2">
          <h3 class="font-bold text-lg text-blue-600">${occ.title}</h3>
          <p class="text-sm text-gray-700">Tipo: ${occ.type}</p>
          <p class="text-sm text-gray-700">Prioridade: ${occ.priority}</p>
        </div>
      `;

      // 2. Criar o Popup
      const popup = new mapboxgl.Popup({ offset: 25 })
        .setHTML(popupHtml);

      // 3. Criar o Elemento 'div' personalizado
      const el = document.createElement('div');
      el.className = 'custom-marker'; // Aplicar a classe CSS
      
      // 4. Definir a imagem de fundo com base no tipo
      el.style.backgroundImage = `url(${this.getPinForType(occ.type)})`;

      // 5. Criar o Marcador com o elemento personalizado
      new mapboxgl.Marker(el) // Passar o 'div' personalizado
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

}
