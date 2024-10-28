const foods = [
    { id: 1, name: "Pizza", description: "Cheese & tomato topping" },
    { id: 2, name: "Burger", description: "Beef patty with lettuce" },
    { id: 3, name: "Sushi", description: "Rice & fresh fish" },
    { id: 4, name: "Pasta", description: "Pasta with tomato sauce" },
    { id: 5, name: "Salad", description: "Mixed greens with dressing" },
];

const foodList = document.getElementById("foodList");

function renderFoodList() {
    foodList.innerHTML = "";
    foods.forEach(food => {
        const foodItem = document.createElement("div");
        foodItem.classList.add("food-item", "shadow-sm", "p-3", "rounded");
        foodItem.dataset.id = food.id;

        foodItem.innerHTML = `
                    <div class="food-name">${food.name}</div>
                    <div class="food-description">${food.description}</div>
                `;

        foodList.appendChild(foodItem);

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
    });
}

function removeFoodItem(id) {
    setTimeout(() => {
        const index = foods.findIndex(food => food.id === id);
        if (index !== -1) foods.splice(index, 1);
        renderFoodList();
    }, 300);
}

renderFoodList();