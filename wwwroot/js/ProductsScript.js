function addToCart(productId, event) {
        event.preventDefault();
    event.stopPropagation();

    fetch('/Cart/Add', {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json',
            },
    body: JSON.stringify({productId: productId, quantity: 1 })
        })
        .then(response => response.json())
        .then(data => {
            const button = event.target.closest('button');
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="bi bi-check-circle"></i> Added!';
    button.style.backgroundColor = '#28a745';

            setTimeout(() => {
        button.innerHTML = originalText;
    button.style.backgroundColor = '';
            }, 2000);
        })
        .catch(error => {
        console.error('Error:', error);
    alert('Added to cart!');
        });
    }
