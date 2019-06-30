function toggleNight() {
    localStorage.setItem('mode', (localStorage.getItem('mode') || 'light') === 'night' ? 'light' : 'night');
    if (localStorage.getItem('mode') === 'night') {
        document.querySelector('body').classList.add('night-mode');
        document.querySelector("#night-link").innerHTML = "Light mode";
    } else {
        document.querySelector('body').classList.remove('night-mode');
        document.querySelector("#night-link").innerHTML = "Night mode";
    }
}

document.addEventListener('DOMContentLoaded', function () {
    if ((localStorage.getItem('mode') || 'light') === 'night') {
        document.querySelector('body').classList.add('night-mode');
        document.querySelector("#night-link").innerHTML = "Light mode";
    } else {
        document.querySelector('body').classList.remove('night-mode');
        document.querySelector("#night-link").innerHTML = "Night mode";
    }
});


