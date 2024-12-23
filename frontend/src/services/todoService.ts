import { Todo, TodoStatus } from '@/types/todo';

const API_URL = 'http://localhost:5247/api/todo';

export const todoService = {
    async getAllTodos(): Promise<Todo[]> {
        const response = await fetch(API_URL);
        return response.json();
    },

    async getTodoById(id: number): Promise<Todo> {
        const response = await fetch(`${API_URL}/${id}`);
        return response.json();
    },

    async getTodosByStatus(status: TodoStatus): Promise<Todo[]> {
        const response = await fetch(`${API_URL}/status/${status}`);
        return response.json();
    },

    async createTodo(todo: Omit<Todo, 'id' | 'createdAt'>): Promise<Todo> {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(todo),
        });
        return response.json();
    },

    async updateTodo(id: number, todo: Todo): Promise<void> {
        await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(todo),
        });
    },

    async updateTodoStatus(id: number, status: TodoStatus): Promise<Todo> {
        const response = await fetch(`${API_URL}/${id}/status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ status: status }),
        });
        return response.json();
    },

    async deleteTodo(id: number): Promise<void> {
        await fetch(`${API_URL}/${id}`, {
            method: 'DELETE',
        });
    },
};
