import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core'; // Importar OnInit
import * as mapboxgl from 'mapbox-gl'; // Importar mapbox-gl

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapComponent implements OnInit { // Implementar OnInit

  private map?: mapboxgl.Map; // PropriedADE para guardar a instância do mapa

  constructor() { }

  ngOnInit(): void {
    // Criar a instância do mapa
    this.map = new mapboxgl.Map({
      accessToken: 'pk.eyJ1IjoidG1zMjYiLCJhIjoiY21pMXk3MW9qMTVnZjJqc2ZkMDVmbGF0NCJ9.ud2aOuGC2KH9YyNbJJM8Yg', // <-- ADICIONADO AQUI
      container: 'map', // O ID do div no map.component.html
      style: 'mapbox://styles/mapbox/streets-v11', // Estilo standard do Mapbox
      center: [-9.1393, 38.7223], // Coordenadas [lng, lat] (ex: Lisboa)
      zoom: 12 // Nível de zoom inicial
    });

    // Opcional: Adicionar controlos de zoom e rotação
    this.map.addControl(new mapboxgl.NavigationControl());
  }
  
}