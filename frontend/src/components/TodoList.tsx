'use client';

import { useEffect, useState } from 'react';
import { Todo, TodoStatus } from '@/types/todo';
import { todoService } from '@/services/todoService';

export default function TodoList() {
    const [todos, setTodos] = useState<Todo[]>([]);
    const [newTodo, setNewTodo] = useState({ 
        title: '', 
        description: '', 
        status: TodoStatus.Todo,  
        dueDate: '' 
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        loadTodos();
    }, []);

    const loadTodos = async () => {
        try {
            const data = await todoService.getAllTodos();
            setTodos(data);
            setLoading(false);
        } catch (err) {
            setError('Failed to load todos');
            setLoading(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const todo = await todoService.createTodo(newTodo);
            setTodos([...todos, todo]);
            setNewTodo({ 
                title: '', 
                description: '', 
                status: TodoStatus.Todo,  
                dueDate: '' 
            });
        } catch (err) {
            setError('Failed to create todo');
        }
    };

    const handleStatusChange = async (id: number, newStatus: TodoStatus) => {
        try {
            console.log('Updating status to:', newStatus);
            const updatedTodo = await todoService.updateTodoStatus(id, newStatus);
            console.log('Updated todo:', updatedTodo);
            setTodos(prevTodos => 
                prevTodos.map(todo => todo.id === id ? updatedTodo : todo)
            );
        } catch (err) {
            console.error('Error updating status:', err);
            setError('Failed to update todo status');
        }
    };

    const handleDelete = async (id: number) => {
        try {
            await todoService.deleteTodo(id);
            setTodos(todos.filter(todo => todo.id !== id));
        } catch (err) {
            setError('Failed to delete todo');
        }
    };

    const formatDate = (dateString: string) => {
        if (!dateString || dateString === '0001-01-01T00:00:00') return 'No due date';
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return 'No due date';
        return date.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <div className="text-red-500">{error}</div>;

    return (
        <div className="max-w-4xl mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Todo List</h1>
            
            {/* Add Todo Form */}
            <form onSubmit={handleSubmit} className="mb-8 space-y-4">
                <div>
                    <input
                        type="text"
                        placeholder="Title"
                        value={newTodo.title}
                        onChange={e => setNewTodo({ ...newTodo, title: e.target.value })}
                        className="w-full p-2 border rounded"
                        required
                    />
                </div>
                <div>
                    <textarea
                        placeholder="Description"
                        value={newTodo.description}
                        onChange={e => setNewTodo({ ...newTodo, description: e.target.value })}
                        className="w-full p-2 border rounded"
                        required
                    />
                </div>
                <div>
                    <input
                        type="date"
                        value={newTodo.dueDate}
                        onChange={e => setNewTodo({ ...newTodo, dueDate: e.target.value })}
                        className="w-full p-2 border rounded"
                    />
                </div>
                <button
                    type="submit"
                    className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
                >
                    Add Todo
                </button>
            </form>

            {/* Todo List */}
            <div className="space-y-4">
                {todos.map(todo => (
                    <div
                        key={todo.id}
                        className="border p-4 rounded shadow-sm hover:shadow-md transition-shadow"
                    >
                        <div className="flex justify-between items-start">
                            <div>
                                <h3 className="font-semibold">{todo.title}</h3>
                                <p className="text-gray-600">{todo.description}</p>
                                <p className="text-sm text-gray-500">
                                    Due: {formatDate(todo.dueDate)}
                                </p>
                            </div>
                            <div className="flex space-x-2">
                                <select
                                    value={todo.status}
                                    onChange={e => handleStatusChange(todo.id, parseInt(e.target.value) as TodoStatus)}
                                    className="border rounded p-1"
                                >
                                    <option value={TodoStatus.Todo}>Todo</option>
                                    <option value={TodoStatus.Doing}>Doing</option>
                                    <option value={TodoStatus.Done}>Done</option>
                                </select>
                                <button
                                    onClick={() => handleDelete(todo.id)}
                                    className="text-red-500 hover:text-red-700"
                                >
                                    Delete
                                </button>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}
