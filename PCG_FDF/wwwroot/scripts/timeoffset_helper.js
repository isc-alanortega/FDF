export function GetTimezoneValue() {
    // Returns the time difference in minutes between UTC time and local time.
    return new Date().getTimezoneOffset();
}