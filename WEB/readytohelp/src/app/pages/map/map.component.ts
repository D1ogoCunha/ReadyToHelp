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

 
  /**
   * Creates and adds custom markers with COMPACT PROFESSIONAL popups
   */
  private addMarkersToMap(occurrences: OccurrenceMap[]): void {
    if (!this.map) return; 

    for (const occ of occurrences) {

      // --- CONSTRUÇÃO DO NOVO POPUP (LADO A LADO) ---

      // 1. Container Principal
      const popupContainer = document.createElement('div');
      popupContainer.className = 'card border-0 shadow-none'; 
      // Mantemos compacto (260px)
      popupContainer.style.width = '260px'; 

      // 2. Cabeçalho
      const headerClass = 'card-header bg-primary text-white py-3 px-3';
      const header = document.createElement('div');
      header.className = headerClass;
      header.innerHTML = `
        <h5 class="mb-0 font-weight-bold text-truncate" style="padding-right: 25px; font-size: 1.2rem; line-height: 1.2;">
          ${occ.title}
        </h5>
      `;

      // 3. Corpo do Card
      const body = document.createElement('div');
      body.className = 'card-body p-3';

      // --- ALTERAÇÃO PRINCIPAL: LINHA ÚNICA (LADO A LADO) ---
      const infoRow = document.createElement('div');
      infoRow.className = 'd-flex justify-content-between align-items-start mb-3'; // mb-3 dá espaço para o botão

      // Coluna da Esquerda: TIPO
      const typeCol = document.createElement('div');
      typeCol.innerHTML = `
        <div>
          <small class="text-muted d-block text-uppercase font-weight-bold" style="font-size: 0.75rem; letter-spacing: 0.5px; margin-bottom: 2px;">Type</small>
          <span class="font-weight-bold text-dark" style="font-size: 1.2rem;">${this.formatEnum(occ.type)}</span>
        </div>
      `;

      // Coluna da Direita: PRIORIDADE
      let priorityClass = 'badge-light';
      if (occ.priority === PriorityLevel.HIGH) priorityClass = 'badge-danger';
      if (occ.priority === PriorityLevel.MEDIUM) priorityClass = 'badge-warning';
      if (occ.priority === PriorityLevel.LOW) priorityClass = 'badge-info';

      const priorityCol = document.createElement('div');
      priorityCol.className = 'text-right'; // Alinha o texto à direita
      priorityCol.innerHTML = `
        <div>
           <small class="text-muted d-block text-uppercase font-weight-bold" style="font-size: 0.75rem; letter-spacing: 0.5px; margin-bottom: 4px;">Priority</small>
           <span class="badge ${priorityClass} px-3 py-2" style="font-size: 1rem;">${this.formatEnum(occ.priority)}</span>
        </div>
      `;

      // Juntar as colunas na linha
      infoRow.appendChild(typeCol);
      infoRow.appendChild(priorityCol);
      
      // Adicionar a linha ao corpo
      body.appendChild(infoRow);

      // 4. Rodapé com Botão (Mais compacto)
      const footer = document.createElement('div');
      footer.className = 'card-footer bg-white border-0 p-3 pt-0'; 
      
      const button = document.createElement('button');
      button.className = 'btn btn-outline-primary btn-block font-weight-bold btn-sm'; 
      button.innerHTML = 'View Details <i class="fas fa-arrow-right ml-1"></i>';
      
      button.addEventListener('click', (e) => {
        e.stopPropagation();
        this.onViewDetails(occ.id);
      });

      footer.appendChild(button);

      // 5. Montar o Popup Final
      popupContainer.appendChild(header);
      popupContainer.appendChild(body);
      popupContainer.appendChild(footer);

      // --- FIM DA CONSTRUÇÃO DO POPUP ---

      const popup = new mapboxgl.Popup({ 
        offset: 35, 
        closeButton: true, 
        closeOnClick: false,
        maxWidth: '280px'
      })
      .setDOMContent(popupContainer);

      const el = document.createElement('div');
      el.className = 'custom-marker'; 
      
      const pinPath = this.getPinForType(occ.type);
      el.style.backgroundImage = `url(${pinPath})`;

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
