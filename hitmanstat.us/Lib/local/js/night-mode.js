function toggleNight() {
    localStorage.setItem('mode', (localStorage.getItem('mode') || 'night') === 'light' ? 'night' : 'light');
    if (localStorage.getItem('mode') === 'light') {
        document.querySelector('body').classList.remove('night-mode');
        document.querySelector("#night-link").innerHTML = "Night mode";
    } else {
        document.querySelector('body').classList.add('night-mode');
        document.querySelector("#night-link").innerHTML = "Light mode";
    }
}

document.addEventListener('DOMContentLoaded', function () {
    if ((localStorage.getItem('mode') || 'night') === 'light') {
        document.querySelector('body').classList.remove('night-mode');
        document.querySelector("#night-link").innerHTML = "Night mode";
    } else {
        document.querySelector('body').classList.add('night-mode');
        document.querySelector("#night-link").innerHTML = "Light mode";
    }
});


