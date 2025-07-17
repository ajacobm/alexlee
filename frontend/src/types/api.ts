// API Response Types
export interface PurchaseDetailItem {
  purchaseDetailItemAutoId: number;
  purchaseOrderNumber: string;
  itemNumber: number;
  itemName: string;
  itemDescription?: string;
  purchasePrice: number;
  purchaseQuantity: number;
  lastModifiedByUser: string;
  lastModifiedDateTime: string;
  lineNumber?: number;
}

export interface CreatePurchaseDetailRequest {
  purchaseOrderNumber: string;
  itemNumber: number;
  itemName: string;
  itemDescription?: string;
  purchasePrice: number;
  purchaseQuantity: number;
  lastModifiedByUser: string;
}

export interface UpdatePurchaseDetailRequest {
  purchaseDetailItemAutoId: number;
  purchaseOrderNumber: string;
  itemNumber: number;
  itemName: string;
  itemDescription?: string;
  purchasePrice: number;
  purchaseQuantity: number;
  lastModifiedByUser: string;
}

export interface PurchaseDetailFilter {
  purchaseOrderNumber?: string;
  itemNumber?: number;
  itemName?: string;
  itemDescription?: string;
}

export interface DuplicatePurchaseDetailGroup {
  purchaseOrderNumber: string;
  itemNumber: number;
  purchasePrice: number;
  purchaseQuantity: number;
  count: number;
  purchaseDetailItemAutoIds: number[];
}

// Algorithm Types
export interface StringInterleaveRequest {
  first?: string;
  second?: string;
}

export interface StringInterleaveResponse {
  first: string;
  second: string;
  result: string;
  length: number;
}

export interface PalindromeCheckRequest {
  input?: string;
}

export interface PalindromeCheckResponse {
  input: string;
  result: string;
  isPalindrome: boolean;
  processedInput: string;
}

export interface FileSearchRequest {
  searchTerm: string;
  directoryPath?: string;
}

export interface FileSearchResponse {
  searchTerm: string;
  fileCount: number;
  lineCount: number;
  occurrenceCount: number;
  searchCompleted: boolean;
  processingTimeMs: number;
  filesSearched: string[];
}

export interface AlgorithmInfo {
  name: string;
  description: string;
  example: string;
  endpoint: string;
}

export interface AlgorithmInfoResponse {
  algorithms: AlgorithmInfo[];
  totalAlgorithms: number;
  author: string;
}
