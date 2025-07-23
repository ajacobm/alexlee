import React, { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import TelemetryConsole from '../components/TelemetryConsole';
import TelemetryService from '../services/telemetry';
import type { LogStatistics } from '../types/telemetry';
import './TelemetryPage.css';

const TelemetryPage: React.FC = () => {
  const [connectionTestResult, setConnectionTestResult] = useState<boolean | null>(null);

  // Fetch log statistics
  const { 
    data: stats, 
    isLoading: statsLoading, 
    error: statsError, 
    refetch: refetchStats 
  } = useQuery({
    queryKey: ['telemetry-stats'],
    queryFn: TelemetryService.getLogStatistics,
    refetchInterval: 30000, // Refetch every 30 seconds
    retry: 3,
  });

  // Fetch metrics data
  const { 
    data: metrics, 
    isLoading: metricsLoading, 
    error: metricsError 
  } = useQuery({
    queryKey: ['telemetry-metrics'],
    queryFn: TelemetryService.getMetrics,
    refetchInterval: 30000,
    retry: 3,
  });

  // Test API connection on mount
  useEffect(() => {
    const testConnection = async () => {
      const result = await TelemetryService.testConnection();
      setConnectionTestResult(result);
    };
    testConnection();
  }, []);

  const formatNumber = (num: number): string => {
    return num.toLocaleString();
  };

  const formatDuration = (date: string): string => {
    const now = new Date();
    const then = new Date(date);
    const diffMs = now.getTime() - then.getTime();
    const diffMins = Math.round(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffMins < 1440) return `${Math.round(diffMins / 60)}h ago`;
    return `${Math.round(diffMins / 1440)}d ago`;
  };

  const getHealthStatus = (stats: LogStatistics) => {
    const errorRate = stats.recentActivity.lastHour.errors / Math.max(stats.recentActivity.lastHour.total, 1);
    if (errorRate > 0.1) return 'critical';
    if (errorRate > 0.05) return 'warning';
    return 'healthy';
  };

  const renderStatCard = (
    title: string, 
    value: string | number, 
    subtitle?: string, 
    trend?: 'up' | 'down' | 'neutral',
    colorClass?: string
  ) => (
    <div className={`stat-card ${colorClass || ''}`}>
      <div className="stat-header">
        <h4 className="stat-title">{title}</h4>
        {trend && (
          <span className={`trend-indicator trend-${trend}`}>
            {trend === 'up' ? '📈' : trend === 'down' ? '📉' : '➡️'}
          </span>
        )}
      </div>
      <div className="stat-value">{typeof value === 'number' ? formatNumber(value) : value}</div>
      {subtitle && <div className="stat-subtitle">{subtitle}</div>}
    </div>
  );

  if (statsError || metricsError) {
    return (
      <div className="telemetry-page">
        <div className="page-header">
          <h1>Telemetry Dashboard</h1>
          <div className="status-badge error">
            ❌ API Connection Failed
          </div>
        </div>
        
        <div className="error-section">
          <div className="error-card">
            <h3>Unable to load telemetry data</h3>
            <p>Please check that the backend API is running and accessible.</p>
            <div className="error-details">
              {statsError && <div>Stats Error: {statsError.message}</div>}
              {metricsError && <div>Metrics Error: {metricsError.message}</div>}
            </div>
            <button 
              className="retry-btn"
              onClick={() => {
                refetchStats();
                window.location.reload();
              }}
            >
              🔄 Retry Connection
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="telemetry-page">
      {/* Header */}
      <div className="page-header">
        <div className="header-content">
          <h1>Telemetry Dashboard</h1>
          <p className="page-description">
            Real-time monitoring and logging for the Alex Lee Developer Exercise API
          </p>
        </div>
        
        <div className="header-status">
          <div className={`status-badge ${connectionTestResult ? 'success' : 'error'}`}>
            {connectionTestResult ? '✅ API Connected' : '❌ API Disconnected'}
          </div>
          
          {stats && (
            <div className={`status-badge ${getHealthStatus(stats)}`}>
              {getHealthStatus(stats) === 'healthy' ? '💚' : 
               getHealthStatus(stats) === 'warning' ? '⚠️' : '🔥'} 
              System {getHealthStatus(stats) === 'healthy' ? 'Healthy' : 
                      getHealthStatus(stats) === 'warning' ? 'Warning' : 'Critical'}
            </div>
          )}
          
          {stats && (
            <div className="last-updated">
              Updated {formatDuration(stats.lastUpdated)}
            </div>
          )}
        </div>
      </div>

      {/* Loading State */}
      {(statsLoading || metricsLoading) && (
        <div className="loading-section">
          <div className="loading-spinner"></div>
          <p>Loading telemetry data...</p>
        </div>
      )}

      {/* Dashboard Content */}
      {stats && (
        <div className="dashboard-content">
          {/* Overview Cards */}
          <div className="overview-section">
            <h2 className="section-title">System Overview</h2>
            <div className="stats-grid">
              {renderStatCard(
                'Total Logs',
                stats.totalLogs,
                'All time',
                'neutral',
                'primary'
              )}
              
              {renderStatCard(
                'Recent Errors',
                stats.recentActivity.lastHour.errors,
                'Last hour',
                stats.recentActivity.lastHour.errors > stats.recentActivity.lastDay.errors / 24 ? 'up' : 'down',
                stats.recentActivity.lastHour.errors > 0 ? 'danger' : 'success'
              )}
              
              {renderStatCard(
                'Recent Warnings',
                stats.recentActivity.lastHour.warnings,
                'Last hour',
                stats.recentActivity.lastHour.warnings > stats.recentActivity.lastDay.warnings / 24 ? 'up' : 'down',
                'warning'
              )}
              
              {renderStatCard(
                'Activity Rate',
                `${stats.recentActivity.lastHour.total}/hr`,
                'Current activity',
                'neutral',
                'info'
              )}
            </div>
          </div>

          {/* Activity Trends */}
          <div className="trends-section">
            <h2 className="section-title">Activity Trends</h2>
            <div className="trends-grid">
              <div className="trend-card">
                <h4>Last Hour</h4>
                <div className="trend-stats">
                  <div className="trend-stat">
                    <span className="trend-label">Total</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastHour.total)}</span>
                  </div>
                  <div className="trend-stat error">
                    <span className="trend-label">Errors</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastHour.errors)}</span>
                  </div>
                  <div className="trend-stat warning">
                    <span className="trend-label">Warnings</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastHour.warnings)}</span>
                  </div>
                </div>
              </div>

              <div className="trend-card">
                <h4>Last Day</h4>
                <div className="trend-stats">
                  <div className="trend-stat">
                    <span className="trend-label">Total</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastDay.total)}</span>
                  </div>
                  <div className="trend-stat error">
                    <span className="trend-label">Errors</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastDay.errors)}</span>
                  </div>
                  <div className="trend-stat warning">
                    <span className="trend-label">Warnings</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastDay.warnings)}</span>
                  </div>
                </div>
              </div>

              <div className="trend-card">
                <h4>Last Week</h4>
                <div className="trend-stats">
                  <div className="trend-stat">
                    <span className="trend-label">Total</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastWeek.total)}</span>
                  </div>
                  <div className="trend-stat error">
                    <span className="trend-label">Errors</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastWeek.errors)}</span>
                  </div>
                  <div className="trend-stat warning">
                    <span className="trend-label">Warnings</span>
                    <span className="trend-value">{formatNumber(stats.recentActivity.lastWeek.warnings)}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Top Categories */}
          {stats.topCategories.length > 0 && (
            <div className="categories-section">
              <h2 className="section-title">Top Log Categories (Last Week)</h2>
              <div className="categories-list">
                {stats.topCategories.slice(0, 8).map((category) => (
                  <div key={category.category} className="category-item">
                    <span className="category-name">{category.category}</span>
                    <span className="category-count">{formatNumber(category.count)}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Live Console */}
          <div className="console-section">
            <h2 className="section-title">Live Log Stream</h2>
            <TelemetryConsole 
              maxLines={300}
              autoScroll={true}
              defaultLevel="Warning"
              height="500px"
            />
          </div>

          {/* Raw Metrics (collapsible) */}
          {metrics && (
            <div className="metrics-section">
              <details className="metrics-details">
                <summary>
                  <h2 className="section-title">Raw Prometheus Metrics</h2>
                </summary>
                <pre className="metrics-output">{metrics}</pre>
              </details>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default TelemetryPage;