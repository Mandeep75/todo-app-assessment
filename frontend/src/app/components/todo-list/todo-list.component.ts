import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoService } from '../../services/todo.service';
import { Todo } from '../../models/todo.model';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.scss']
})
export class TodoListComponent implements OnInit {
  todos = signal<Todo[]>([]);
  newTitle = signal('');
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  pendingTitle = '';

  remainingCount = computed(() =>
    this.todos().filter(t => !t.isCompleted).length
  );

  completedCount = computed(() =>
    this.todos().filter(t => t.isCompleted).length
  );

  constructor(private readonly todoService: TodoService) {}

  ngOnInit(): void {
    this.loadTodos();
  }

  loadTodos(): void {
    this.isLoading.set(true);
    this.todoService.getAll().subscribe({
      next: todos => {
        this.todos.set(todos);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load todos. Is the API running?');
        this.isLoading.set(false);
      }
    });
  }

  addTodo(): void {
    const title = this.pendingTitle.trim();
    if (!title) return;

    this.todoService.create({ title }).subscribe({
      next: created => {
        this.todos.update(todos => [...todos, created]);
        this.pendingTitle = '';
        this.errorMessage.set(null);
      },
      error: () => this.errorMessage.set('Failed to add todo.')
    });
  }

  deleteTodo(id: string): void {
    this.todoService.delete(id).subscribe({
      next: () => {
        this.todos.update(todos => todos.filter(t => t.id !== id));
        this.errorMessage.set(null);
      },
      error: () => this.errorMessage.set('Failed to delete todo.')
    });
  }

  toggleTodo(id: string): void {
    this.todoService.toggle(id).subscribe({
      next: updated => {
        this.todos.update(todos =>
          todos.map(t => (t.id === updated.id ? updated : t))
        );
        this.errorMessage.set(null);
      },
      error: () => this.errorMessage.set('Failed to update todo.')
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.addTodo();
  }

  trackById(_: number, todo: Todo): string {
    return todo.id;
  }
}
