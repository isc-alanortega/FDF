export function displayOverlay() {
    document.querySelector('.global-wrapper').classList.remove('d-none');
}

export function hideOverlay() {
    document.querySelector('.global-wrapper').classList.add('d-none');
    document.querySelector('.global-wrapper').removeEventListener("click", addDotNet);
}

export function addDotNet(dotNetObjectRef) {
    document.querySelector('.global-wrapper').addEventListener("click", () => {
        dotNetObjectRef.invokeMethodAsync("CustomClose");
    });
}
