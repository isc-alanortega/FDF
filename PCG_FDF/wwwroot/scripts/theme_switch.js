export function ManageThemeSwitch(theme) {
    const bodyStyles = document.body.style;
    for (const [key, value] of Object.entries(theme)) {
       bodyStyles.setProperty(key, value);
    }
}