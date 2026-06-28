import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await api.post('/auth/login', { email, password });
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
      navigate('/documents');
    } catch {
      setError('Неверный email или пароль');
    }
  };

  return (
    <div style={{ maxWidth: 400, margin: '100px auto', padding: 20, fontFamily: 'Arial' }}>
      <h1>Вход в систему</h1>
      <form onSubmit={handleSubmit}>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Email"
          required
          style={{ width: '100%', padding: 10, marginBottom: 10, boxSizing: 'border-box' }}
        />
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Пароль"
          required
          style={{ width: '100%', padding: 10, marginBottom: 10, boxSizing: 'border-box' }}
        />
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <button type="submit" style={{ width: '100%', padding: 10 }}>Войти</button>
      </form>
      <div style={{ marginTop: 20, fontSize: 14 }}>
        <p><b>Тестовые пользователи:</b></p>
        <p>admin@company.ru / admin123 (Администратор)</p>
        <p>petrov@company.ru / petrov123 (Сотрудник)</p>
        <p>sidorova@company.ru / sidorova123 (Согласующий)</p>
        <p>kozlov@company.ru / kozlov123 (Согласующий)</p>
        <p>morozova@company.ru / morozova123 (Руководитель)</p>
      </div>
    </div>
  );
};

export default Login;