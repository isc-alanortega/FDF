export function getCulture() {
    return window.localStorage['BlazorCulture']
}

export function setCulture(value) {
    window.localStorage['BlazorCulture'] = value
}