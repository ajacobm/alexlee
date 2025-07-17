import React from 'react';
import { Link } from 'react-router-dom';

const HomePage: React.FC = () => {
  return (
    <div className="fade-in">
      {/* Hero Section */}
      <div className="card mb-4">
        <div className="card-body text-center">
          <h1 className="mb-3" style={{ fontSize: '2.5rem', background: 'linear-gradient(135deg, var(--alex-primary), var(--alex-secondary))', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
            Alex Lee Developer Exercise
          </h1>
          <p className="text-lg text-secondary mb-4">
            A full-stack technical demonstration showcasing modern development practices, 
            algorithm implementation, and clean architecture patterns.
          </p>
          <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', flexWrap: 'wrap' }}>
            <Link to="/purchase-details" className="btn btn-primary">
              Explore Purchase Details
            </Link>
            <Link to="/algorithms" className="btn btn-secondary">
              Try Algorithms
            </Link>
          </div>
        </div>
      </div>

      {/* Features Grid */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
        {/* C# Algorithms */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">C# Algorithm Problems</h3>
          </div>
          <div className="card-body">
            <ul style={{ listStyle: 'none', padding: 0 }}>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>String Interleaving:</strong> Merge two strings character by character
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Palindrome Checker:</strong> Detect palindromes with smart filtering
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Parallel File Search:</strong> Concurrent text search with performance metrics
              </li>
            </ul>
            <Link to="/algorithms" className="btn btn-primary btn-sm mt-3">
              Test Algorithms →
            </Link>
          </div>
        </div>

        {/* Database Management */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Database Operations</h3>
          </div>
          <div className="card-body">
            <ul style={{ listStyle: 'none', padding: 0 }}>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>CRUD Operations:</strong> Complete Create, Read, Update, Delete functionality
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Advanced Filtering:</strong> Multi-field search with real-time results
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-success)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Duplicate Detection:</strong> SQL-based duplicate identification
              </li>
            </ul>
            <Link to="/purchase-details" className="btn btn-primary btn-sm mt-3">
              Manage Data →
            </Link>
          </div>
        </div>

        {/* Technical Architecture */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Modern Architecture</h3>
          </div>
          <div className="card-body">
            <ul style={{ listStyle: 'none', padding: 0 }}>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-primary)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Clean Architecture:</strong> Domain-driven design with CQRS pattern
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-primary)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Modern Stack:</strong> .NET 8, React 19, TypeScript, Entity Framework
              </li>
              <li style={{ marginBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ width: '8px', height: '8px', backgroundColor: 'var(--alex-primary)', borderRadius: '50%', flexShrink: 0 }}></span>
                <strong>Best Practices:</strong> Unit testing, CI/CD, Docker containerization
              </li>
            </ul>
            <a href="http://localhost:5000" target="_blank" rel="noopener noreferrer" className="btn btn-primary btn-sm mt-3">
              View API Docs →
            </a>
          </div>
        </div>
      </div>

      {/* Technical Details */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">Implementation Highlights</h3>
        </div>
        <div className="card-body">
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '2rem' }}>
            <div>
              <h4 style={{ color: 'var(--alex-primary)', marginBottom: '1rem' }}>Backend Excellence</h4>
              <ul style={{ fontSize: '0.875rem', lineHeight: '1.6' }}>
                <li>29 comprehensive unit tests with 100% pass rate</li>
                <li>CQRS pattern with MediatR for clean separation</li>
                <li>Entity Framework Core with SQLite for rapid deployment</li>
                <li>RESTful API with comprehensive Swagger documentation</li>
                <li>Immutable record types for data integrity</li>
              </ul>
            </div>
            <div>
              <h4 style={{ color: 'var(--alex-primary)', marginBottom: '1rem' }}>Frontend Innovation</h4>
              <ul style={{ fontSize: '0.875rem', lineHeight: '1.6' }}>
                <li>React 19 with TypeScript for type safety</li>
                <li>Alex Lee custom branding and responsive design</li>
                <li>React Query for efficient server state management</li>
                <li>Modal-based CRUD operations with form validation</li>
                <li>Intuitive filtering and search capabilities</li>
              </ul>
            </div>
            <div>
              <h4 style={{ color: 'var(--alex-primary)', marginBottom: '1rem' }}>DevOps Ready</h4>
              <ul style={{ fontSize: '0.875rem', lineHeight: '1.6' }}>
                <li>GitHub Actions CI/CD pipeline configured</li>
                <li>Docker containerization with multi-stage builds</li>
                <li>Professional project structure and documentation</li>
                <li>Automated testing and code quality checks</li>
                <li>Production-ready deployment architecture</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
