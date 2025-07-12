export async function openFileFromStream(contentStreamReference, isMobile) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], {type: "application/pdf"});
    const url = URL.createObjectURL(blob);

    if (isMobile) {
        window.location.href = url;
    } else {
        window.open(url, '_blank');
    }
}