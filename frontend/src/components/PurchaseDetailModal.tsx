import React, { useState, useEffect } from 'react';
import { purchaseDetailsApi } from '../services/api';
import type { PurchaseDetailItem, CreatePurchaseDetailRequest, UpdatePurchaseDetailRequest } from '../types/api';
import './PurchaseDetailModal.css';

interface PurchaseDetailModalProps {
  item: PurchaseDetailItem | null;
  onSave: () => void;
  onCancel: () => void;
}

const PurchaseDetailModal: React.FC<PurchaseDetailModalProps> = ({ item, onSave, onCancel }) => {
  const [formData, setFormData] = useState({
    purchaseOrderNumber: '',
    itemNumber: 0,
    itemName: '',
    itemDescription: '',
    purchasePrice: 0,
    purchaseQuantity: 1,
    lastModifiedByUser: 'Current User',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!item;

  useEffect(() => {
    if (item) {
      setFormData({
        purchaseOrderNumber: item.purchaseOrderNumber,
        itemNumber: item.itemNumber,
        itemName: item.itemName,
        itemDescription: item.itemDescription || '',
        purchasePrice: item.purchasePrice,
        purchaseQuantity: item.purchaseQuantity,
        lastModifiedByUser: item.lastModifiedByUser,
      });
    }
  }, [item]);

  const handleChange = (field: string, value: string | number) => {
    setFormData(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      if (isEditing && item) {
        const updateRequest: UpdatePurchaseDetailRequest = {
          purchaseDetailItemAutoId: item.purchaseDetailItemAutoId,
          ...formData,
        };
        await purchaseDetailsApi.update(item.purchaseDetailItemAutoId, updateRequest);
      } else {
        const createRequest: CreatePurchaseDetailRequest = formData;
        await purchaseDetailsApi.create(createRequest);
      }
      
      onSave();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save purchase detail');
    } finally {
      setLoading(false);
    }
  };

  const handleOverlayClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onCancel();
    }
  };

  return (
    <div className="modal-overlay" onClick={handleOverlayClick}>
      <div className="modal">
        <div className="modal-header">
          <h3 className="modal-title">
            {isEditing ? 'Edit Purchase Detail' : 'Add New Purchase Detail'}
          </h3>
          <button className="modal-close" onClick={onCancel}>
            ×
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="modal-body">
          {error && (
            <div className="error-message">
              {error}
            </div>
          )}
          
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Purchase Order Number *</label>
              <input
                type="text"
                className="form-control"
                value={formData.purchaseOrderNumber}
                onChange={(e) => handleChange('purchaseOrderNumber', e.target.value)}
                required
                placeholder="e.g., PO-2024-001"
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Item Number *</label>
              <input
                type="number"
                className="form-control"
                value={formData.itemNumber}
                onChange={(e) => handleChange('itemNumber', parseInt(e.target.value) || 0)}
                required
                min="1"
                placeholder="e.g., 100"
              />
            </div>
          </div>
          
          <div className="form-group">
            <label className="form-label">Item Name *</label>
            <input
              type="text"
              className="form-control"
              value={formData.itemName}
              onChange={(e) => handleChange('itemName', e.target.value)}
              required
              placeholder="e.g., Wireless Keyboard"
            />
          </div>
          
          <div className="form-group">
            <label className="form-label">Item Description</label>
            <textarea
              className="form-control"
              value={formData.itemDescription}
              onChange={(e) => handleChange('itemDescription', e.target.value)}
              placeholder="Optional description of the item"
              rows={3}
            />
          </div>
          
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Purchase Price *</label>
              <input
                type="number"
                step="0.01"
                min="0"
                className="form-control"
                value={formData.purchasePrice}
                onChange={(e) => handleChange('purchasePrice', parseFloat(e.target.value) || 0)}
                required
                placeholder="e.g., 29.99"
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Purchase Quantity *</label>
              <input
                type="number"
                min="1"
                className="form-control"
                value={formData.purchaseQuantity}
                onChange={(e) => handleChange('purchaseQuantity', parseInt(e.target.value) || 1)}
                required
                placeholder="e.g., 5"
              />
            </div>
          </div>
          
          <div className="form-group">
            <label className="form-label">Last Modified By</label>
            <input
              type="text"
              className="form-control"
              value={formData.lastModifiedByUser}
              onChange={(e) => handleChange('lastModifiedByUser', e.target.value)}
              placeholder="User name"
            />
          </div>
        </form>
        
        <div className="modal-footer">
          <button type="button" className="btn btn-secondary" onClick={onCancel}>
            Cancel
          </button>
          <button 
            type="submit" 
            className="btn btn-primary" 
            onClick={handleSubmit}
            disabled={loading}
          >
            {loading ? 'Saving...' : (isEditing ? 'Update' : 'Create')}
          </button>
        </div>
      </div>
    </div>
  );
};

export default PurchaseDetailModal;
