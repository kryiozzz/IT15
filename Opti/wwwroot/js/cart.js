// Cart Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    // Initialize cart count when page loads
    fetch('/CustomerDashboard/GetCartCount')
        .then(response => response.json())
        .then(data => {
            updateCartCount(data.count);
        })
        .catch(error => console.error('Error loading cart count:', error));

    // Setup select all checkbox functionality
    const selectAllCheckbox = document.createElement('input');
    selectAllCheckbox.type = 'checkbox';
    selectAllCheckbox.className = 'item-checkbox';
    const headerCell = document.querySelector('thead th:first-child');
    if (headerCell) {
        headerCell.appendChild(selectAllCheckbox);

        selectAllCheckbox.addEventListener('change', function () {
            const checkboxes = document.querySelectorAll('.item-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
        });
    }

    // Handle checkout form submission
    const checkoutForm = document.querySelector('form[asp-controller="Payment"]');
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', function (e) {
            e.preventDefault(); // Prevent default form submission

            // Get all selected items
            const selectedItems = [];
            document.querySelectorAll('.item-checkbox:checked').forEach(checkbox => {
                const row = checkbox.closest('tr');
                const productId = row.dataset.productId;
                selectedItems.push(productId);
            });

            if (selectedItems.length === 0) {
                showToast('Please select at least one item to checkout', 'error');
                return;
            }

            // Set the hidden input value with selected product IDs
            const hiddenInput = document.getElementById('selected-products-input');
            if (hiddenInput) {
                hiddenInput.value = selectedItems.join(',');
            }

            // Show processing state
            const submitBtn = checkoutForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
            }

            // Submit the form
            this.submit();
        });
    }

    // Add pulse animation for cart count
    const style = document.createElement('style');
    style.textContent = `
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.2); }
            100% { transform: scale(1); }
        }
        .pulse {
            animation: pulse 0.5s ease-in-out;
        }
    `;
    document.head.appendChild(style);
});

// Function to update cart count display
function updateCartCount(count) {
    const cartCountElement = document.querySelector('.cart-count');
    if (cartCountElement) {
        cartCountElement.textContent = count;

        // Add animation
        cartCountElement.classList.add('pulse');
        setTimeout(() => {
            cartCountElement.classList.remove('pulse');
        }, 1000);
    }
}

// Update item quantity
function updateQuantity(productId, change) {
    fetch('/CustomerDashboard/UpdateCartQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${productId}&change=${change}`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update cart count badge
                updateCartCount(data.itemCount);

                // If item was removed (quantity = 0), remove the row
                if (data.newQuantity <= 0) {
                    let row = document.querySelector(`tr[data-product-id="${productId}"]`);
                    if (row) {
                        row.remove();
                    }

                    // If no items left, show empty cart message
                    if (data.itemCount === 0) {
                        location.reload();
                    }
                } else {
                    // Update quantity display
                    let quantitySpan = document.querySelector(`tr[data-product-id="${productId}"] .quantity-value`);
                    if (quantitySpan) {
                        quantitySpan.textContent = data.newQuantity;
                    }

                    // Update item subtotal
                    let priceCell = document.querySelector(`tr[data-product-id="${productId}"] .item-price`);
                    if (priceCell) {
                        priceCell.textContent = `₱${data.itemSubtotal}`;
                    }
                }

                // Update summary
                const summaryItemValues = document.querySelectorAll('.summary-item-value');
                if (summaryItemValues.length > 0) {
                    summaryItemValues[0].textContent = `₱${data.cartTotal}`;
                }

                const summaryTotalValue = document.querySelector('.summary-total-value');
                if (summaryTotalValue) {
                    summaryTotalValue.textContent = `₱${data.cartTotal}`;
                }

                // Show success toast
                showToast('Cart updated successfully', 'success');
            } else {
                // Check if login is required
                if (data.requireLogin) {
                    showLoginPrompt(data.message);
                } else {
                    showToast(data.message, 'error');
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Error updating quantity. Please try again later.', 'error');
        });
}

// Remove item from cart
function removeFromCart(productId) {
    if (confirm('Are you sure you want to remove this item from your cart?')) {
        fetch('/CustomerDashboard/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `productId=${productId}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update cart count badge
                    updateCartCount(data.itemCount);

                    let row = document.querySelector(`tr[data-product-id="${productId}"]`);
                    if (row) {
                        // Add fade out animation
                        row.style.transition = 'opacity 0.3s ease';
                        row.style.opacity = '0';

                        // Remove after animation completes
                        setTimeout(() => {
                            row.remove();
                        }, 300);
                    }

                    // Update total price
                    const summaryItemValues = document.querySelectorAll('.summary-item-value');
                    if (summaryItemValues.length > 0) {
                        summaryItemValues[0].textContent = `₱${data.cartTotal}`;
                    }

                    const summaryTotalValue = document.querySelector('.summary-total-value');
                    if (summaryTotalValue) {
                        summaryTotalValue.textContent = `₱${data.cartTotal}`;
                    }

                    // If no items left, show empty cart message
                    if (data.itemCount === 0) {
                        setTimeout(() => {
                            location.reload();
                        }, 500);
                    }

                    // Show success toast
                    showToast('Item removed from cart', 'success');
                } else {
                    // Check if login is required
                    if (data.requireLogin) {
                        showLoginPrompt(data.message);
                    } else {
                        showToast(data.message, 'error');
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('Error removing item from cart', 'error');
            });
    }
}

// Show login modal or redirect to login page
function showLoginPrompt(message) {
    if (confirm(message + " Would you like to login now?")) {
        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
    }
}

// Toast notification system
function showToast(message, type = 'success') {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.style.position = 'fixed';
        toastContainer.style.bottom = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '1050';
        document.body.appendChild(toastContainer);
    }

    // Create toast
    const toast = document.createElement('div');
    toast.className = `toast ${type === 'success' ? 'bg-success' : 'bg-danger'} text-white`;
    toast.style.minWidth = '250px';
    toast.style.margin = '10px';
    toast.style.padding = '15px';
    toast.style.borderRadius = '5px';
    toast.style.boxShadow = '0 4px 8px rgba(0, 0, 0, 0.2)';
    toast.style.display = 'flex';
    toast.style.alignItems = 'center';
    toast.style.justifyContent = 'space-between';

    // Add toast content
    toast.innerHTML = `
        <div>
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
            ${message}
        </div>
        <button type="button" class="btn-close btn-close-white" aria-label="Close"></button>
    `;

    // Add to container
    toastContainer.appendChild(toast);

    // Add close button functionality
    const closeButton = toast.querySelector('.btn-close');
    closeButton.addEventListener('click', function () {
        toast.remove();
    });

    // Auto-remove after 3 seconds
    setTimeout(() => {
        if (toast.parentNode) {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.5s ease';

            setTimeout(() => {
                if (toast.parentNode) {
                    toast.remove();
                }
            }, 500);
        }
    }, 3000);
}