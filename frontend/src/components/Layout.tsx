import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import './Layout.css';
import logo from '../assets/alex_lee-logo.svg';
interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="layout">
      
      <header className="header">
        <div className="header-container">
          <div className="brand">
            <div className="brand-logo">
              <img 
                src={logo} 
                alt="Alex Lee" 
                className="brand-logo-img"
              />
            </div>
            <p className="brand-subtitle">Developer Technical Exercise</p>
          </div>
          <nav className="nav">
            <Link 
              to="/" 
              className={`nav-link ${isActive('/') ? 'active' : ''}`}
            >
              Home
            </Link>
            <Link 
              to="/purchase-details" 
              className={`nav-link ${isActive('/purchase-details') ? 'active' : ''}`}
            >
              Purchase Details
            </Link>
            <Link 
              to="/algorithms" 
              className={`nav-link ${isActive('/algorithms') ? 'active' : ''}`}
            >
              Algorithms
            </Link>
            <Link 
              to="/duplicates" 
              className={`nav-link ${isActive('/duplicates') ? 'active' : ''}`}
            >
              Duplicates
            </Link>
          </nav>
        </div>
      </header>
      
      <main className="main">
        <div className="main-container">
          {children}
        </div>
      </main>
      
      <footer className="footer">
        <div className="footer-container">
          <p className="footer-text">
            © 2025 Alex Lee Developer Exercise. Built with React, .NET 8, and Entity Framework Core.
          </p>
          <div className="footer-links">
            <a href="https://github.com/ajacobm/alexlee" target="_blank" rel="noopener noreferrer">
              GitHub Repository
            </a>
            <span className="separator">•</span>
            <a href="/index.html" target="_blank" rel="noopener noreferrer">
              API Documentation
            </a>
          </div>
        </div>
      </footer>
    </div>
  );
};

export default Layout;
