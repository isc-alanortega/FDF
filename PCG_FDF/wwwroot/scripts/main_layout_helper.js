export function toggleDrawer() {
    let drawer = document.querySelector('.chat-drawer');
    let currentHeight = parseInt(getComputedStyle(drawer).height, 10);
    drawer.style.height = currentHeight === 0 ? 'var(--drawer-height)' : '0';
}

export function setCssVariable(name, value) {
    document.documentElement.style.setProperty(name, value);
}