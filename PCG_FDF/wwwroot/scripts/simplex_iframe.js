export function TryInteconLogin(username, password) {
    return new Promise((resolve) => {
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';

        const url = `https://solicitudes.cimalogistic.com/simplex/login/?username=${encodeURIComponent(username)}&password=${encodeURIComponent(password)}`;
        iframe.src = url;

        const handleMessage = (event) => {
            try {
                const eventData = JSON.parse(event.data);

                if (eventData && eventData.type === 'loginSuccess') {
                    window.removeEventListener('message', handleMessage);
                    document.body.removeChild(iframe);
                    resolve(true);
                } else if (eventData && eventData.type === 'loginError') {
                    window.removeEventListener('message', handleMessage);
                    document.body.removeChild(iframe);
                    resolve(false);
                }
            } catch (error) {
                resolve(false);
            }
        };

        window.addEventListener('message', handleMessage);

        document.body.appendChild(iframe);
    });
}