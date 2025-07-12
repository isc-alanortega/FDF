export function scrollToContent() {
    document.querySelector(".button-container").scrollIntoView({ block: 'center' });
}

export function animateAddProduct() {
    document.querySelector(".cart-boxes").classList.remove("d-none");
    anime({
        targets: ".cart-boxes",
        translateY: [0, 85],
        opacity: [100, 0],
        scale: [1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.5, 1.6],
        easing: 'easeInOutSine',
        duration: 1000,
        complete: function () {
            anime({
                targets: ".internal-floating-icon",
                scale: [1, 0.9, 0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, 1],
                easing: 'spring(1, 80, 10, 0)',
                duration: 500
            });
            document.querySelector('.cart-boxes').classList.add('d-none');
            anime({
                targets: ".cart-boxes",
                translateY: [85, 0],
                scale: 1,
                duration: 1,
            });
        }
    });
}

export function animateAndSwitch() {
    anime({
        targets: '.switching-component-wrapper',
        translateY: -2000,
        scale: 0.3
    });
    document.querySelector('.switching-component-wrapper').classList.add('d-none');
    document.querySelector(".search-component-wrapper").classList.remove('d-none');
    anime({
        targets: '.search-component-wrapper',
        translateY: 0,
        scale: 1
    });
}

export function returnToMain() {
    document.querySelector('.switching-component-wrapper').classList.remove('d-none');
    anime({
        targets: '.search-component-wrapper',
        translateY: -2000,
        scale: 0.3
    });
    document.querySelector(".search-component-wrapper").classList.add('d-none');
    anime({
        targets: '.switching-component-wrapper',
        translateY: 0,
        scale: 1
    });
}

export function animateSuggestion() {
    setTimeout(() => {
        document.querySelector('.package-suggestion').style.overflow = 'visible';
        anime({
            targets: '.suggestion-wrapper',
            translateX: ['200%', '0%'],
            easing: 'easeOutElastic(1, .4)',
            complete: function () {
                document.querySelector('.suggestion-wrapper').classList.add('animated-shadow');
            }
        });

    }, 500)
}