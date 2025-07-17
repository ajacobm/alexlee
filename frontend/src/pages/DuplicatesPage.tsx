import React, { useState, useEffect } from 'react';
import { purchaseDetailsApi } from '../services/api';
import type { DuplicatePurchaseDetailGroup } from '../types/api';

const DuplicatesPage: React.FC = () => {
  const [duplicates, setDuplicates] = useState<DuplicatePurchaseDetailGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDuplicates = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await purchaseDetailsApi.getDuplicates();
      setDuplicates(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch duplicate records');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDuplicates();
  }, []);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  return (
    <div className="fade-in">
      <div className="mb-4">
        <h2>Duplicate Purchase Details</h2>
        <p className="text-secondary">
          SQL Problem #5: Identify duplicate records with the same purchase order, item number, price, and quantity.
        </p>
        <button className="btn btn-primary" onClick={fetchDuplicates} disabled={loading}>
          {loading ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      {error && (
        <div className="error-message mb-4">
          Error: {error}
        </div>
      )}

      {loading && (
        <div className="loading">
          Loading duplicate records...
        </div>
      )}

      {!loading && !error && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">
              Duplicate Groups ({duplicates.length} groups found)
            </h3>
          </div>
          <div className="card-body">
            {duplicates.length === 0 ? (
              <div className="empty-state">
                <h4>No Duplicate Records Found</h4>
                <p className="text-secondary">
                  All purchase detail records are unique. This indicates good data quality!
                </p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
                {duplicates.map((group, index) => (
                  <div key={index} className="card" style={{ border: '1px solid var(--alex-warning)', background: '#fef3c7' }}>
                    <div className="card-header" style={{ background: '#fbbf24', color: 'white' }}>
                      <h4 style={{ margin: 0, fontSize: '1rem' }}>
                        Duplicate Group #{index + 1} - {group.count} Records
                      </h4>
                    </div>
                    <div className="card-body">
                      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
                        <div>
                          <strong className="text-sm">Purchase Order:</strong>
                          <div style={{ fontFamily: 'monospace', color: 'var(--alex-primary)' }}>
                            {group.purchaseOrderNumber}
                          </div>
                        </div>
                        <div>
                          <strong className="text-sm">Item Number:</strong>
                          <div style={{ fontFamily: 'monospace', color: 'var(--alex-primary)' }}>
                            {group.itemNumber}
                          </div>
                        </div>
                        <div>
                          <strong className="text-sm">Purchase Price:</strong>
                          <div style={{ fontFamily: 'monospace', color: 'var(--alex-success)' }}>
                            {formatCurrency(group.purchasePrice)}
                          </div>
                        </div>
                        <div>
                          <strong className="text-sm">Quantity:</strong>
                          <div style={{ fontFamily: 'monospace', color: 'var(--alex-primary)' }}>
                            {group.purchaseQuantity}
                          </div>
                        </div>
                      </div>
                      
                      <div>
                        <strong className="text-sm">Affected Record IDs:</strong>
                        <div style={{ marginTop: '0.5rem' }}>
                          {group.purchaseDetailItemAutoIds.map((id) => (
                            <span
                              key={id}
                              style={{
                                display: 'inline-block',
                                background: 'var(--alex-primary)',
                                color: 'white',
                                padding: '0.25rem 0.5rem',
                                borderRadius: '0.25rem',
                                marginRight: '0.5rem',
                                marginBottom: '0.25rem',
                                fontSize: '0.75rem',
                                fontFamily: 'monospace'
                              }}
                            >
                              ID: {id}
                            </span>
                          ))}
                        </div>
                      </div>
                      
                      <div style={{ 
                        marginTop: '1rem', 
                        padding: '0.75rem',
                        background: 'rgba(251, 191, 36, 0.2)',
                        borderRadius: '0.375rem',
                        fontSize: '0.875rem',
                        color: 'var(--alex-warning)'
                      }}>
                        <strong>⚠️ Action Required:</strong> These {group.count} records have identical values and should be reviewed for potential consolidation.
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      {!loading && !error && duplicates.length > 0 && (
        <div className="card mt-4" style={{ background: '#f0f9ff', border: '1px solid var(--alex-primary)' }}>
          <div className="card-header" style={{ background: 'var(--alex-primary)', color: 'white' }}>
            <h3 className="card-title" style={{ margin: 0 }}>SQL Implementation Details</h3>
          </div>
          <div className="card-body">
            <h4 style={{ fontSize: '1rem', marginBottom: '1rem' }}>Query Logic (Problem #5):</h4>
            <pre style={{ 
              background: 'var(--alex-dark)', 
              color: '#e2e8f0', 
              padding: '1rem', 
              borderRadius: '0.375rem', 
              fontSize: '0.75rem',
              overflow: 'auto'
            }}>
{`SELECT PurchaseOrderNumber, ItemNumber, PurchasePrice, PurchaseQuantity, 
       COUNT(*) as Count,
       STRING_AGG(CAST(PurchaseDetailItemAutoId AS VARCHAR), ',') as IDs
FROM PurchaseDetailItem 
GROUP BY PurchaseOrderNumber, ItemNumber, PurchasePrice, PurchaseQuantity
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC`}
            </pre>
            <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: 'var(--alex-text-light)' }}>
              This query groups records by key fields and identifies groups with more than one record, 
              indicating duplicates that need attention.
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default DuplicatesPage;
