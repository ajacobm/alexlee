// Test script to validate the transformation logic for log entries
// This simulates what happens in the TelemetryService when processing backend responses

// Sample backend response (Pascal case)
const backendLogEntry = {
  Id: 1,
  Timestamp: "2025-07-23T05:17:27.0589219",
  Level: "Warning", 
  Category: "Test.Category",
  Message: "Test message",
  Exception: null,
  TraceId: "fe863fd124359ed00bdfbaf308aa5540",
  SpanId: "c3a2d2464e2bbf3f",
  RequestPath: null,
  UserId: null
};

// Transformation logic from TelemetryService
const transformedLog = {
  id: backendLogEntry.Id || backendLogEntry.id,
  timestamp: backendLogEntry.Timestamp || backendLogEntry.timestamp,
  level: backendLogEntry.Level || backendLogEntry.level,
  category: backendLogEntry.Category || backendLogEntry.category,
  message: backendLogEntry.Message || backendLogEntry.message,
  exception: backendLogEntry.Exception || backendLogEntry.exception,
  traceId: backendLogEntry.TraceId || backendLogEntry.traceId,
  spanId: backendLogEntry.SpanId || backendLogEntry.spanId,
  requestPath: backendLogEntry.RequestPath || backendLogEntry.requestPath,
  userId: backendLogEntry.UserId || backendLogEntry.userId,
  machineName: backendLogEntry.MachineName || backendLogEntry.machineName,
  processId: backendLogEntry.ProcessId || backendLogEntry.processId,
};

console.log("Original backend entry:", backendLogEntry);
console.log("Transformed entry:", transformedLog);

// Test the filtering logic that was causing the error
console.log("\n--- Testing filter functions ---");

// Test with proper log entry
console.log("level property exists:", transformedLog.level);
console.log("toLowerCase() works:", transformedLog.level.toLowerCase());

// Test with missing level property (this was causing the error)
const malformedLog = { ...transformedLog };
delete malformedLog.level;

console.log("\n--- Testing malformed log (no level property) ---");
console.log("level property:", malformedLog.level);
console.log("Safe toLowerCase():", (malformedLog.level || 'unknown').toLowerCase());

// Test the filtering logic
const logs = [transformedLog, malformedLog];

console.log("\n--- Testing filter logic ---");

// Filter for Warning level (including Error)
const warningFilter = logs.filter(log => log.level === 'Warning' || log.level === 'Error');
console.log("Warning filter result:", warningFilter.length, "entries");

// Filter for All (should include both)
const allFilter = logs.filter(log => true); // No level filtering for 'All'
console.log("All filter result:", allFilter.length, "entries");

console.log("\n--- Test completed successfully! ---");