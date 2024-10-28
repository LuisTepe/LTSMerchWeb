// Function to toggle the cart modal
function toggleCartModal() {
    document.getElementById("cartModal").classList.toggle("show");
}

function addItemToCart(item) {
    const cartItemsContainer = document.getElementById("cartItemsContainer");

    // Create item container
    const cartItem = document.createElement("div");
    cartItem.classList.add("cart-item");

    // Set item image
    const img = document.createElement("img");
    img.src = item.imageUrl;
    img.alt = item.name;
    cartItem.appendChild(img);

    // Set item details
    const details = document.createElement("div");
    details.classList.add("cart-item-details");

    const title = document.createElement("p");
    title.classList.add("cart-item-title");
    title.innerText = item.name;
    details.appendChild(title);

    const price = document.createElement("p");
    price.classList.add("cart-item-price");
    price.innerText = `$${item.price.toFixed(2)}`;
    details.appendChild(price);

    cartItem.appendChild(details);

    // Add item to container
    cartItemsContainer.appendChild(cartItem);
}
