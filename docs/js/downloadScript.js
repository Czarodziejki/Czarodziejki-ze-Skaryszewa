async function downloadFile(url) {
    const response = await fetch(url);
    const blob = await response.blob();
    const urlBlob = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlBlob;
    a.download = url.split('/').pop();
    a.click();
    URL.revokeObjectURL(urlBlob);
}

document.getElementById('windows-button').addEventListener('click', async function() {
    const url = 'release/Czarodziejki ze Skaryszewa (Windows).zip';
    await downloadFile(url);
});

document.getElementById('linux-button').addEventListener('click', async function() {
    const url = 'release/Czarodziejki ze Skaryszewa (Linux).zip';
    await downloadFile(url);
});