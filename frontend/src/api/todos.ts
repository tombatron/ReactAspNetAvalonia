import {Todo} from '../types/todo';

const BASE_URL = 'app://api/todos';

export async function fetchTodos(): Promise<Todo[]> {
    const res = await fetch(BASE_URL);
    if (!res.ok) {
        throw new Error("Failed to fetch todos");
    }
    return res.json();
}

export async function addTodo(title: string): Promise<Todo> {
    const res = await fetch(BASE_URL, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({title})
    });
    if (!res.ok) throw new Error("Failed to add todo");
    return res.json();
}

export async function toggleTodo(id: number): Promise<Todo> {
    const res = await fetch(`${BASE_URL}/${id}/toggle`, {
        method: "PATCH"
    });
    if (!res.ok) throw new Error("Failed to toggle todo");
    return res.json();
}

export async function deleteTodo(id: number): Promise<void> {
    const res = await fetch(`${BASE_URL}/${id}`, {
        method: "DELETE"
    });

    if (!res.ok) {
        throw new Error("Failed to delete todo");
    }
}