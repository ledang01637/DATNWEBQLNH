function renderFoodList(card, dotNetObjectReference) {
    const foodItems = document.querySelectorAll(card);

    if (foodItems.length === 0) {
        const observer = new MutationObserver((mutationsList, observer) => {
            const newFoodItems = document.querySelectorAll(card);
            if (newFoodItems.length > 0) {
                observer.disconnect();
                addSwipeEvents(newFoodItems, dotNetObjectReference);
            }
        });

        observer.observe(document.getElementById("foodList"), { childList: true, subtree: true });
    } else {
        addSwipeEvents(foodItems, dotNetObjectReference);
    }
}

function addSwipeEvents(foodItems, dotNetObjectReference) {
    foodItems.forEach(foodItem => {
        let startX = 0;
        let currentX = 0;
        let isDragging = false;

        function startDrag(clientX) {
            startX = clientX;
            foodItem.classList.add("removing");
            isDragging = true;

            window.addEventListener("pointermove", onDrag);
            window.addEventListener("pointerup", endDrag);
        }

        function onDrag(event) {
            if (!isDragging) return;
            currentX = event.clientX - startX;
            foodItem.style.transform = `translateX(${currentX}px)`;
        }

        function endDrag() {
            if (!isDragging) return;
            foodItem.classList.remove("removing");
            isDragging = false;

            if (Math.abs(currentX) > 100) {
                foodItem.style.transition = "transform 0.3s ease";
                foodItem.style.transform = `translateX(${currentX > 0 ? 100 : -100}%)`;
                const tableNumber = foodItem.getAttribute("data-id");
                setTimeout(() => removeFoodCard(tableNumber, dotNetObjectReference), 300);
            } else {
                foodItem.style.transform = "translateX(0)";
            }

            startX = 0;
            currentX = 0;
            window.removeEventListener("pointermove", onDrag);
            window.removeEventListener("pointerup", endDrag);
        }

        foodItem.addEventListener("pointerdown", e => startDrag(e.clientX));
    });
}

async function removeFoodCard(tableNumber, dotNetObjectReference) {
    const card = document.querySelector(`.card[data-id="${tableNumber}"]`);
    if (card) {
        card.style.transition = "opacity 0.5s ease";
        card.style.opacity = "0";
        setTimeout(() => card.remove(), 500);
    }
    await dotNetObjectReference.invokeMethodAsync("RemoveNoteProdReq", tableNumber);
}
