function renderFoodList() {
    let startX = 0;
    let currentX = 0;

    foodItem.addEventListener("mousedown", e => {
        startX = e.clientX;
        foodItem.classList.add("removing");
        document.addEventListener("mousemove", onMouseMove);
        document.addEventListener("mouseup", onMouseUp);
    });

    function onMouseMove(e) {
        currentX = e.clientX - startX;
        foodItem.style.transform = `translateX(${currentX}px)`;
    }

    function onMouseUp() {
        document.removeEventListener("mousemove", onMouseMove);
        document.removeEventListener("mouseup", onMouseUp);
        foodItem.classList.remove("removing");

        if (Math.abs(currentX) > 100) {
            foodItem.style.transform = `translateX(${currentX > 0 ? 100 : -100}%)`;
            removeFoodItem(food.id);
        } else {
            foodItem.style.transform = "translateX(0)";
        }
    }
}

function removeFoodItem(id) {
    setTimeout(() => {
        const index = foods.findIndex(food => food.id === id);
        if (index !== -1) foods.splice(index, 1);
        renderFoodList();
    }, 300);
}