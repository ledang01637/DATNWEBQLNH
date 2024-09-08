document.addEventListener("DOMContentLoaded", function () {
    const bubblesContainer = document.getElementById('bubbles');
    const numberOfBubbles = 300;

    function createRandomBubble() {
        const span = document.createElement('span');
        const size = Math.random() * 5 + 2 + 'px';
        const left = Math.random() * 100 + '%';
        const delay = Math.random() * 2 + 's';
        const duration = Math.random() * 3 + 2 + 's';

        span.style.width = size;
        span.style.height = size;
        span.style.left = left;
        span.style.animationDelay = delay;
        span.style.animationDuration = duration;
        span.style.backgroundColor = Math.random() > 0.5 ? '#fff' : 'white';

        bubblesContainer.appendChild(span);
    }

    for (let i = 0; i < numberOfBubbles; i++) {
        createRandomBubble();
    }
});