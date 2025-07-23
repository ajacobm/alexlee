// Telemetry-specific types for the frontend

export interface LogEntry {
  id: number;
  timestamp: string;
  level: 'Error' | 'Warning' | 'Information' | 'Debug';
  category: string;
  message: string;
  exception?: string;
  traceId?: string;
  spanId?: string;
  requestPath?: string;
  userId?: string;
  machineName?: string;
  processId?: number;
}

export interface LogsResponse {
  data: LogEntry[];
  pagination: {
    page: number;
    size: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  filters: {
    level?: string;
    category?: string;
    fromDate?: string;
    toDate?: string;
  };
}

export interface LogStatistics {
  totalLogs: number;
  logsByLevel: Array<{
    level: string;
    count: number;
  }>;
  recentActivity: {
    lastHour: {
      total: number;
      errors: number;
      warnings: number;
    };
    lastDay: {
      total: number;
      errors: number;
      warnings: number;
    };
    lastWeek: {
      total: number;
      errors: number;
      warnings: number;
    };
  };
  topCategories: Array<{
    category: string;
    count: number;
  }>;
  lastUpdated: string;
}

export interface TelemetryFilters {
  level?: string;
  category?: string;
  fromDate?: Date | null;
  toDate?: Date | null;
  search?: string;
}

export interface ConnectionStatus {
  isConnected: boolean;
  connectionId?: string;
  lastHeartbeat?: Date;
  reconnectAttempts?: number;
}

export interface HeartbeatEvent {
  type: 'heartbeat';
  timestamp: string;
  activeConnections: string;
}