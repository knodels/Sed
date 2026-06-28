import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import Login from './components/Login';
import DocumentList from './components/DocumentList';
import DocumentCard from './components/DocumentCard';
import CreateDocument from './components/CreateDocument';
import PersonalCabinet from './components/PersonalCabinet';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/documents" />} />
          <Route path="/documents" element={<DocumentList />} />
          <Route path="/documents/:id" element={<DocumentCard />} />
          <Route path="/create" element={<CreateDocument />} />
          <Route path="/cabinet" element={<PersonalCabinet />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;