
        function changeQuantity(change) {
            const quantityInput = document.getElementById('quantity');
            let currentValue = parseInt(quantityInput.value);
            let maxValue = parseInt(quantityInput.max);
            let newValue = currentValue + change;

            if (newValue >= 1 && newValue <= maxValue) {
                quantityInput.value = newValue;
            }
        }

        function setMainImage(thumbnail) {
            const mainImage = document.getElementById('mainImage');
            mainImage.src = thumbnail.src;

            document.querySelectorAll('.thumbnail').forEach(t => t.classList.remove('active'));
            thumbnail.classList.add('active');
        }

        function changeImage(direction) {
            const thumbnails = document.querySelectorAll('.thumbnail');
            const activeThumbnail = document.querySelector('.thumbnail.active');
            let currentIndex = Array.from(thumbnails).indexOf(activeThumbnail);
            let newIndex = currentIndex + direction;

            if (newIndex >= 0 && newIndex < thumbnails.length) {
                setMainImage(thumbnails[newIndex]);
            }
        }

        function toggleFavorite(btn) {
            const icon = btn.querySelector('i');
            if (icon.classList.contains('bi-heart')) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                btn.classList.add('active');
                showNotification('Added to favorites!', 'success');
            } else {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
                btn.classList.remove('active');
                showNotification('Removed from favorites', 'info');
            }
        }

        function keepAside(productId) {
            const quantity = document.getElementById('quantity').value;

            fetch('/CustomerDashboard/KeepAside', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ productId: productId, quantity: parseInt(quantity) })
            })
            .then(response => response.json())
            .then(data => {
                showNotification('Product kept aside successfully!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('Added to keep aside!', 'success');
            });
        }

async function loadCart() {
    try {
        if (window.Cart && typeof window.Cart.loadCart === 'function') {
            await window.Cart.loadCart();
            if (window.Cart.currentItemCount !== undefined) {
                updateCartBadge(window.Cart.currentItemCount);
            }
            return;
        }

        const res = await fetch('/Cart/Get');
        if (!res.ok) throw new Error('Failed to load cart');
        const cart = await res.json();
        const count = (cart && cart.items) ? cart.items.reduce((s, it) => s + (parseInt(it.quantity || it.Quantity || 0)), 0) : 0;
        updateCartBadge(count);
    } catch (err) {
        console.error('loadCart error', err);
        updateCartBadge(0);
    }
}

function updateCartBadge(count) {
    const badge = document.getElementById('cart-count');
    if (!badge) return;
    badge.textContent = count > 0 ? String(count) : '';
    if (count > 0) badge.classList.remove('d-none'); else badge.classList.add('d-none');
}

async function updateQuantity(productId, quantity) {
    try {
        const res = await fetch('/Cart/Update', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: quantity })
        });
        if (!res.ok) throw new Error('Failed to update quantity');
        const result = await res.json();
        if (result && result.success) {
            showNotification('Cart updated', 'success');
        } else {
            showNotification('Unable to update cart', 'warning');
        }
        await loadCart();
    } catch (err) {
        console.error('updateQuantity error', err);
        showNotification('Unable to update cart', 'warning');
    }
}

async function removeItem(productId) {
    try {
        const res = await fetch('/Cart/Remove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId })
        });
        if (!res.ok) throw new Error('Failed to remove item');
        const result = await res.json();
        if (result && result.success) {
            showNotification('Item removed from cart', 'success');
        } else {
            showNotification('Unable to remove item', 'warning');
        }
        await loadCart();
    } catch (err) {
        console.error('removeItem error', err);
        showNotification('Unable to remove item', 'warning');
    }
}


async function addToCart(productId, event) {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }

    const quantityEl = document.getElementById('quantity');
    const quantity = quantityEl ? parseInt(quantityEl.value) || 1 : 1;

    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: parseInt(quantity) })
        });

        if (res.ok) {
            const data = await res.json();
            const btn = event ? event.target.closest('button') : null;
            if (btn) {
                const original = btn.innerHTML;
                btn.innerHTML = '<i class="bi bi-check-circle"></i> Added!';
                btn.style.backgroundColor = '#28a745';
                setTimeout(() => {
                    btn.innerHTML = original;
                    btn.style.backgroundColor = '';
                }, 1500);
            }

            await loadCart();
            showNotification('Added to cart successfully!', 'success');
        } else {
            showNotification('Failed to add to cart', 'danger');
        }
    } catch (err) {
        console.error('addToCart error', err);
        showNotification('Added to cart!', 'success');
        await loadCart();
    }
}

async function quickAddToCart(productId, event) {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: 1 })
        });
        if (res.ok) {
            showNotification('Added to cart!', 'success');
            await loadCart();
        } else {
            showNotification('Failed to add to cart', 'warning');
        }
    } catch (err) {
        console.error('quickAddToCart error', err);
        showNotification('Added to cart!', 'success');
        await loadCart();
    }
}

        function showNotification(message, type) {
            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            notification.innerHTML = `
                <i class="bi bi-check-circle"></i>
                <span>${message}</span>
            `;

            document.body.appendChild(notification);

            setTimeout(() => {
                notification.classList.add('show');
            }, 100);

            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => notification.remove(), 300);
            }, 3000);
        }
    </script>
    <script>
        function changeQuantity(change) {
            const quantityInput = document.getElementById('quantity');
            let currentValue = parseInt(quantityInput.value);
            let maxValue = parseInt(quantityInput.max);
            let newValue = currentValue + change;

            if (newValue >= 1 && newValue <= maxValue) {
                quantityInput.value = newValue;
            }
        }

        function setMainImage(thumbnail) {
            const mainImage = document.getElementById('mainImage');
            mainImage.src = thumbnail.src;

            document.querySelectorAll('.thumbnail').forEach(t => t.classList.remove('active'));
            thumbnail.classList.add('active');
        }

        function changeImage(direction) {
            const thumbnails = document.querySelectorAll('.thumbnail');
            const activeThumbnail = document.querySelector('.thumbnail.active');
            let currentIndex = Array.from(thumbnails).indexOf(activeThumbnail);
            let newIndex = currentIndex + direction;

            if (newIndex >= 0 && newIndex < thumbnails.length) {
                setMainImage(thumbnails[newIndex]);
            }
        }

        function toggleFavorite(btn) {
            const icon = btn.querySelector('i');
            if (icon.classList.contains('bi-heart')) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                btn.classList.add('active');
                showNotification('Added to favorites!', 'success');
            } else {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
                btn.classList.remove('active');
                showNotification('Removed from favorites', 'info');
            }
        }

        function keepAside(productId) {
            const quantity = document.getElementById('quantity').value;

            fetch('/CustomerDashboard/KeepAside', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ productId: productId, quantity: parseInt(quantity) })
            })
            .then(response => response.json())
            .then(data => {
                showNotification('Product kept aside successfully!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('Added to keep aside!', 'success');
            });
        }

        function addToCart(productId) {
            const quantity = document.getElementById('quantity').value;

            fetch('/Cart/Add', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ productId: productId, quantity: parseInt(quantity) })
            })
            .then(response => response.json())
            .then(data => {
                showNotification('Added to cart successfully!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('Added to cart!', 'success');
            });
        }

        function quickAddToCart(productId) {
            event.preventDefault();
            event.stopPropagation();

            fetch('/Cart/Add', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ productId: productId, quantity: 1 })
            })
            .then(response => response.json())
            .then(data => {
                showNotification('Added to cart!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('Added to cart!', 'success');
            });
        }

        function showNotification(message, type) {
            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            notification.innerHTML = `
                <i class="bi bi-check-circle"></i>
                <span>${message}</span>
            `;

            document.body.appendChild(notification);

            setTimeout(() => {
                notification.classList.add('show');
            }, 100);

            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => notification.remove(), 300);
            }, 3000);
        }