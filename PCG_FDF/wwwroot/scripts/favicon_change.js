export function change_favicon(page) {
    const favicon = document.querySelector('link[rel="icon"]');
    switch (page) {
        case 0:
            break;
        case 8:
            favicon.href = "./assets/ico/shipping.ico";
            break;
        case 13:
            favicon.href = "./assets/ico/bafar.ico";
            break;
        case 14:
            favicon.href = "./assets/ico/sharks.ico";
            break;
        case 1014:
            favicon.href = "./assets/ico/evergreen.ico";
            break;
        case 2014:
            favicon.href = "./assets/ico/smargo.ico";
            break;
        case 2015:
            favicon.href = "./assets/ico/cargopi.ico";
            break;
        default:
            break;
    }
}