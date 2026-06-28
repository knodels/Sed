import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

const CreateDocument: React.FC = () => {
  const [title, setTitle] = useState('');
  const [type, setType] = useState('Memo');
  const [content, setContent] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await api.post('/documents', { title, type, content });
    navigate('/documents');
  };

  return (
    <div>
      <h1>Создание документа</h1>
      <form onSubmit={handleSubmit}>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Название документа"
          required
        />
        <select value={type} onChange={(e) => setType(e.target.value)}>
          <option value="Order">Приказ</option>
          <option value="Contract">Договор</option>
          <option value="Memo">Служебная записка</option>
          <option value="Letter">Письмо</option>
        </select>
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="Содержание документа"
          required
        />
        <button type="submit">Создать</button>
      </form>
    </div>
  );
};

export default CreateDocument;