import axios from 'axios';
import type { LogEntry, LogsResponse, LogStatistics, TelemetryFilters } from '../types/telemetry';

// Get the API base URL from environment or use default
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

export class TelemetryService {
  /**
   * Get paginated logs with optional filtering
   */
  static async getLogs(
    page: number = 1,
    size: number = 100,
    filters?: TelemetryFilters
  ): Promise<LogsResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      size: size.toString(),
    });

    if (filters?.level) {
      params.append('level', filters.level);
    }
    if (filters?.category) {
      params.append('category', filters.category);
    }
    if (filters?.fromDate) {
      params.append('fromDate', filters.fromDate.toISOString());
    }
    if (filters?.toDate) {
      params.append('toDate', filters.toDate.toISOString());
    }

    const response = await api.get(`/api/telemetry/logs?${params.toString()}`);
    const rawData = response.data;
    
    // Transform Pascal case properties to camelCase for TypeScript compatibility
    const transformedData: LogsResponse = {
      data: rawData.Data?.map((log: any) => ({
        id: log.Id || log.id,
        timestamp: log.Timestamp || log.timestamp,
        level: log.Level || log.level,
        category: log.Category || log.category,
        message: log.Message || log.message,
        exception: log.Exception || log.exception,
        traceId: log.TraceId || log.traceId,
        spanId: log.SpanId || log.spanId,
        requestPath: log.RequestPath || log.requestPath,
        userId: log.UserId || log.userId,
        machineName: log.MachineName || log.machineName,
        processId: log.ProcessId || log.processId,
      } as LogEntry)) || [],
      pagination: {
        page: rawData.Pagination?.Page || rawData.pagination?.page || page,
        size: rawData.Pagination?.Size || rawData.pagination?.size || size,
        totalCount: rawData.Pagination?.TotalCount || rawData.pagination?.totalCount || 0,
        totalPages: rawData.Pagination?.TotalPages || rawData.pagination?.totalPages || 0,
        hasNextPage: rawData.Pagination?.HasNextPage || rawData.pagination?.hasNextPage || false,
        hasPreviousPage: rawData.Pagination?.HasPreviousPage || rawData.pagination?.hasPreviousPage || false,
      },
      filters: {
        level: rawData.Filters?.Level || rawData.filters?.level,
        category: rawData.Filters?.Category || rawData.filters?.category,
        fromDate: rawData.Filters?.FromDate || rawData.filters?.fromDate,
        toDate: rawData.Filters?.ToDate || rawData.filters?.toDate,
      },
    };
    
    return transformedData;
  }

  /**
   * Get log statistics and trends
   */
  static async getLogStatistics(): Promise<LogStatistics> {
    const response = await api.get('/api/telemetry/stats');
    const rawData = response.data;
    
    // Transform Pascal case properties to camelCase for TypeScript compatibility
    const transformedStats: LogStatistics = {
      totalLogs: rawData.TotalLogs || rawData.totalLogs || 0,
      logsByLevel: rawData.LogsByLevel?.map((item: any) => ({
        level: item.Level || item.level,
        count: item.Count || item.count,
      })) || rawData.logsByLevel || [],
      recentActivity: {
        lastHour: {
          total: rawData.RecentActivity?.LastHour?.Total || rawData.recentActivity?.lastHour?.total || 0,
          errors: rawData.RecentActivity?.LastHour?.Errors || rawData.recentActivity?.lastHour?.errors || 0,
          warnings: rawData.RecentActivity?.LastHour?.Warnings || rawData.recentActivity?.lastHour?.warnings || 0,
        },
        lastDay: {
          total: rawData.RecentActivity?.LastDay?.Total || rawData.recentActivity?.lastDay?.total || 0,
          errors: rawData.RecentActivity?.LastDay?.Errors || rawData.recentActivity?.lastDay?.errors || 0,
          warnings: rawData.RecentActivity?.LastDay?.Warnings || rawData.recentActivity?.lastDay?.warnings || 0,
        },
        lastWeek: {
          total: rawData.RecentActivity?.LastWeek?.Total || rawData.recentActivity?.lastWeek?.total || 0,
          errors: rawData.RecentActivity?.LastWeek?.Errors || rawData.recentActivity?.lastWeek?.errors || 0,
          warnings: rawData.RecentActivity?.LastWeek?.Warnings || rawData.recentActivity?.lastWeek?.warnings || 0,
        },
      },
      topCategories: rawData.TopCategories?.map((item: any) => ({
        category: item.Category || item.category,
        count: item.Count || item.count,
      })) || rawData.topCategories || [],
      lastUpdated: rawData.LastUpdated || rawData.lastUpdated || new Date().toISOString(),
    };
    
    return transformedStats;
  }

  /**
   * Get Prometheus-compatible metrics
   */
  static async getMetrics(): Promise<string> {
    const response = await api.get<string>('/api/telemetry/metrics', {
      headers: {
        'Accept': 'text/plain',
      },
    });
    return response.data;
  }

  /**
   * Create a Server-Sent Events connection for real-time log streaming
   */
  static createLogStream(
    level: string = 'Warning',
    maxLines: number = 50,
    onMessage: (log: LogEntry) => void,
    onHeartbeat?: (data: any) => void,
    onError?: (error: Event) => void,
    onOpen?: () => void,
    onClose?: () => void
  ): EventSource | null {
    if (!window.EventSource) {
      console.error('Server-Sent Events not supported in this browser');
      return null;
    }

    const params = new URLSearchParams({
      level,
      maxLines: maxLines.toString(),
    });

    const eventSource = new EventSource(
      `${API_BASE_URL}/api/telemetry/logs/stream?${params.toString()}`
    );

    eventSource.onopen = () => {
      console.log('Log stream connected');
      onOpen?.();
    };

    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        
        // Handle different event types
        if (data.type === 'heartbeat') {
          onHeartbeat?.(data);
        } else {
          // Transform Pascal case properties to camelCase for TypeScript compatibility
          const transformedLog: LogEntry = {
            id: data.Id || data.id,
            timestamp: data.Timestamp || data.timestamp,
            level: data.Level || data.level,
            category: data.Category || data.category,
            message: data.Message || data.message,
            exception: data.Exception || data.exception,
            traceId: data.TraceId || data.traceId,
            spanId: data.SpanId || data.spanId,
            requestPath: data.RequestPath || data.requestPath,
            userId: data.UserId || data.userId,
            machineName: data.MachineName || data.machineName,
            processId: data.ProcessId || data.processId,
          };
          
          // Regular log entry
          onMessage(transformedLog);
        }
      } catch (error) {
        console.error('Error parsing log stream data:', error);
      }
    };

    eventSource.addEventListener('heartbeat', (event) => {
      try {
        const data = JSON.parse(event.data);
        onHeartbeat?.(data);
      } catch (error) {
        console.error('Error parsing heartbeat data:', error);
      }
    });

    eventSource.addEventListener('error', (_event) => {
      // Note: EventSource error events don't contain parseable data
      console.error('Log stream error event received');
    });

    eventSource.onerror = (error) => {
      console.error('Log stream connection error:', error);
      onError?.(error);
    };

    eventSource.addEventListener('close', () => {
      console.log('Log stream closed');
      onClose?.();
    });

    return eventSource;
  }

  /**
   * Close a log stream connection
   */
  static closeLogStream(eventSource: EventSource): void {
    eventSource.close();
  }

  /**
   * Test API connectivity
   */
  static async testConnection(): Promise<boolean> {
    try {
      const response = await api.get('/health');
      return response.status === 200;
    } catch (error) {
      console.error('API connection test failed:', error);
      return false;
    }
  }
}

export default TelemetryService;