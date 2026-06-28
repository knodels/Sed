import { useEffect } from 'react';
import { Link, Outlet, useNavigate } from 'react-router-dom';

const Layout: React.FC = () => {
  const navigate = useNavigate();
  const token = localStorage.getItem('token');
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    if (!token) {
      navigate('/login');
    }
  }, [token, navigate]);

  if (!token) {
    return null;
  }

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  return (
    <div>
      <nav>
        <Link to="/documents">Документы</Link>
        <Link to="/create">Создать</Link>
        <Link to="/cabinet">Личный кабинет</Link>
        <span>{user.name} ({user.role})</span>
        <button onClick={handleLogout}>Выйти</button>
      </nav>
      <div style={{ padding: 30, maxWidth: 1200, margin: '0 auto' }}>
        <Outlet />
      </div>
    </div>
  );
};

export default Layout;