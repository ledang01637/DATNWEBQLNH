function renderFoodList(card) {
    const foodItems = document.querySelectorAll(card);

    if (foodItems.length === 0) {
        const observer = new MutationObserver((mutationsList, observer) => {
            const newFoodItems = document.querySelectorAll(card);
            if (newFoodItems.length > 0) {
                observer.disconnect();
                addSwipeEvents(newFoodItems);
            }
        });

        observer.observe(document.getElementById("foodList"), { childList: true, subtree: true });
    } else {
        addSwipeEvents(foodItems);
    }
    console.log(foodItems);
}

function addSwipeEvents(foodItems) {
    foodItems.forEach(foodItem => {
        let startX = 0;
        let currentX = 0;
        let isDragging = false;

        function startDrag(clientX) {
            startX = clientX;
            foodItem.classList.add("removing");
            isDragging = true;
        }

        function onDrag(clientX) {
            if (!isDragging) return;
            currentX = clientX - startX;
            foodItem.style.transform = `translateX(${currentX}px)`;
        }

        function endDrag() {
            if (!isDragging) return;
            foodItem.classList.remove("removing");
            isDragging = false;

            if (Math.abs(currentX) > 100) {
                foodItem.style.transform = `translateX(${currentX > 0 ? 100 : -100}%)`;
                const tableNumber = foodItem.getAttribute("data-id");
                removeFoodCard(tableNumber);
            } else {
                foodItem.style.transform = "translateX(0)";
            }

            startX = 0;
            currentX = 0;
        }

        // Sự kiện chuột
        foodItem.addEventListener("mousedown", e => startDrag(e.clientX));
        window.addEventListener("mousemove", e => onDrag(e.clientX));
        window.addEventListener("mouseup", () => endDrag());

        // Sự kiện cảm ứng
        foodItem.addEventListener("touchstart", e => startDrag(e.touches[0].clientX));
        window.addEventListener("touchmove", e => onDrag(e.touches[0].clientX));
        window.addEventListener("touchend", () => endDrag());
    });
}

async function removeFoodCard(tableNumber) {
    setTimeout(() => {
        const card = document.querySelector(`.card[data-id="${tableNumber}"]`);
        if (card) {
            card.remove();
        }
    }, 300);

    // Xóa `NoteProdReq` khỏi `localStorage`
    const noteProdReqs = await _localStorageService.GetAsync("noteProdReqs");
    const updatedNoteProdReqs = noteProdReqs.filter(req => req.TableNumber !== tableNumber);
    await _localStorageService.SetAsync("noteProdReqs", updatedNoteProdReqs);
}
