window.renderChart = (canvasRef, type, data, options) => {
    const ctx = canvasRef.getContext("2d");
    if (canvasRef.chart) {
        canvasRef.chart.destroy();
    }
    canvasRef.chart = new Chart(ctx, {
        type: type,
        data: data,
        options: options
    });
};