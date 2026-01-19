window.registerGlobalKeyboard = (dotnetHelper) => {
    document.addEventListener("keydown", (event) => {
        dotnetHelper.invokeMethodAsync("OnGlobalKey", event.key);
    });
};