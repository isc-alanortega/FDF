const onConfirmRefresh = function (event) {
    // Cancel the event as stated by the standard.
    event.preventDefault();
    // Chrome requires returnValue to be set.
    event.returnValue = '';
}

export function AddListener() {
    window.addEventListener("beforeunload", onConfirmRefresh, { capture: true });
}

export function RemoveListener() {
    window.removeEventListener("beforeunload", onConfirmRefresh, { capture: true });
}
