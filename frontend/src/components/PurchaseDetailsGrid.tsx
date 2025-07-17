import React, { useState, useEffect } from 'react';
import { purchaseDetailsApi } from '../services/api';
import type { PurchaseDetailItem, PurchaseDetailFilter } from '../types/api';
import PurchaseDetailModal from './PurchaseDetailModal';
import './PurchaseDetailsGrid.css';

const PurchaseDetailsGrid: React.FC = () => {
  const [items, setItems] = useState<PurchaseDetailItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editingItem, setEditingItem] = useState<PurchaseDetailItem | null>(null);
  
  // Filter state
  const [filters, setFilters] = useState<PurchaseDetailFilter>({
    purchaseOrderNumber: '',
    itemNumber: undefined,
    itemName: '',
    itemDescription: '',
  });

  const fetchItems = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Create filter object with only non-empty values
      const activeFilters: PurchaseDetailFilter = {};
      if (filters.purchaseOrderNumber?.trim()) {
        activeFilters.purchaseOrderNumber = filters.purchaseOrderNumber.trim();
      }
      if (filters.itemNumber) {
        activeFilters.itemNumber = filters.itemNumber;
      }
      if (filters.itemName?.trim()) {
        activeFilters.itemName = filters.itemName.trim();
      }
      if (filters.itemDescription?.trim()) {
        activeFilters.itemDescription = filters.itemDescription.trim();
      }

      const data = await purchaseDetailsApi.getAll(
        Object.keys(activeFilters).length > 0 ? activeFilters : undefined
      );
      setItems(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch purchase details');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems();
  }, []);

  const handleFilterChange = (field: keyof PurchaseDetailFilter, value: string | number | undefined) => {
    setFilters(prev => ({
      ...prev,
      [field]: value === '' ? undefined : value,
    }));
  };

  const handleSearch = () => {
    fetchItems();
  };

  const handleClearFilters = () => {
    setFilters({
      purchaseOrderNumber: '',
      itemNumber: undefined,
      itemName: '',
      itemDescription: '',
    });
    // Fetch all items after clearing filters
    setTimeout(() => fetchItems(), 0);
  };

  const handleAdd = () => {
    setEditingItem(null);
    setShowModal(true);
  };

  const handleEdit = (item: PurchaseDetailItem) => {
    setEditingItem(item);
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this purchase detail item?')) {
      return;
    }

    try {
      await purchaseDetailsApi.delete(id);
      await fetchItems(); // Refresh the list
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete item');
    }
  };

  const handleSave = async () => {
    setShowModal(false);
    await fetchItems(); // Refresh the list
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(dateString));
  };

  return (
    <div className="purchase-details-grid">
      {/* Header */}
      <div className="grid-header">
        <h2>Purchase Details Management</h2>
        <button className="btn btn-primary" onClick={handleAdd}>
          Add New Item
        </button>
      </div>

      {/* Filters */}
      <div className="card filter-card">
        <div className="card-header">
          <h3 className="card-title">Search Filters</h3>
        </div>
        <div className="card-body">
          <div className="filter-grid">
            <div className="form-group">
              <label className="form-label">Purchase Order #</label>
              <input
                type="text"
                className="form-control"
                value={filters.purchaseOrderNumber || ''}
                onChange={(e) => handleFilterChange('purchaseOrderNumber', e.target.value)}
                placeholder="Enter order number"
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Item #</label>
              <input
                type="number"
                className="form-control"
                value={filters.itemNumber || ''}
                onChange={(e) => handleFilterChange('itemNumber', e.target.value ? parseInt(e.target.value) : undefined)}
                placeholder="Enter item number"
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Item Name</label>
              <input
                type="text"
                className="form-control"
                value={filters.itemName || ''}
                onChange={(e) => handleFilterChange('itemName', e.target.value)}
                placeholder="Enter item name"
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Description</label>
              <input
                type="text"
                className="form-control"
                value={filters.itemDescription || ''}
                onChange={(e) => handleFilterChange('itemDescription', e.target.value)}
                placeholder="Enter description"
              />
            </div>
          </div>
          
          <div className="filter-actions">
            <button className="btn btn-primary" onClick={handleSearch}>
              Search
            </button>
            <button className="btn btn-secondary" onClick={handleClearFilters}>
              Clear Filters
            </button>
          </div>
        </div>
      </div>

      {/* Error Display */}
      {error && (
        <div className="error-message">
          Error: {error}
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="loading">
          Loading purchase details...
        </div>
      )}

      {/* Data Grid */}
      {!loading && !error && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">
              Purchase Details ({items.length} items)
            </h3>
          </div>
          <div className="card-body">
            {items.length === 0 ? (
              <div className="empty-state">
                No purchase details found. Try adjusting your filters or add a new item.
              </div>
            ) : (
              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Order #</th>
                      <th>Item #</th>
                      <th>Item Name</th>
                      <th>Description</th>
                      <th>Price</th>
                      <th>Quantity</th>
                      <th>Modified By</th>
                      <th>Modified Date</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((item) => (
                      <tr key={item.purchaseDetailItemAutoId}>
                        <td>{item.purchaseOrderNumber}</td>
                        <td>{item.itemNumber}</td>
                        <td>{item.itemName}</td>
                        <td>{item.itemDescription || '-'}</td>
                        <td>{formatCurrency(item.purchasePrice)}</td>
                        <td>{item.purchaseQuantity}</td>
                        <td>{item.lastModifiedByUser}</td>
                        <td>{formatDate(item.lastModifiedDateTime)}</td>
                        <td>
                          <div className="action-buttons">
                            <button
                              className="btn btn-sm btn-secondary"
                              onClick={() => handleEdit(item)}
                            >
                              Edit
                            </button>
                            <button
                              className="btn btn-sm btn-danger"
                              onClick={() => handleDelete(item.purchaseDetailItemAutoId)}
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <PurchaseDetailModal
          item={editingItem}
          onSave={handleSave}
          onCancel={() => setShowModal(false)}
        />
      )}
    </div>
  );
};

export default PurchaseDetailsGrid;
