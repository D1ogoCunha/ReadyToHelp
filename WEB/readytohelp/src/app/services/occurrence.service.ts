import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OccurrenceMap } from '../models/occurrenceMap.model';
import { OccurrenceDetails } from '../models/occurrence-details.model';

@Injectable({
  providedIn: 'root',
})
export class OccurrenceService {
  private apiUrl = 'http://localhost:5134/api/occurrence';

  private http = inject(HttpClient);

  /**
   * Gets all active occurrences for the map.
   * Corresponds to [HttpGet("active")]
   */
  getActiveOccurrences(): Observable<OccurrenceMap[]> {
    return this.http.get<OccurrenceMap[]>(`${this.apiUrl}/active`);
  }

  /**
   * Added this method
   * Gets the full details for a single occurrence by its ID.
   * Corresponds to [HttpGet("{id:int}")]
   */
  getOccurrenceById(id: number): Observable<OccurrenceDetails> {
    return this.http.get<OccurrenceDetails>(`${this.apiUrl}/${id}`);
  }

  getOccurrences(options?: {
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
    filter?: string;
  }): Observable<OccurrenceDetails[]> {
    const {
      pageNumber = 1,
      pageSize = 10,
      sortBy = 'CreationDateTime',
      sortOrder = 'desc',
      filter = '',
    } = options || {};

    const params = {
      pageNumber: String(pageNumber),
      pageSize: String(pageSize),
      sortBy,
      sortOrder,
      filter,
    };

    return this.http.get<OccurrenceDetails[]>(this.apiUrl, { params });
  }
}
