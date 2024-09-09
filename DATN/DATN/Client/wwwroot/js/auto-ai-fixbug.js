function openChatGPT(query) {
    try {
        window.open(`http://chatgpt.com/?q=${encodeURIComponent(query)}`);
    } catch (error) {
        const query = '[JavaScript] fix error :${error.message}'
        window.open(`http://chatgpt.com/?q=${encodeURIComponent(query)}`);
    }
}