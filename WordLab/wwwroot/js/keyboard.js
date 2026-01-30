window.registerGlobalKeyboard = (dotnetHelper) => {
    document.addEventListener("keydown", (event) => {

        const target = event.target;

        // Ignore typing inside inputs, textareas, or contenteditable
        if (
            target instanceof HTMLInputElement ||
            target instanceof HTMLTextAreaElement ||
            target.isContentEditable
        ) {
            return;
        }

        dotnetHelper.invokeMethodAsync("OnGlobalKey", event.key);
    });
};
