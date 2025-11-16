import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OccurrenceMap } from '../models/occurrenceMap.model'; 

@Injectable({
  providedIn: 'root'
})
export class OccurrenceService {

  private apiUrl = 'https://readytohelp-api.azurewebsites.net/api/occurrence'; 

  private http = inject(HttpClient);

  /**
   * Search for all active occurrences
   * Corresponds to the endpoint [HttpGet("active")]
   */
  getActiveOccurrences(): Observable<OccurrenceMap[]> {
    return this.http.get<OccurrenceMap[]>(`${this.apiUrl}/active`);
  }
}