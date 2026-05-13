import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TodoService } from './todo.service';
import { Todo } from '../models/todo.model';
import { environment } from '../../environments/environment';

describe('TodoService', () => {
  let service: TodoService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/api/todos`;

  const mockTodo: Todo = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    title: 'Test todo',
    isCompleted: false,
    createdAt: new Date().toISOString()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TodoService]
    });
    service = TestBed.inject(TodoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  describe('getAll', () => {
    it('should GET all todos from the API', () => {
      service.getAll().subscribe(todos => {
        expect(todos.length).toBe(1);
        expect(todos[0]).toEqual(mockTodo);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush([mockTodo]);
    });

    it('should return an empty array when there are no todos', () => {
      service.getAll().subscribe(todos => expect(todos).toEqual([]));
      httpMock.expectOne(apiUrl).flush([]);
    });
  });

  describe('create', () => {
    it('should POST a new todo to the API', () => {
      service.create({ title: 'New task' }).subscribe(todo => {
        expect(todo).toEqual(mockTodo);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ title: 'New task' });
      req.flush(mockTodo);
    });
  });

  describe('delete', () => {
    it('should DELETE the todo by ID', () => {
      service.delete(mockTodo.id).subscribe(() => {
        // no-op; just verifying the call
      });

      const req = httpMock.expectOne(`${apiUrl}/${mockTodo.id}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null, { status: 204, statusText: 'No Content' });
    });
  });

  describe('toggle', () => {
    it('should PATCH the toggle endpoint for the given ID', () => {
      const toggled = { ...mockTodo, isCompleted: true };
      service.toggle(mockTodo.id).subscribe(todo => {
        expect(todo.isCompleted).toBeTrue();
      });

      const req = httpMock.expectOne(`${apiUrl}/${mockTodo.id}/toggle`);
      expect(req.request.method).toBe('PATCH');
      req.flush(toggled);
    });
  });
});
