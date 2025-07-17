import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import PurchaseDetailsPage from './pages/PurchaseDetailsPage';
import AlgorithmsPage from './pages/AlgorithmsPage';
import DuplicatesPage from './pages/DuplicatesPage';
import './App.css';

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/purchase-details" element={<PurchaseDetailsPage />} />
            <Route path="/algorithms" element={<AlgorithmsPage />} />
            <Route path="/duplicates" element={<DuplicatesPage />} />
          </Routes>
        </Layout>
      </Router>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
