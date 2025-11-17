import { Component, inject, OnInit, signal } from '@angular/core'; 
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { OccurrenceService } from '../../services/occurrence.service'; 
import { OccurrenceDetails } from '../../models/occurrence-details.model';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { OccurrenceType } from '../../models/occurrence-type.enum';
import { PriorityLevel } from '../../models/priority-level.enum';

@Component({
  selector: 'app-occurence-detail',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './occurence-detail.component.html',
  styleUrl: './occurence-detail.component.css'
})
export class OccurrenceDetailComponent implements OnInit {
goBack() {
throw new Error('Method not implemented.');
}
refresh() {
throw new Error('Method not implemented.');
}
  
  private route = inject(ActivatedRoute);
  private occurrenceService = inject(OccurrenceService);

  // Create a signal to hold the loaded data
  public occurrence = signal<OccurrenceDetails | null>(null);
  public isLoading = signal<boolean>(true);

  ngOnInit(): void {
    // Get the 'id' parameter from the URL
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      const occurrenceId = +idParam;
      console.log('Loading details for ID:', occurrenceId);

      // Call the service to fetch data
      this.occurrenceService.getOccurrenceById(occurrenceId).subscribe({
        next: (data) => {
          this.occurrence.set(data);
          this.isLoading.set(false);
          console.log('Data loaded:', data);
        },
        error: (err) => {
          console.error('Error loading occurrence details', err);
          this.isLoading.set(false);
        }
      });
    } else {
      console.error('No Occurrence ID found in URL');
      this.isLoading.set(false);
    }
  }

  // Add the helper function to format ENUMs
  formatEnum(value: string | OccurrenceType | PriorityLevel | OccurrenceStatus): string {
    if (!value) return '';
    return value
      .toString()
      .split('_')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' '); 
  }

  // Add a helper for formatting dates
  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('pt-PT');
  }
}