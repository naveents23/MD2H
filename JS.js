
(function () {
    // Optional enhancement: mark the active link without classes/ids using aria-current
    document.addEventListener('click', function (e) {
        const a = e.target.closest('a');
        if (!a) return;
        // Remove previous aria-current
        document.querySelectorAll('aside nav a[aria-current="page"]').forEach(el => el.removeAttribute('aria-current'));
        a.setAttribute('aria-current', 'page');
    });
})();

// Minimal copy-to-clipboard (no libraries)
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.copy-btn');
    if (!btn) return;

    const container = btn.closest('.code-container');
    const codeEl = container.querySelector('code');
    const text = codeEl.textContent; // copies raw code (no markup)

    const done = () => {
        const original = btn.textContent;
        btn.textContent = 'Copied!';
        btn.disabled = true;
        setTimeout(() => {
            btn.textContent = original;
            btn.disabled = false;
        }, 1200);
    };

    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(done).catch(() => {
            // fallback below
            const ta = document.createElement('textarea');
            ta.value = text;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.focus();
            ta.select();
            try { document.execCommand('copy'); } catch { }
            document.body.removeChild(ta);
            done();
        });
    } else {
        // Legacy fallback
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.style.position = 'fixed';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.focus();
        ta.select();
        try { document.execCommand('copy'); } catch { }
        document.body.removeChild(ta);
        done();
    }
});
