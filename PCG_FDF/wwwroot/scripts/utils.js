// Transforma un string a datetime
// string que recibe "09/05/2024 09:50 M"
export function transformStringToDatetime(dateTime) {
    try {
        // Utilizar una expresión regular para dividir la cadena en partes
        const parts = dateTime.split(/[\s/:]+/);

        const month = parseInt(parts[0]) - 1; // Mes 0-11
        const day = parseInt(parts[1]);
        const year = parseInt(parts[2]);

        // Manejar las horas y minutos correctamente
        let hours = parseInt(parts[3]);
        const minutes = parseInt(parts[4]);

        // Ajustar horas según AM/PM
        if (parts[5] === "PM" && hours < 12) {
            hours += 12; // Convertir a formato 24 horas
        } else if (parts[5] === "AM" && hours === 12) {
            hours = 0; // 12 AM es medianoche
        }

        return new Date(year, month, day, hours, minutes);
    } catch (ex) {
        console.log(ex);
        return null; // Retornar null en caso de error
    }
}

export function getRandomColor(colors) {
    let letters = '0123456789ABCDEF';
    let color = '#';
    do {
        color = '#';
        for (let i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
    } while (isTooLight(color) && !colors.has(color));
    return color;
}

export function isTooLight(hexColor) {
    let r = parseInt(hexColor.substr(1, 2), 16);
    let g = parseInt(hexColor.substr(3, 2), 16);
    let b = parseInt(hexColor.substr(5, 2), 16);
    // Calculate luminance (perceived brightness) using the formula for sRGB luminance
    let luminance = (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255;
    // Return true if luminance is too high (color is too light), false otherwise
    return luminance > 0.7;
}