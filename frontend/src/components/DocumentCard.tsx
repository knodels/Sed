import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import api from '../api';

interface ApprovalRoute {
  id: number;
  order: number;
  status: string;
  comment: string | null;
  signedAt: string | null;
  approver: string;
}

interface DocumentVersion {
  id: number;
  filePath: string;
  versionNumber: number;
  createdAt: string;
}

interface User {
  id: number;
  name: string;
  email: string;
  role: string;
}

interface Document {
  id: number;
  title: string;
  type: string;
  content: string;
  status: string;
  routeType: string;
  author: string;
  createdAt: string;
  approvalRoutes: ApprovalRoute[];
  documentVersions: DocumentVersion[];
}

const statusLabels: Record<string, string> = {
  Draft: 'Черновик',
  Pending: 'На согласовании',
  Approved: 'Утверждён',
  Rejected: 'Отклонён',
  Revision: 'Доработка'
};

const DocumentCard: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [document, setDocument] = useState<Document | null>(null);
  const [comment, setComment] = useState('');
  const [showSendForm, setShowSendForm] = useState(false);
  const [users, setUsers] = useState<User[]>([]);
  const [selectedApprovers, setSelectedApprovers] = useState<number[]>([]);
  const [routeType, setRouteType] = useState('Sequential');

  const fetchDocument = async () => {
    const response = await api.get(`/documents/${id}`);
    setDocument(response.data);
  };

  useEffect(() => {
    fetchDocument();
  }, [id]);

  const handleSign = async () => {
    await api.post(`/documents/${id}/sign`, { comment });
    setComment('');
    fetchDocument();
  };

  const handleReject = async () => {
    if (!comment.trim()) {
      alert('При отклонении комментарий обязателен');
      return;
    }
    await api.post(`/documents/${id}/reject`, { comment });
    setComment('');
    fetchDocument();
  };

  const handleRevise = async () => {
    await api.post(`/documents/${id}/revise`);
    fetchDocument();
  };

  const handleOpenSendForm = async () => {
    try {
      const response = await api.get('/users');
      const approversAndManagers = response.data.filter(
        (u: User) => u.role === 'Approver' || u.role === 'Manager'
      );
      setUsers(approversAndManagers);
      setSelectedApprovers([]);
      setRouteType('Sequential');
      setShowSendForm(true);
    } catch {
      alert('Не удалось загрузить список пользователей');
    }
  };

  const handleToggleApprover = (userId: number) => {
    setSelectedApprovers((prev) =>
      prev.includes(userId)
        ? prev.filter((id) => id !== userId)
        : [...prev, userId]
    );
  };

  const handleMoveUp = (index: number) => {
    if (index === 0) return;
    const newList = [...selectedApprovers];
    [newList[index - 1], newList[index]] = [newList[index], newList[index - 1]];
    setSelectedApprovers(newList);
  };

  const handleMoveDown = (index: number) => {
    if (index === selectedApprovers.length - 1) return;
    const newList = [...selectedApprovers];
    [newList[index], newList[index + 1]] = [newList[index + 1], newList[index]];
    setSelectedApprovers(newList);
  };

  const handleSend = async () => {
    if (selectedApprovers.length === 0) {
      alert('Выберите хотя бы одного согласующего');
      return;
    }

    const approvers = selectedApprovers.map((userId, index) => ({
      userId,
      order: index + 1
    }));

    try {
      await api.post(`/documents/${id}/send`, {
        routeType,
        approvers
      });
      setShowSendForm(false);
      fetchDocument();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Ошибка при отправке');
    }
  };

  if (!document) {
    return <div>Загрузка...</div>;
  }

  const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
  const isAuthor = currentUser.name === document.author;

  return (
    <div className="document-card">
      <h1>{document.title}</h1>
      <p>Тип: {document.type}</p>
      <p>Статус: {statusLabels[document.status] || document.status}</p>
      <p>Автор: {document.author}</p>
      <p>Создан: {new Date(document.createdAt).toLocaleString()}</p>
      <div style={{ margin: '20px 0', padding: 15, background: '#f8f9fa', borderRadius: 8 }}>
        {document.content}
      </div>

      {document.status === 'Draft' && isAuthor && !showSendForm && (
        <div className="actions">
          <button
            onClick={handleOpenSendForm}
            style={{ background: '#3498db' }}
          >
            Отправить на согласование
          </button>
        </div>
      )}

      {showSendForm && (
        <div className="actions">
          <h3>Отправка на согласование</h3>

          <div style={{ marginBottom: 15 }}>
            <label style={{ marginRight: 20 }}>
              <input
                type="radio"
                value="Sequential"
                checked={routeType === 'Sequential'}
                onChange={() => setRouteType('Sequential')}
              />
              Последовательный
            </label>
            <label>
              <input
                type="radio"
                value="Parallel"
                checked={routeType === 'Parallel'}
                onChange={() => setRouteType('Parallel')}
              />
              Параллельный
            </label>
          </div>

          <div style={{ marginBottom: 15 }}>
            <p>Выберите согласующих:</p>
            {users.map((user) => (
              <label key={user.id} style={{ display: 'block', marginBottom: 5 }}>
                <input
                  type="checkbox"
                  checked={selectedApprovers.includes(user.id)}
                  onChange={() => handleToggleApprover(user.id)}
                />
                {user.name} ({user.role === 'Manager' ? 'Руководитель' : 'Согласующий'})
              </label>
            ))}
          </div>

          {routeType === 'Sequential' && selectedApprovers.length > 0 && (
            <div style={{ marginBottom: 15 }}>
              <p>Порядок согласования:</p>
              <ol>
                {selectedApprovers.map((userId, index) => {
                  const user = users.find((u) => u.id === userId);
                  return (
                    <li key={userId} style={{ marginBottom: 5 }}>
                      {user?.name}
                      <button
                        onClick={() => handleMoveUp(index)}
                        disabled={index === 0}
                        style={{ marginLeft: 10, padding: '2px 8px', fontSize: 12 }}
                      >
                        ↑
                      </button>
                      <button
                        onClick={() => handleMoveDown(index)}
                        disabled={index === selectedApprovers.length - 1}
                        style={{ marginLeft: 5, padding: '2px 8px', fontSize: 12 }}
                      >
                        ↓
                      </button>
                    </li>
                  );
                })}
              </ol>
            </div>
          )}

          <button onClick={handleSend} style={{ background: '#27ae60' }}>
            Отправить
          </button>
          <button
            onClick={() => setShowSendForm(false)}
            style={{ background: '#95a5a6', marginLeft: 10 }}
          >
            Отмена
          </button>
        </div>
      )}

      {document.status === 'Pending' && document.approvalRoutes.some(
        (route) => route.approver === currentUser.name && route.status === 'Ожидает'
      ) && (
        <div className="actions">
          <textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Комментарий"
          />
          <button onClick={handleSign}>Подписать</button>
          <button onClick={handleReject}>Отклонить</button>
          <button onClick={handleRevise}>Запросить доработку</button>
        </div>
      )}

      <h2>Маршрут согласования</h2>
      {document.approvalRoutes.length === 0 && <p>Документ ещё не отправлен на согласование</p>}
      {document.approvalRoutes.map((route) => (
        <div key={route.id} style={{ padding: 10, marginBottom: 5, background: '#f8f9fa', borderRadius: 4 }}>
          <p>Шаг {route.order}: {route.approver} — {route.status}</p>
          {route.comment && <p>Комментарий: {route.comment}</p>}
          {route.signedAt && <p>Подписано: {new Date(route.signedAt).toLocaleString()}</p>}
        </div>
      ))}

      <h2>История версий</h2>
      {document.documentVersions.length === 0 && <p>Нет загруженных файлов</p>}
      {document.documentVersions.map((version) => (
        <div key={version.id} style={{ padding: 10, marginBottom: 5, background: '#f8f9fa', borderRadius: 4 }}>
          <p>Версия {version.versionNumber} от {new Date(version.createdAt).toLocaleString()}</p>
          <a href={version.filePath}>Скачать</a>
        </div>
      ))}
    </div>
  );
};

export default DocumentCard;