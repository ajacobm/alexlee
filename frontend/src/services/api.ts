import axios from 'axios';
import type {
  PurchaseDetailItem,
  CreatePurchaseDetailRequest,
  UpdatePurchaseDetailRequest,
  PurchaseDetailFilter,
  DuplicatePurchaseDetailGroup,
  StringInterleaveRequest,
  StringInterleaveResponse,
  PalindromeCheckRequest,
  PalindromeCheckResponse,
  FileSearchRequest,
  FileSearchResponse,
  AlgorithmInfoResponse
} from '../types/api';

// Configure axios base URL
const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Purchase Detail API
export const purchaseDetailsApi = {
  // Get all purchase details with optional filtering
  getAll: async (filter?: PurchaseDetailFilter): Promise<PurchaseDetailItem[]> => {
    const params = new URLSearchParams();
    if (filter?.purchaseOrderNumber) params.append('purchaseOrderNumber', filter.purchaseOrderNumber);
    if (filter?.itemNumber) params.append('itemNumber', filter.itemNumber.toString());
    if (filter?.itemName) params.append('itemName', filter.itemName);
    if (filter?.itemDescription) params.append('itemDescription', filter.itemDescription);
    
    const response = await api.get<PurchaseDetailItem[]>(`/purchasedetails?${params.toString()}`);
    return response.data;
  },

  // Get purchase detail by ID
  getById: async (id: number): Promise<PurchaseDetailItem> => {
    const response = await api.get<PurchaseDetailItem>(`/purchasedetails/${id}`);
    return response.data;
  },

  // Create new purchase detail
  create: async (data: CreatePurchaseDetailRequest): Promise<PurchaseDetailItem> => {
    const response = await api.post<PurchaseDetailItem>('/purchasedetails', data);
    return response.data;
  },

  // Update purchase detail
  update: async (id: number, data: UpdatePurchaseDetailRequest): Promise<PurchaseDetailItem> => {
    const response = await api.put<PurchaseDetailItem>(`/purchasedetails/${id}`, data);
    return response.data;
  },

  // Delete purchase detail
  delete: async (id: number): Promise<void> => {
    await api.delete(`/purchasedetails/${id}`);
  },

  // Get duplicate purchase details
  getDuplicates: async (): Promise<DuplicatePurchaseDetailGroup[]> => {
    const response = await api.get<DuplicatePurchaseDetailGroup[]>('/purchasedetails/duplicates');
    return response.data;
  },
};

// Algorithm API
export const algorithmsApi = {
  // Get algorithm information
  getInfo: async (): Promise<AlgorithmInfoResponse> => {
    const response = await api.get<AlgorithmInfoResponse>('/algorithms/info');
    return response.data;
  },

  // String interleaving
  interleaveStrings: async (data: StringInterleaveRequest): Promise<StringInterleaveResponse> => {
    const response = await api.post<StringInterleaveResponse>('/algorithms/string-interleave', data);
    return response.data;
  },

  // Palindrome check
  checkPalindrome: async (data: PalindromeCheckRequest): Promise<PalindromeCheckResponse> => {
    const response = await api.post<PalindromeCheckResponse>('/algorithms/palindrome-check', data);
    return response.data;
  },

  // File search
  searchFiles: async (data: FileSearchRequest): Promise<FileSearchResponse> => {
    const response = await api.post<FileSearchResponse>('/algorithms/file-search', data);
    return response.data;
  },
};

export default api;
