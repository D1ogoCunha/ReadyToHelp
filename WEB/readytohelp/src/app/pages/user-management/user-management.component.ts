import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css',
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  loading = false;
  error = '';
  pageNumber = 1;
  pageSize = 10;
  sortBy = 'Name';
  sortOrder = 'asc';
  filter = '';

  constructor(private userService: UserService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = '';
    this.userService
      .getAllUsers(
        this.pageNumber,
        this.pageSize,
        this.sortBy,
        this.sortOrder,
        this.filter
      )
      .subscribe({
        next: (data) => {
          this.users = (data || []).filter((u) => u.id !== 1);
          this.loading = false;
        },
        error: () => {
          this.error = 'Falha ao carregar utilizadores';
          this.loading = false;
        },
      });
  }

  applyFilter() {
    this.pageNumber = 1;
    this.load();
  }

  changeSort(field: string) {
    if (this.sortBy === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortOrder = 'asc';
    }
    this.load();
  }

  nextPage() {
    this.pageNumber++;
    this.load();
  }

  prevPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.load();
    }
  }
}
