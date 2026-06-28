import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

interface Document {
  id: number;
  title: string;
  type: string;
  status: string;
  author: string;
  createdAt: string;
}

const PersonalCabinet: React.FC = () => {
  const [tasks, setTasks] = useState<Document[]>([]);

  useEffect(() => {
    api.get('/documents?status=Pending').then((response) => {
      setTasks(response.data);
    });
  }, []);

  return (
    <div>
      <h1>Личный кабинет</h1>
      <h2>Задачи на подпись</h2>
      {tasks.length === 0 && <p>Нет задач на подпись</p>}
      {tasks.map((task) => (
        <div key={task.id} style={{ padding: 15, marginBottom: 10, background: 'white', borderRadius: 8 }}>
          <Link to={`/documents/${task.id}`}>{task.title}</Link>
          <span style={{ marginLeft: 20, color: '#7f8c8d' }}>{task.author}</span>
          <span style={{ marginLeft: 20, color: '#7f8c8d' }}>{new Date(task.createdAt).toLocaleDateString()}</span>
        </div>
      ))}
    </div>
  );
};

export default PersonalCabinet;