export enum TodoStatus {
    Todo = 0,
    Doing = 1,
    Done = 2
}

export interface Todo {
    id: number;
    title: string;
    description: string;
    status: TodoStatus;
    dueDate: string;
    createdAt: string;
}
