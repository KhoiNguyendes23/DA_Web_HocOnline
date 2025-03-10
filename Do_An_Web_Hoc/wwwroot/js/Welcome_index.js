const text = "Website Học Online !";
const speed = 100;
let i = 0;
function typeWriter() {
    if (i < text.length) {
        document.getElementById("text").innerHTML += text.charAt(i);
        i++;
        setTimeout(typeWriter, speed);
    }
}
window.onload = typeWriter;