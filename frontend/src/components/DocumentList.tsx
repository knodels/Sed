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

const typeLabels: Record<string, string> = {
  Order: 'Приказ',
  Contract: 'Договор',
  Memo: 'Служебная записка',
  Letter: 'Письмо'
};

const statusLabels: Record<string, string> = {
  Draft: 'Черновик',
  Pending: 'На согласовании',
  Approved: 'Утверждён',
  Rejected: 'Отклонён',
  Revision: 'Доработка',
  Archived: 'Архив'
};

const DocumentList: React.FC = () => {
  const [documents, setDocuments] = useState<Document[]>([]);
  const [status, setStatus] = useState('');
  const [type, setType] = useState('');
  const [search, setSearch] = useState('');

  const fetchDocuments = async () => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (type) params.append('type', type);
    if (search) params.append('search', search);
    const response = await api.get(`/documents?${params}`);
    setDocuments(response.data);
  };

  useEffect(() => {
    fetchDocuments();
  }, [status, type]);

  const handleSearch = () => {
    fetchDocuments();
  };

  return (
    <div>
      <h1>Документы</h1>
      <div className="filters">
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">Все статусы</option>
          <option value="Draft">Черновик</option>
          <option value="Pending">На согласовании</option>
          <option value="Approved">Утверждён</option>
          <option value="Rejected">Отклонён</option>
        </select>
        <select value={type} onChange={(e) => setType(e.target.value)}>
          <option value="">Все типы</option>
          <option value="Order">Приказ</option>
          <option value="Contract">Договор</option>
          <option value="Memo">Служебная записка</option>
          <option value="Letter">Письмо</option>
        </select>
        <input
          type="text"
          placeholder="Поиск..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button onClick={handleSearch}>Найти</button>
      </div>
      <table>
        <thead>
          <tr>
            <th>Название</th>
            <th>Тип</th>
            <th>Статус</th>
            <th>Автор</th>
            <th>Дата</th>
          </tr>
        </thead>
        <tbody>
          {documents.map((doc) => (
            <tr key={doc.id}>
              <td>
                <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
              </td>
              <td>{typeLabels[doc.type] || doc.type}</td>
              <td>{statusLabels[doc.status] || doc.status}</td>
              <td>{doc.author}</td>
              <td>{new Date(doc.createdAt).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default DocumentList;