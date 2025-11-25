import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OccurrenceMap } from '../models/occurrenceMap.model';
import { OccurrenceDetails } from '../models/occurrence-details.model';

/**
 * OccurrenceService
 * Provides methods to interact with the occurrence API for fetching map data, details, and paginated lists.
 */
@Injectable({
  providedIn: 'root',
})
export class OccurrenceService {

  /** Base URL for occurrence API endpoints */
  private apiUrl = 'https://readytohelp-api.up.railway.app/api/occurrence'; 

  /** Injected HttpClient for making HTTP requests */
  private http = inject(HttpClient);

  /**
   * Gets all active occurrences for the map.
   * Corresponds to [HttpGet("active")]
   * @returns An observable of an array of OccurrenceMap objects.
   */
  getActiveOccurrences(): Observable<OccurrenceMap[]> {
    return this.http.get<OccurrenceMap[]>(`${this.apiUrl}/active`);
  }

  /**
   * Gets the full details for a single occurrence by its ID.
   * Corresponds to [HttpGet("{id:int}")]
   * @param id The ID of the occurrence.
   * @returns An observable of the OccurrenceDetails object.
   */
  getOccurrenceById(id: number): Observable<OccurrenceDetails> {
    return this.http.get<OccurrenceDetails>(`${this.apiUrl}/${id}`);
  }

  /**
   * Gets a paginated list of occurrences with optional sorting and filtering.
   * Corresponds to [HttpGet]
   * @param options An object containing pagination, sorting, and filtering options.
   * @returns An observable of an array of OccurrenceDetails objects.
   */
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