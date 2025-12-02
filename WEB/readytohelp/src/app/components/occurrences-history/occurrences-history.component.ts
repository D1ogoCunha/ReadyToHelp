import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceDetails } from '../../models/occurrence-details.model';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { PriorityLevel } from '../../models/priority-level.enum';
import { OccurrenceType } from '../../models/occurrence-type.enum';
import { LegalFooterComponent } from '../legal-footer/legal-footer.component';

@Component({
  selector: 'app-occurrences-history',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, LegalFooterComponent],
  templateUrl: './occurrences-history.component.html',
  styleUrls: ['./occurrences-history.component.css'],
})
export class OccurrencesHistoryComponent implements OnInit {
  private occurrenceService = inject(OccurrenceService);

  // State Signals
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  occurrencesClosed = signal<OccurrenceDetails[]>([]);

  // Paging Signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(50);
  hasNextPage = signal<boolean>(false);

  // Sorting Signals
  currentSortBy = signal<string>('CreationDateTime');
  currentSortOrder = signal<'asc' | 'desc'>('desc');

  // Filtering Signals
  filterSearch = signal<string>('');
  filterType = signal<string>('');
  filterPriority = signal<string>('');
  filterStartDate = signal<string>('');
  filterEndDate = signal<string>('');

  // Enums for dropdowns
  types = Object.values(OccurrenceType);
  priorities = Object.values(PriorityLevel);

  /**
   * OnInit lifecycle hook to load the first page of occurrences.
   */
  ngOnInit(): void {
    this.loadPage();
  }

  /**
   * Loads a page of closed occurrences applying current filters and sorting.
   */
  loadPage(): void {
    this.isLoading.set(true);
    this.error.set(null);

    // Build filter string for API
    const searchTerms: string[] = [];
    if (this.filterSearch()) searchTerms.push(this.filterSearch());
    const apiFilterString = searchTerms.join(' ');

    // Call API with Sorting
    this.occurrenceService
      .getOccurrences({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.currentSortBy(), // Passar coluna de ordenação
        sortOrder: this.currentSortOrder(), // Passar direção
        filter: apiFilterString.trim(),
      })
      .subscribe({
        next: (data) => {
          const rawList = this.extractList(data);

          // Client-side Strict Filtering
          const filteredList = rawList
            .map((item) => this.normalizeItem(item))
            .filter((item): item is OccurrenceDetails => {
              if (!item) return false;

              // Rule 1: Only Closed
              if (item.status !== OccurrenceStatus.CLOSED) return false;

              // Rule 2: Text Search (Title or ID)
              // This solves the problem if the API returns "extra" data or if we want to guarantee the match
              if (this.filterSearch()) {
                const term = this.filterSearch().toLowerCase();
                const matchesId = item.id.toString().includes(term);
                const matchesTitle = item.title.toLowerCase().includes(term);
                if (!matchesId && !matchesTitle) return false;
              }

              // Rule 3: Type
              if (this.filterType() && item.type !== this.filterType())
                return false;

              // Rule 4: Priority
              if (
                this.filterPriority() &&
                item.priority !== this.filterPriority()
              )
                return false;

              // Rule 5: Dates
              const itemDate = new Date(item.creationDateTime);

              if (this.filterStartDate()) {
                const start = new Date(this.filterStartDate() + 'T00:00:00');
                if (itemDate < start) return false;
              }

              if (this.filterEndDate()) {
                const end = new Date(this.filterEndDate() + 'T23:59:59.999');
                if (itemDate > end) return false;
              }

              return true;
            });

          this.occurrencesClosed.set(filteredList);
          this.hasNextPage.set(rawList.length >= this.pageSize());
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error loading history:', err);
          this.error.set('It was not possible to load the history.');
          this.isLoading.set(false);
        },
      });
  }

  /**
   * Toggles the sorting order for a given column.
   * @param column The column to sort by
   */
  toggleSort(column: string): void {
    // If already sorting by this column, toggle order
    if (this.currentSortBy() === column) {
      const newOrder = this.currentSortOrder() === 'asc' ? 'desc' : 'asc';
      this.currentSortOrder.set(newOrder);
    } else {
      // If it's a new column, set default to descending (most recent/largest first)
      this.currentSortBy.set(column);
      this.currentSortOrder.set('desc');
    }
    this.pageNumber.set(1);
    this.loadPage();
  }

  /**
   * Handles filter changes by resetting to the first page and reloading data.
   */
  onFilterChange(): void {
    this.pageNumber.set(1);
    this.loadPage();
  }

  /**
   * Clears all filters and reloads the first page.
   */
  clearFilters(): void {
    this.filterSearch.set('');
    this.filterType.set('');
    this.filterPriority.set('');
    this.filterStartDate.set('');
    this.filterEndDate.set('');

    // Reset sorting as well if desired, or keep
    this.currentSortBy.set('CreationDateTime');
    this.currentSortOrder.set('desc');

    this.pageNumber.set(1);
    this.loadPage();
  }

  /**
   * Navigates to the next page if available.
   */
  nextPage(): void {
    if (this.hasNextPage() && !this.isLoading()) {
      this.pageNumber.update((v) => v + 1);
      this.loadPage();
    }
  }

  /**
   * Navigates to the previous page if not on the first page.
   */
  prevPage(): void {
    if (this.pageNumber() > 1 && !this.isLoading()) {
      this.pageNumber.update((v) => v - 1);
      this.loadPage();
    }
  }

  /**
   * Formats an enum value into a human-readable string.
   * @param value The enum value to format
   * @returns A human-readable string representation of the enum value
   */
  formatEnum(value: any): string {
    if (!value) return '';
    return String(value)
      .split('_')
      .map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())
      .join(' ');
  }

  /**
   * Formats a date string into a localized string.
   * @param dateString The date string to format
   * @returns A localized string representation of the date or 'N/A' if no date is provided
   */
  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('pt-PT');
  }

  /**
   * Extracts a list of items from various possible API response structures.
   * @param data The raw data from the API 
   * @returns An array extracted from the raw data
   */
  private extractList(data: any): any[] {
    if (Array.isArray(data)) return data;
    return data?.items || data?.value || data?.$values || data?.data || [];
  }

  /**
   * Normalizes a raw occurrence object into an OccurrenceDetails object.
   * @param o The raw occurrence object to normalize
   * @returns A normalized occurrence details object or null if input is invalid
   */
  private normalizeItem(o: any): OccurrenceDetails | null {
    if (!o) return null;
    const statusMap = [
      OccurrenceStatus.WAITING,
      OccurrenceStatus.ACTIVE,
      OccurrenceStatus.CLOSED,
    ];
    let status = o.status;
    if (typeof o.status === 'number') status = statusMap[o.status];

    return {
      ...o,
      status: status as OccurrenceStatus,
      type: o.type as OccurrenceType,
      priority: o.priority as PriorityLevel,
    };
  }
}
