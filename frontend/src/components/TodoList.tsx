import React, {useEffect, useState} from "react";
import {Todo} from "../types/todo";
import {fetchTodos, addTodo, toggleTodo, deleteTodo} from "../api/todos";
import {TodoItem} from "./TodoItem";
import TodoStatusMessages from "./TodoStatusMessages";
import {LocalLink} from "./LocalLink";

export const TodoList: React.FC = () => {
    const [todos, setTodos] = useState<Todo[]>([]);
    const [newTitle, setNewTitle] = useState("");
    const [loading, setLoading] = useState(false);

    // Function to load all todos
    const loadTodos = async () => {
        setLoading(true);
        try {
            const data = await fetchTodos();
            setTodos(data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    // Load todos on mount
    useEffect(() => {
        loadTodos();
    }, []);

    const handleAdd = async () => {
        if (!newTitle.trim()) return;
        const todo = await addTodo(newTitle);
        setTodos(prev => [...prev, todo]);
        setNewTitle("");
    };

    const handleToggle = async (id: number) => {
        const updated = await toggleTodo(id);
        setTodos(prev => prev.map(t => t.id === id ? updated : t));
    };

    const handleDelete = async (id: number) => {
        await deleteTodo(id);
        setTodos(prev => prev.filter(t => t.id !== id));
    };

    return (
        <>
            <div className="todo-list">
                <h1>Todo List</h1>

                <div className="controls">
                    <div className="add-todo">
                        <input
                            type="text"
                            value={newTitle}
                            onChange={e => setNewTitle(e.target.value)}
                            placeholder="New task"
                        />
                        <button onClick={handleAdd}>Add</button>
                    </div>

                    <button onClick={loadTodos} disabled={loading}>
                        {loading ? "Refreshing..." : "Refresh"}
                    </button>
                </div>

                <ul>
                    {todos.map(todo => (
                        <TodoItem
                            key={todo.id}
                            todo={todo}
                            onToggle={handleToggle}
                            onDelete={handleDelete}
                        />
                    ))}
                </ul>
            </div>
            <div>
                <LocalLink fileName={"sample"} text={"Click here for status messages."} />
            </div>
        </>
    );
};
