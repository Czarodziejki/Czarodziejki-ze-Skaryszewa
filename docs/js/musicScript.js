let audioMuted = false;
let audioStarted = true;

var positionAbsolute;

const audioElement = document.getElementById('main-screen-audio');
audioElement.volume = 0.5;
try {
    await audioElement.play();
} catch (error) {
    audioStarted = false;
}

if (audioStarted) {
    addMuteButton();
} else {
    // add a listener to start audio after the user clicks on the page
    document.addEventListener('click', function() {
        if (audioStarted) {
            return;
        }
        audioStarted = true;
        audioElement.play();

        // add an element to the page to allow the user to mute the audio
        addMuteButton();
    });
}

function addMuteButton() {
    const muteButton = document.createElement('button');
    muteButton.classList.add('mute-button');
    muteButton.innerText = '🔊';
    document.body.appendChild(muteButton);

    if (positionAbsolute) {
        muteButton.style.position = 'absolute';
    }

    muteButton.addEventListener('click', function () {
        if (audioMuted) {
            audioElement.muted = false;
            muteButton.innerText = '🔊';
            audioMuted = false;
        } else {
            audioElement.muted = true;
            muteButton.innerText = '🔈';
            audioMuted = true;
        }
    });
}
