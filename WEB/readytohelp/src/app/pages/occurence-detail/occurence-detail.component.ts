import { Component, inject, OnInit, signal } from '@angular/core'; 
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common'; 
import { OccurrenceService } from '../../services/occurrence.service'; 
import { OccurrenceDetails } from '../../models/occurrence-details.model';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { OccurrenceType } from '../../models/occurrence-type.enum';
import { PriorityLevel } from '../../models/priority-level.enum';
import { LegalFooterComponent } from '../../components/legal-footer/legal-footer.component';

@Component({
  selector: 'app-occurence-detail',
  standalone: true,
  imports: [
    CommonModule,
    LegalFooterComponent
  ],
  templateUrl: './occurence-detail.component.html',
  styleUrl: './occurence-detail.component.css'
})
export class OccurrenceDetailComponent implements OnInit {
  
  private route = inject(ActivatedRoute);
  private occurrenceService = inject(OccurrenceService);

  public occurrence = signal<OccurrenceDetails | null>(null);
  public isLoading = signal<boolean>(true);
  
  private mapboxAccessToken = 'pk.eyJ1IjoidG1zMjYiLCJhIjoiY21pMXk3MW9qMTVnZjJqc2ZkMDVmbGF0NCJ9.ud2aOuGC2KH9YyNbJJM8Yg';

  /**
   * OnInit lifecycle hook to load occurrence details based on route parameter.
   */
  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      const occurrenceId = +idParam;
      console.log('Loading details for ID:', occurrenceId);

      this.occurrenceService.getOccurrenceById(occurrenceId).subscribe({
        next: (data) => {
          this.occurrence.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error loading details', err);
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }

  /**
   * Formats an enum value into a more readable string.
   * @param value The enum value to format.
   * @returns A formatted string with spaces and capitalization.
   */
  formatEnum(value: string | OccurrenceType | PriorityLevel | OccurrenceStatus): string {
    if (!value) return '';
    return value
      .toString()
      .split('_')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' '); 
  }

  /**
   * Formats a date string into a localized date and time string.
   * @param dateString The date string to format.
   * @returns A formatted date string or 'N/A' if the input is invalid.
   */
  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('pt-PT', { 
      dateStyle: 'short', 
      timeStyle: 'medium' 
    });
  }

  /**
   * Generates a static map URL for the occurrence location using Mapbox Static Images API.
   * @param occ The occurrence details containing latitude and longitude.
   * @returns A URL string for the static map image.
   */
  getStaticMapUrl(occ: OccurrenceDetails): string {
    const longitude = occ.longitude;
    const latitude = occ.latitude;
    const zoom = 14;
    const width = 600;
    const height = 400;
    
    return `https://api.mapbox.com/styles/v1/mapbox/streets-v11/static/pin-s(${longitude},${latitude})/${longitude},${latitude},${zoom},0/${width}x${height}@2x?access_token=${this.mapboxAccessToken}`;
  }
}