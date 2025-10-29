
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