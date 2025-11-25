import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user.model';
import { FormsModule } from '@angular/forms';
import { LegalFooterComponent } from '../../components/legal-footer/legal-footer.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, LegalFooterComponent],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css',
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  loading = false;
  deletingId: number | null = null;
  error = '';
  pageNumber = 1;
  pageSize = 10;
  sortBy = 'Name';
  sortOrder = 'asc';
  filter = '';
  confirmModalOpen = false;
  userPendingDelete: User | null = null;
  toast: { show: boolean; message: string; type: 'success' | 'error' } = {
    show: false,
    message: '',
    type: 'success',
  };
  editModalOpen = false;
  saving = false;
  isEditing = false;
  editedUser: {
    id?: number;
    name: string;
    email: string;
    password?: string;
    profile: 'CITIZEN' | 'MANAGER' | 'ADMIN';
  } = { name: '', email: '', password: '', profile: 'CITIZEN' };

  /**
   * Constructor for UserManagementComponent
   * @param userService The user service to manage users
   */
  constructor(private userService: UserService) {}

  /**
   * OnInit lifecycle hook to load users initially
   */
  ngOnInit() {
    this.load();
  }

  /**
   * Load users with current settings
   */
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
          // Filtra ID 1
          this.users = (data || []).filter((u) => u.id !== 1);
          this.loading = false;
        },
        error: () => {
          this.error = 'Falha ao carregar utilizadores';
          this.loading = false;
        },
      });
  }

  /**
   * Apply filter and reload users
   */
  applyFilter() {
    this.pageNumber = 1;
    this.load();
  }

  /**
   * Change sorting field and order, then reload users
   * @param field The field to sort by
   */
  changeSort(field: string) {
    if (this.sortBy === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortOrder = 'asc';
    }
    this.load();
  }

  /**
   * Go to the next page and reload users
   */
  nextPage() {
    this.pageNumber++;
    this.load();
  }

  /**
   * Go to the previous page and reload users
   */
  prevPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.load();
    }
  }

  /**
   * Open modal to create a new user
   * @param ev Optional event to prevent default
   */
  openCreate(ev?: Event) {
    ev?.preventDefault();
    this.isEditing = false;
    this.editedUser = { name: '', email: '', password: '', profile: 'CITIZEN' };
    this.editModalOpen = true;
  }

  /**
   * Open modal to edit an existing user
   * @param u The user to edit
   * @param ev Optional event to prevent default
   */
  openEdit(u: User, ev?: Event) {
    ev?.preventDefault();
    if (!u || u.id === 1) return;
    this.isEditing = true;
    this.editedUser = {
      id: u.id,
      name: u.name,
      email: u.email,
      password: '',
      profile: u.profile as any,
    };
    this.editModalOpen = true;
  }

  /**
   * Close edit modal
   * @param ev Optional event to prevent default
   */
  closeEdit(ev?: Event) {
    ev?.preventDefault();
    if (this.saving) return;
    this.editModalOpen = false;
  }

  /**
   * Save the edited or new user
   */
  saveUser() {
    if (!this.editedUser.name?.trim() || !this.editedUser.email?.trim()) return;
    if (!this.isEditing && !this.editedUser.password?.trim()) return;

    this.saving = true;

    const done = (ok: boolean, msg: string) => {
      this.saving = false;
      this.editModalOpen = false;
      this.showToast(msg, ok ? 'success' : 'error');
      if (ok) this.load();
    };

    if (this.isEditing && this.editedUser.id) {
      this.userService
        .updateUser(this.editedUser.id, {
          name: this.editedUser.name.trim(),
          email: this.editedUser.email.trim(),
          password: this.editedUser.password?.trim(),
          profile: this.editedUser.profile,
        })
        .subscribe({
          next: () => done(true, 'User updated successfully'),
          error: () => done(false, 'Failed to update user'),
        });
    } else {
      this.userService
        .createUser({
          name: this.editedUser.name.trim(),
          email: this.editedUser.email.trim(),
          password: this.editedUser.password?.trim() || '',
          profile: this.editedUser.profile,
        })
        .subscribe({
          next: () => done(true, 'User created successfully'),
          error: () => done(false, 'Failed to create user'),
        });
    }
  }

  /**   
   * Delete a user directly (without confirmation modal)
   * @param u The user to delete
   * @param ev Optional event to prevent default
   */
  onDelete(u: User, ev?: Event) {
    ev?.preventDefault();
    if (!u || u.id === 1) return;

    const ok = confirm(`Remove user "${u.name}" (ID ${u.id})?`);
    if (!ok) return;

    this.deletingId = u.id;
    this.error = '';
    this.userService.deleteUser(u.id).subscribe({
      next: () => {
        this.users = this.users.filter((x) => x.id !== u.id);
        this.deletingId = null;
      },
      error: () => {
        this.error = 'Failed to remove user';
        this.deletingId = null;
      },
    });
  }

  /**
   * Open modal to confirm user deletion
   * @param u The user to confirm deletion
   * @param ev Optional event to prevent default
   * @returns 
   */
  openConfirm(u: User, ev?: Event) {
    ev?.preventDefault();
    if (!u || u.id === 1) return;
    this.userPendingDelete = u;
    this.confirmModalOpen = true;
  }

  /**
   * Close confirmation modal
   * @param ev Optional event to prevent default
   */
  closeConfirm(ev?: Event) {
    ev?.preventDefault();
    if (this.deletingId) return;
    this.confirmModalOpen = false;
    this.userPendingDelete = null;
  }

  /**
   * Confirm deletion of the pending user
   */
  confirmDelete() {
    const u = this.userPendingDelete;
    if (!u) return;

    this.deletingId = u.id;
    this.userService.deleteUser(u.id).subscribe({
      next: () => {
        this.users = this.users.filter((x) => x.id !== u.id);
        this.deletingId = null;
        this.confirmModalOpen = false;
        this.userPendingDelete = null;
        this.showToast(
          `Utilizador "${u.name}" removido com sucesso.`,
          'success'
        );
      },
      error: () => {
        this.deletingId = null;
        this.confirmModalOpen = false;
        this.userPendingDelete = null;
        this.showToast(`Falha ao remover o utilizador "${u.name}".`, 'error');
      },
    });
  }

  /**
   * Show a toast message
   * @param message The message to show
   * @param type The type of the message ('success' or 'error')
   */
  showToast(message: string, type: 'success' | 'error') {
    this.toast = { show: true, message, type };
    setTimeout(() => (this.toast.show = false), 3000);
  }
}
