document.getElementById('play-button').addEventListener('click', async function() {
    // download release/Czarodziejki ze Skaryszewa (Windows).zip
    const url = 'release/Czarodziejki ze Skaryszewa (Windows).zip';
    const response = await fetch(url);
    const blob = await response.blob();
    const urlBlob = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlBlob;
    a.download = 'Czarodziejki ze Skaryszewa (Windows).zip';
    a.click();
    URL.revokeObjectURL(urlBlob);
});