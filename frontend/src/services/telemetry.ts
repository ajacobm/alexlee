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

    const response = await api.get<LogsResponse>(`/api/telemetry/logs?${params.toString()}`);
    return response.data;
  }

  /**
   * Get log statistics and trends
   */
  static async getLogStatistics(): Promise<LogStatistics> {
    const response = await api.get<LogStatistics>('/api/telemetry/stats');
    return response.data;
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
          // Regular log entry
          onMessage(data as LogEntry);
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