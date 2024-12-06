

function renderFoodList(card, dotNetObjectReference) {
    const foodItems = document.querySelectorAll(card);

    if (foodItems.length === 0) {
        const observer = new MutationObserver((mutationsList, observer) => {
            mutationsList.forEach(mutation => {
                mutation.addedNodes.forEach(node => {
                    if (node.matches && node.matches(card)) {
                        addSwipeEvents([node], dotNetObjectReference);
                    }
                });
            });
        });

        observer.observe(document.getElementById("foodList"), { childList: true, subtree: true });
    } else {
        addSwipeEvents(foodItems, dotNetObjectReference);
    }
}


let allCompleted = false;

function checkCompleteProd(isComplete, dotNetObjectReference) {
    allCompleted = isComplete;

    if (allCompleted) {
        const foodItems = document.querySelectorAll(".card");
        addSwipeEvents(foodItems, dotNetObjectReference);
    } else {
        console.log("Cannot drag yet, not all items completed");
    }
}

function addSwipeEvents(foodItems, dotNetObjectReference) {
    foodItems.forEach(foodItem => {
        let startX = 0;
        let currentX = 0;
        let isDragging = false;

        if (allCompleted) {
            foodItem.addEventListener("pointerdown", e => {
                console.log('Allow dragging for:', foodItem);
                startDrag(e.clientX);
            });
        } else {
            console.log('Cannot drag yet, not all items completed');
        }

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

            const maxDistance = 200;
            currentX = Math.min(Math.max(currentX, -maxDistance), maxDistance);

            foodItem.style.transform = `translateX(${currentX}px)`;
        }

        function endDrag() {
            if (!isDragging) return;
            foodItem.classList.remove("removing");
            isDragging = false;

            if (Math.abs(currentX) > 100) {
                foodItem.style.transition = "transform 0.3s ease";
                foodItem.style.transform = `translateX(${currentX > 0 ? 100 : -100}%)`;

                // Gọi hàm xóa món
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
    });
}

async function removeFoodCard(tableNumber, dotNetObjectReference) {
    const card = document.querySelector(`.card[data-id="${tableNumber}"]`);
    if (card && !card.classList.contains("removing")) {
        card.classList.add("removing");
        card.style.transition = "opacity 0.5s ease";
        card.style.opacity = "0";
        setTimeout(() => card.remove(), 500);

        // Gọi hàm C# để xóa dữ liệu
        await dotNetObjectReference.invokeMethodAsync("RemoveNoteProdReq", tableNumber);
    }
}
