import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TodoListComponent } from './todo-list.component';
import { TodoService } from '../../services/todo.service';
import { of, throwError } from 'rxjs';
import { Todo } from '../../models/todo.model';
import { By } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

const makeTodo = (overrides: Partial<Todo> = {}): Todo => ({
  id: crypto.randomUUID(),
  title: 'Test todo',
  isCompleted: false,
  createdAt: new Date().toISOString(),
  ...overrides
});

describe('TodoListComponent', () => {
  let component: TodoListComponent;
  let fixture: ComponentFixture<TodoListComponent>;
  let todoServiceSpy: jasmine.SpyObj<TodoService>;

  beforeEach(async () => {
    todoServiceSpy = jasmine.createSpyObj('TodoService', [
      'getAll', 'create', 'delete', 'toggle'
    ]);
    todoServiceSpy.getAll.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [TodoListComponent, FormsModule],
      providers: [{ provide: TodoService, useValue: todoServiceSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(TodoListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ── Initialisation ─────────────────────────────────────────────────────────

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should call getAll on init', () => {
    expect(todoServiceSpy.getAll).toHaveBeenCalledTimes(1);
  });

  it('should display todos returned by the service', () => {
    const todos = [makeTodo({ title: 'First' }), makeTodo({ title: 'Second' })];
    todoServiceSpy.getAll.and.returnValue(of(todos));

    component.ngOnInit();
    fixture.detectChanges();

    const items = fixture.debugElement.queryAll(By.css('.todo-item'));
    expect(items.length).toBe(2);
  });

  it('should show the empty-state message when there are no todos', () => {
    todoServiceSpy.getAll.and.returnValue(of([]));
    component.ngOnInit();
    fixture.detectChanges();

    const empty = fixture.debugElement.query(By.css('.empty-state'));
    expect(empty).toBeTruthy();
  });

  it('should show an error banner when loading fails', () => {
    todoServiceSpy.getAll.and.returnValue(throwError(() => new Error('Network error')));
    component.ngOnInit();
    fixture.detectChanges();

    const banner = fixture.debugElement.query(By.css('.error-banner'));
    expect(banner).toBeTruthy();
  });

  // ── Adding todos ───────────────────────────────────────────────────────────

  it('should call create and append the new todo', () => {
    const newTodo = makeTodo({ title: 'New task' });
    todoServiceSpy.create.and.returnValue(of(newTodo));

    component.pendingTitle = 'New task';
    component.addTodo();
    fixture.detectChanges();

    expect(todoServiceSpy.create).toHaveBeenCalledWith({ title: 'New task' });
    expect(component.todos()).toContain(newTodo);
  });

  it('should clear the input after a successful add', () => {
    todoServiceSpy.create.and.returnValue(of(makeTodo()));
    component.pendingTitle = 'Some task';
    component.addTodo();

    expect(component.pendingTitle).toBe('');
  });

  it('should not call create when title is empty', () => {
    component.pendingTitle = '   ';
    component.addTodo();

    expect(todoServiceSpy.create).not.toHaveBeenCalled();
  });

  it('should trim whitespace before sending to the service', () => {
    todoServiceSpy.create.and.returnValue(of(makeTodo()));
    component.pendingTitle = '  Trimmed  ';
    component.addTodo();

    expect(todoServiceSpy.create).toHaveBeenCalledWith({ title: 'Trimmed' });
  });

  it('should add a todo when Enter is pressed', () => {
    todoServiceSpy.create.and.returnValue(of(makeTodo({ title: 'Enter task' })));
    component.pendingTitle = 'Enter task';
    component.onKeydown(new KeyboardEvent('keydown', { key: 'Enter' }));

    expect(todoServiceSpy.create).toHaveBeenCalled();
  });

  it('should not add a todo when a non-Enter key is pressed', () => {
    component.pendingTitle = 'Not yet';
    component.onKeydown(new KeyboardEvent('keydown', { key: 'Tab' }));

    expect(todoServiceSpy.create).not.toHaveBeenCalled();
  });

  // ── Deleting todos ─────────────────────────────────────────────────────────

  it('should call delete and remove the todo from the list', () => {
    const todo = makeTodo({ title: 'Delete me' });
    component.todos.set([todo]);
    todoServiceSpy.delete.and.returnValue(of(undefined));

    component.deleteTodo(todo.id);

    expect(todoServiceSpy.delete).toHaveBeenCalledWith(todo.id);
    expect(component.todos()).not.toContain(todo);
  });

  // ── Toggling todos ─────────────────────────────────────────────────────────

  it('should call toggle and update the todo in the list', () => {
    const todo = makeTodo({ isCompleted: false });
    const toggled = { ...todo, isCompleted: true };
    component.todos.set([todo]);
    todoServiceSpy.toggle.and.returnValue(of(toggled));

    component.toggleTodo(todo.id);

    expect(todoServiceSpy.toggle).toHaveBeenCalledWith(todo.id);
    expect(component.todos()[0].isCompleted).toBeTrue();
  });

  // ── Computed signals ───────────────────────────────────────────────────────

  it('remainingCount should reflect incomplete todos', () => {
    component.todos.set([
      makeTodo({ isCompleted: false }),
      makeTodo({ isCompleted: false }),
      makeTodo({ isCompleted: true })
    ]);

    expect(component.remainingCount()).toBe(2);
  });

  it('completedCount should reflect completed todos', () => {
    component.todos.set([
      makeTodo({ isCompleted: true }),
      makeTodo({ isCompleted: false })
    ]);

    expect(component.completedCount()).toBe(1);
  });

  // ── trackById ──────────────────────────────────────────────────────────────

  it('trackById should return the todo id', () => {
    const todo = makeTodo();
    expect(component.trackById(0, todo)).toBe(todo.id);
  });
});
