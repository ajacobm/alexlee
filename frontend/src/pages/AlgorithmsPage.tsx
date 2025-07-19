import React, { useState } from 'react';
import { algorithmsApi } from '../services/api';
import type { 
  StringInterleaveRequest, 
  StringInterleaveResponse, 
  PalindromeCheckRequest, 
  PalindromeCheckResponse,
  FileSearchRequest,
  FileSearchResponse
} from '../types/api';

const AlgorithmsPage: React.FC = () => {
  // String Interleave State
  const [interleaveForm, setInterleaveForm] = useState<StringInterleaveRequest>({ first: 'abc', second: '123' });
  const [interleaveResult, setInterleaveResult] = useState<StringInterleaveResponse | null>(null);
  const [interleaveLoading, setInterleaveLoading] = useState(false);

  // Palindrome State
  const [palindromeForm, setPalindromeForm] = useState<PalindromeCheckRequest>({ input: 'A man a plan a canal Panama' });
  const [palindromeResult, setPalindromeResult] = useState<PalindromeCheckResponse | null>(null);
  const [palindromeLoading, setPalindromeLoading] = useState(false);

  // File Search State
  const [fileSearchForm, setFileSearchForm] = useState<FileSearchRequest>({ searchTerm: 'TODO', directoryPath: '/src' });
  const [fileSearchResult, setFileSearchResult] = useState<FileSearchResponse | null>(null);
  const [fileSearchLoading, setFileSearchLoading] = useState(false);

  const handleStringInterleave = async () => {
    setInterleaveLoading(true);
    try {
      const result = await algorithmsApi.interleaveStrings(interleaveForm);
      setInterleaveResult(result);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setInterleaveLoading(false);
    }
  };

  const handlePalindromeCheck = async () => {
    setPalindromeLoading(true);
    try {
      const result = await algorithmsApi.checkPalindrome(palindromeForm);
      setPalindromeResult(result);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setPalindromeLoading(false);
    }
  };

  const handleFileSearch = async () => {
    setFileSearchLoading(true);
    try {
      const result = await algorithmsApi.searchFiles(fileSearchForm);
      setFileSearchResult(result);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setFileSearchLoading(false);
    }
  };

  return (
    <div className="fade-in">
      <div className="mb-4">
        <h2>Algorithm Demonstrations</h2>
        <p className="text-secondary">
          Test the three C# algorithm implementations from the Alex Lee Developer Exercise.
        </p>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
        {/* Problem 1: String Interleaving */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Problem 1: String Interleaving</h3>
          </div>
          <div className="card-body">
            <p className="text-secondary mb-3">
              Interleave two strings character by character. Example: "abc" + "123" = "a1b2c3"
            </p>
            
            <div className="form-row mb-3">
              <div className="form-group">
                <label className="form-label">First String</label>
                <input
                  type="text"
                  className="form-control"
                  value={interleaveForm.first || ''}
                  onChange={(e) => setInterleaveForm(prev => ({ ...prev, first: e.target.value }))}
                  placeholder="Enter first string"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Second String</label>
                <input
                  type="text"
                  className="form-control"
                  value={interleaveForm.second || ''}
                  onChange={(e) => setInterleaveForm(prev => ({ ...prev, second: e.target.value }))}
                  placeholder="Enter second string"
                />
              </div>
            </div>

            <button 
              className="btn btn-primary mb-3" 
              onClick={handleStringInterleave}
              disabled={interleaveLoading}
            >
              {interleaveLoading ? 'Processing...' : 'Interleave Strings'}
            </button>

            {interleaveResult && (
              <div className="card" style={{ background: 'var(--alex-light)', border: '1px solid var(--alex-border)' }}>
                <div className="card-body" style={{ padding: '1rem' }}>
                  <h4 style={{ fontSize: '1rem', marginBottom: '0.5rem' }}>Result:</h4>
                  <p style={{ fontFamily: 'monospace', fontSize: '1.25rem', color: 'var(--alex-primary)', fontWeight: 'bold', marginBottom: '0.5rem' }}>
                    "{interleaveResult.result}"
                  </p>
                  <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>
                    <span>First: "{interleaveResult.first}" | </span>
                    <span>Second: "{interleaveResult.second}" | </span>
                    <span>Length: {interleaveResult.length}</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Problem 2: Palindrome Check */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Problem 2: Palindrome Check</h3>
          </div>
          <div className="card-body">
            <p className="text-secondary mb-3">
              Check if a string is a palindrome (ignoring case, spaces, and punctuation).
            </p>
            
            <div className="form-group mb-3">
              <label className="form-label">Input String</label>
              <input
                type="text"
                className="form-control"
                value={palindromeForm.input || ''}
                onChange={(e) => setPalindromeForm(prev => ({ ...prev, input: e.target.value }))}
                placeholder="Enter text to check"
              />
            </div>

            <button 
              className="btn btn-primary mb-3" 
              onClick={handlePalindromeCheck}
              disabled={palindromeLoading}
            >
              {palindromeLoading ? 'Processing...' : 'Check Palindrome'}
            </button>

            {palindromeResult && (
              <div className="card" style={{ background: 'var(--alex-light)', border: '1px solid var(--alex-border)' }}>
                <div className="card-body" style={{ padding: '1rem' }}>
                  <h4 style={{ fontSize: '1rem', marginBottom: '0.5rem' }}>Result:</h4>
                  <p style={{ 
                    fontSize: '1.25rem', 
                    fontWeight: 'bold', 
                    marginBottom: '0.5rem',
                    color: palindromeResult.isPalindrome ? 'var(--alex-success)' : 'var(--alex-danger)'
                  }}>
                    {palindromeResult.result}
                  </p>
                  <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>
                    <div>Original: "{palindromeResult.input}"</div>
                    <div>Processed: "{palindromeResult.processedInput}"</div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Problem 3: File Search */}
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Problem 3: Parallel File Search</h3>
          </div>
          <div className="card-body">
            <p className="text-secondary mb-3">
              Search for text across multiple files using parallel processing. (Demo version with simulated results)
            </p>
            
            <div className="form-row mb-3">
              <div className="form-group">
                <label className="form-label">Search Term</label>
                <input
                  type="text"
                  className="form-control"
                  value={fileSearchForm.searchTerm}
                  onChange={(e) => setFileSearchForm(prev => ({ ...prev, searchTerm: e.target.value }))}
                  placeholder="Enter search term"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Directory Path (Simulated)</label>
                <input
                  type="text"
                  className="form-control"
                  value={fileSearchForm.directoryPath || ''}
                  onChange={(e) => setFileSearchForm(prev => ({ ...prev, directoryPath: e.target.value }))}
                  placeholder="Enter directory path"
                />
              </div>
            </div>

            <button 
              className="btn btn-primary mb-3" 
              onClick={handleFileSearch}
              disabled={fileSearchLoading}
            >
              {fileSearchLoading ? 'Searching...' : 'Search Files'}
            </button>

            {fileSearchResult && (
              <div className="card" style={{ background: 'var(--alex-light)', border: '1px solid var(--alex-border)' }}>
                <div className="card-body" style={{ padding: '1rem' }}>
                  <h4 style={{ fontSize: '1rem', marginBottom: '1rem' }}>Search Results:</h4>
                  
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
                    <div style={{ textAlign: 'center', padding: '1rem', background: 'white', borderRadius: '0.5rem' }}>
                      <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: 'var(--alex-primary)' }}>
                        {fileSearchResult.fileCount}
                      </div>
                      <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>Files Searched</div>
                    </div>
                    <div style={{ textAlign: 'center', padding: '1rem', background: 'white', borderRadius: '0.5rem' }}>
                      <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: 'var(--alex-success)' }}>
                        {fileSearchResult.occurrenceCount}
                      </div>
                      <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>Occurrences Found</div>
                    </div>
                    <div style={{ textAlign: 'center', padding: '1rem', background: 'white', borderRadius: '0.5rem' }}>
                      <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: 'var(--alex-warning)' }}>
                        {fileSearchResult.processingTimeMs}ms
                      </div>
                      <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>Processing Time</div>
                    </div>
                  </div>

                  <div style={{ fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>
                    <div>Search Term: "{fileSearchResult.searchTerm}"</div>
                    <div>Status: {fileSearchResult.searchCompleted ? 'Completed' : 'In Progress'}</div>
                    <div>Sample Files: {(fileSearchResult.filesSearched || []).slice(0, 3).join(', ')}{(fileSearchResult.filesSearched || []).length > 3 ? '...' : ''}</div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AlgorithmsPage;
