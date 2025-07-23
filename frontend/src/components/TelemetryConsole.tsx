import React, { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import type { LogEntry, ConnectionStatus, TelemetryFilters } from '../types/telemetry';
import TelemetryService from '../services/telemetry';
import './TelemetryConsole.css';

interface TelemetryConsoleProps {
  maxLines?: number;
  autoScroll?: boolean;
  defaultLevel?: string;
  height?: string;
}

const LOG_LEVEL_COLORS = {
  Error: '#dc3545',
  Warning: '#ffc107', 
  Information: '#17a2b8',
  Debug: '#6c757d',
} as const;

const LOG_LEVELS = ['All', 'Error', 'Warning', 'Information', 'Debug'];

export const TelemetryConsole: React.FC<TelemetryConsoleProps> = ({
  maxLines = 500,
  autoScroll = true,
  defaultLevel = 'Warning',
  height = '600px',
}) => {
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>({
    isConnected: false,
  });
  const [filters, setFilters] = useState<TelemetryFilters>({
    level: defaultLevel,
  });
  const [isPaused, setIsPaused] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  const eventSourceRef = useRef<EventSource | null>(null);
  const consoleRef = useRef<HTMLDivElement>(null);
  const autoScrollRef = useRef(autoScroll);
  const logsBufferRef = useRef<LogEntry[]>([]);

  // Update autoScroll ref when prop changes
  useEffect(() => {
    autoScrollRef.current = autoScroll;
  }, [autoScroll]);

  // Filter logs based on search and filters
  const filteredLogs = useMemo(() => {
    let filtered = logs;

    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(
        log =>
          log.message?.toLowerCase().includes(term) ||
          log.category?.toLowerCase().includes(term) ||
          (log.exception && log.exception.toLowerCase().includes(term))
      );
    }

    if (filters.level && filters.level !== 'All') {
      if (filters.level === 'Warning') {
        // Include both Warning and Error for Warning level
        filtered = filtered.filter(log => log.level === 'Warning' || log.level === 'Error');
      } else {
        filtered = filtered.filter(log => log.level === filters.level);
      }
    }

    if (filters.category) {
      filtered = filtered.filter(log => 
        log.category?.toLowerCase().includes(filters.category!.toLowerCase())
      );
    }

    return filtered;
  }, [logs, searchTerm, filters]);

  const scrollToBottom = useCallback(() => {
    if (consoleRef.current && autoScrollRef.current && !isPaused) {
      consoleRef.current.scrollTop = consoleRef.current.scrollHeight;
    }
  }, [isPaused]);

  const addLog = useCallback((newLog: LogEntry) => {
    if (isPaused) {
      // Add to buffer instead of displaying when paused
      logsBufferRef.current.push(newLog);
      return;
    }

    setLogs(prevLogs => {
      const updatedLogs = [...prevLogs, newLog];
      // Trim logs to maxLines to prevent memory issues
      if (updatedLogs.length > maxLines) {
        return updatedLogs.slice(-maxLines);
      }
      return updatedLogs;
    });
  }, [isPaused, maxLines]);

  const handleHeartbeat = useCallback((_data: any) => {
    setConnectionStatus(prev => ({
      ...prev,
      lastHeartbeat: new Date(),
    }));
  }, []);

  const connectToStream = useCallback(() => {
    // Close existing connection
    if (eventSourceRef.current) {
      eventSourceRef.current.close();
    }

    const level = filters.level === 'All' ? '' : filters.level || 'Warning';
    
    const eventSource = TelemetryService.createLogStream(
      level,
      50,
      addLog,
      handleHeartbeat,
      (error) => {
        console.error('Stream error:', error);
        setConnectionStatus(prev => ({
          ...prev,
          isConnected: false,
          reconnectAttempts: (prev.reconnectAttempts || 0) + 1,
        }));
        
        // Attempt to reconnect after a delay
        setTimeout(() => {
          if (!isPaused) {
            connectToStream();
          }
        }, 5000);
      },
      () => {
        setConnectionStatus(prev => ({
          ...prev,
          isConnected: true,
          reconnectAttempts: 0,
        }));
      },
      () => {
        setConnectionStatus(prev => ({
          ...prev,
          isConnected: false,
        }));
      }
    );

    eventSourceRef.current = eventSource;
  }, [filters.level, addLog, handleHeartbeat, isPaused]);

  // Connect to stream on mount and when filters change
  useEffect(() => {
    if (!isPaused) {
      connectToStream();
    }

    return () => {
      if (eventSourceRef.current) {
        eventSourceRef.current.close();
      }
    };
  }, [connectToStream, isPaused]);

  // Auto-scroll when new logs are added
  useEffect(() => {
    scrollToBottom();
  }, [filteredLogs, scrollToBottom]);

  const handlePauseToggle = () => {
    setIsPaused(prev => {
      const newPaused = !prev;
      
      if (!newPaused) {
        // Resuming: add buffered logs and reconnect
        if (logsBufferRef.current.length > 0) {
          setLogs(prevLogs => {
            const combined = [...prevLogs, ...logsBufferRef.current];
            logsBufferRef.current = [];
            return combined.slice(-maxLines);
          });
        }
        connectToStream();
      } else {
        // Pausing: disconnect stream  
        if (eventSourceRef.current) {
          eventSourceRef.current.close();
        }
      }
      
      return newPaused;
    });
  };

  const handleClear = () => {
    setLogs([]);
    logsBufferRef.current = [];
  };

  const handleLevelChange = (level: string) => {
    setFilters(prev => ({ ...prev, level }));
  };

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleTimeString('en-US', {
      hour12: false,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      fractionalSecondDigits: 3,
    });
  };

  const getLogLevelBadge = (level: string) => (
    <span 
      className="log-level-badge" 
      style={{ backgroundColor: LOG_LEVEL_COLORS[level as keyof typeof LOG_LEVEL_COLORS] }}
    >
      {level.toUpperCase()}
    </span>
  );

  return (
    <div className="telemetry-console">
      {/* Header Controls */}
      <div className="console-header">
        <div className="console-title">
          <h3>Live Telemetry Console</h3>
          <div className="connection-status">
            <div className={`status-indicator ${connectionStatus.isConnected ? 'connected' : 'disconnected'}`} />
            <span className="status-text">
              {connectionStatus.isConnected ? 'Connected' : 'Disconnected'}
              {connectionStatus.reconnectAttempts ? ` (${connectionStatus.reconnectAttempts} reconnects)` : ''}
            </span>
          </div>
        </div>

        <div className="console-controls">
          <button 
            className={`control-btn ${showFilters ? 'active' : ''}`}
            onClick={() => setShowFilters(!showFilters)}
          >
            🔍 Filters
          </button>
          
          <select 
            value={filters.level || 'Warning'} 
            onChange={(e) => handleLevelChange(e.target.value)}
            className="level-select"
          >
            {LOG_LEVELS.map(level => (
              <option key={level} value={level}>{level}</option>
            ))}
          </select>

          <button 
            className={`control-btn ${isPaused ? 'active' : ''}`}
            onClick={handlePauseToggle}
          >
            {isPaused ? '▶️ Resume' : '⏸️ Pause'}
          </button>

          <button className="control-btn" onClick={handleClear}>
            🗑️ Clear
          </button>

          <div className="log-count">
            {filteredLogs.length} / {logs.length} logs
            {logsBufferRef.current.length > 0 && (
              <span className="buffered-count">
                (+{logsBufferRef.current.length} buffered)
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Filter Panel */}
      {showFilters && (
        <div className="filter-panel">
          <input
            type="text"
            placeholder="Search logs..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          
          <input
            type="text"
            placeholder="Filter by category..."
            value={filters.category || ''}
            onChange={(e) => setFilters(prev => ({ ...prev, category: e.target.value || undefined }))}
            className="category-input"
          />
        </div>
      )}

      {/* Console Display */}
      <div 
        ref={consoleRef}
        className="console-display" 
        style={{ height }}
      >
        {filteredLogs.length === 0 ? (
          <div className="console-placeholder">
            {isPaused ? 'Console paused - click Resume to continue' : 'Waiting for log entries...'}
          </div>
        ) : (
          filteredLogs.map((log, index) => (
            <div key={`${log.id}-${index}`} className={`log-entry log-${(log.level || 'unknown').toLowerCase()}`}>
              <div className="log-header">
                <span className="log-timestamp">{formatTimestamp(log.timestamp)}</span>
                {log.level && getLogLevelBadge(log.level)}
                <span className="log-category">{log.category || 'Unknown'}</span>
                {log.traceId && (
                  <span className="log-trace" title={`Trace: ${log.traceId}`}>
                    🔗 {log.traceId.substring(0, 8)}...
                  </span>
                )}
              </div>
              
              <div className="log-message">{log.message || 'No message'}</div>
              
              {log.exception && (
                <details className="log-exception">
                  <summary>Exception Details</summary>
                  <pre>{log.exception}</pre>
                </details>
              )}
              
              {(log.requestPath || log.userId) && (
                <div className="log-metadata">
                  {log.requestPath && <span className="log-path">Path: {log.requestPath}</span>}
                  {log.userId && <span className="log-user">User: {log.userId}</span>}
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default TelemetryConsole;