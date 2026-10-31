(function () {
    const select = document.getElementById("roomSelect");
    const info = document.getElementById("roomInfo");
    if (!select || !info) return;

    async function update() {
        if (!select.value || select.value === "0") { info.textContent = ""; return; }
        try {
            const res = await fetch(`/Rooms/Info/${select.value}`);
            if (!res.ok) throw new Error("not found");
            const r = await res.json();
            info.textContent = `${r.name}: floor ${r.floor}, ${r.capacity} seats, ${r.hasProjector ? "projector available" : "no projector"}`;
        } catch {
            info.textContent = "Could not load room details.";
        }
    }
    select.addEventListener("change", update);
    update();
})();
