import React from "react";
import { Todo } from "../types/todo";

interface Props {
    todo: Todo;
    onToggle: (id: number) => void;
    onDelete: (id: number) => void;
}

export const TodoItem: React.FC<Props> = ({ todo, onToggle, onDelete }) => {
    return (
        <li className={`todo-item ${todo.completed ? "completed" : ""}`}>
            <input
                type="checkbox"
                checked={todo.completed}
                onChange={() => onToggle(todo.id)}
            />
            <span>{todo.title}</span>
            <button onClick={() => onDelete(todo.id)}>Delete</button>
        </li>
    );
};
