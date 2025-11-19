import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OccurrenceService } from '../../services/occurrence.service';
import { OccurrenceDetails } from '../../models/occurrence-details.model';
import { OccurrenceStatus } from '../../models/occurrence-status.enum';
import { PriorityLevel } from '../../models/priority-level.enum';
import { OccurrenceType } from '../../models/occurrence-type.enum';

@Component({
  selector: 'app-occurrences-history',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './occurrences-history.component.html',
  styleUrls: ['./occurrences-history.component.css'],
})
export class OccurrencesHistoryComponent implements OnInit {
  private occurrenceService = inject(OccurrenceService);

  // --- Estado da Tabela ---
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  occurrencesClosed = signal<OccurrenceDetails[]>([]);

  // --- Paginação ---
  pageNumber = signal<number>(1);
  pageSize = signal<number>(50);
  hasNextPage = signal<boolean>(false);

  // --- Ordenação (Novos Signals) ---
  currentSortBy = signal<string>('CreationDateTime');
  currentSortOrder = signal<'asc' | 'desc'>('desc');

  // --- Filtros (Signals) ---
  filterSearch = signal<string>('');
  filterType = signal<string>('');
  filterPriority = signal<string>('');
  filterStartDate = signal<string>('');
  filterEndDate = signal<string>('');

  // Listas para os Selects
  types = Object.values(OccurrenceType);
  priorities = Object.values(PriorityLevel);

  ngOnInit(): void {
    this.loadPage();
  }

  loadPage(): void {
    this.isLoading.set(true);
    this.error.set(null);

    // 1. Construir a string de filtro para a API
    const searchTerms: string[] = [];
    if (this.filterSearch()) searchTerms.push(this.filterSearch());
    const apiFilterString = searchTerms.join(' ');

    // 2. Chamar API com Ordenação
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

          // 3. Filtragem Rigorosa no Cliente
          const filteredList = rawList
            .map((item) => this.normalizeItem(item))
            .filter((item): item is OccurrenceDetails => {
              if (!item) return false;

              // Regra 1: Apenas Encerradas
              if (item.status !== OccurrenceStatus.CLOSED) return false;

              // Regra 2: Pesquisa de Texto (Título ou ID)
              // Isto resolve o problema se a API devolver dados "extra" ou se quisermos garantir o match
              if (this.filterSearch()) {
                const term = this.filterSearch().toLowerCase();
                const matchesId = item.id.toString().includes(term);
                const matchesTitle = item.title.toLowerCase().includes(term);
                if (!matchesId && !matchesTitle) return false;
              }

              // Regra 3: Tipo
              if (this.filterType() && item.type !== this.filterType())
                return false;

              // Regra 4: Prioridade
              if (
                this.filterPriority() &&
                item.priority !== this.filterPriority()
              )
                return false;

              // Regra 5: Datas
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
          console.error('Erro ao carregar histórico:', err);
          this.error.set('Não foi possível carregar o histórico.');
          this.isLoading.set(false);
        },
      });
  }

  // --- Ações de Ordenação ---
  toggleSort(column: string): void {
    // Se clicarmos na mesma coluna, invertemos a ordem
    if (this.currentSortBy() === column) {
      const newOrder = this.currentSortOrder() === 'asc' ? 'desc' : 'asc';
      this.currentSortOrder.set(newOrder);
    } else {
      // Se for nova coluna, definimos como descendente por defeito (mais recente/maior primeiro)
      this.currentSortBy.set(column);
      this.currentSortOrder.set('desc');
    }
    this.pageNumber.set(1); // Voltar à página 1 ao reordenar
    this.loadPage();
  }

  // --- Ações de UI ---

  onFilterChange(): void {
    this.pageNumber.set(1);
    this.loadPage();
  }

  clearFilters(): void {
    this.filterSearch.set('');
    this.filterType.set('');
    this.filterPriority.set('');
    this.filterStartDate.set('');
    this.filterEndDate.set('');

    // Reset ordenação também se desejar, ou manter
    this.currentSortBy.set('CreationDateTime');
    this.currentSortOrder.set('desc');

    this.pageNumber.set(1);
    this.loadPage();
  }

  nextPage(): void {
    if (this.hasNextPage() && !this.isLoading()) {
      this.pageNumber.update((v) => v + 1);
      this.loadPage();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1 && !this.isLoading()) {
      this.pageNumber.update((v) => v - 1);
      this.loadPage();
    }
  }

  // --- Helpers ---

  formatEnum(value: any): string {
    if (!value) return '';
    return String(value)
      .split('_')
      .map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())
      .join(' ');
  }

  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('pt-PT');
  }

  private extractList(data: any): any[] {
    if (Array.isArray(data)) return data;
    return data?.items || data?.value || data?.$values || data?.data || [];
  }

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
