// ── State ──
let currentSearch = '';
let currentCommunity = '';
let currentSort = '';
let debounceTimer = null;

// ── Init on DOM ready ──
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.querySelector('.Input-search');
    const container = document.getElementById('post-feed-container');
    // 🌟 ADDED: ดึงกล่อง Suggestions
    const suggestionsBox = document.getElementById('search-suggestions');

    if (!searchInput || !container) return;

    const params = new URLSearchParams(window.location.search);
    currentSearch = params.get('searchQuery') || '';
    currentCommunity = params.get('communityId') || '';
    currentSort = params.get('sortOrder') || '';

    // ── Search input with debounce & Suggestions ──
    searchInput.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        const term = searchInput.value.trim();

        // 🌟 ADDED: ฟังก์ชันจัดการ Suggestions
        handleSuggestions(term, suggestionsBox, searchInput);

        debounceTimer = setTimeout(() => {
            currentSearch = term;
            fetchPosts();
        }, 350);
    });

    // 🌟 ADDED: ปิดกล่อง Suggestions เมื่อคลิกข้างนอก
    document.addEventListener('click', (e) => {
        if (suggestionsBox && !searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
            suggestionsBox.style.display = 'none';
        }
    });

    // ── Prevent form submit ──
    const searchForm = searchInput.closest('form');
    if (searchForm) {
        searchForm.addEventListener('submit', e => e.preventDefault());
    }

    // ── Category pills (ปรับให้อ่านจาก data-community-id) ──
    document.querySelectorAll('.cat-pill').forEach(pill => {
        pill.addEventListener('click', e => {
            e.preventDefault();
            document.querySelectorAll('.cat-pill').forEach(p => p.classList.remove('active'));
            pill.classList.add('active');
            // 🌟 แก้จาก dataset เป็นgetAttribute เพื่อความแม่นยำ
            currentCommunity = pill.getAttribute('data-community-id') || '';
            fetchPosts();
        });
    });

    // ── Sort buttons (ปรับให้อ่านจาก data-sort) ──
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', e => {
            e.preventDefault();
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentSort = btn.getAttribute('data-sort') || '';
            fetchPosts();
        });
    });
});

// 🌟 ADDED: ฟังก์ชันใหม่สำหรับจัดการรายการแนะนำ (Autocomplete)
async function handleSuggestions(term, box, input) {
    if (!box) return;
    if (term.length < 1) {
        box.style.display = 'none';
        return;
    }

    try {
        const res = await fetch(`/Post/GetSuggestions?term=${encodeURIComponent(term)}`);
        const suggestions = await res.json();

        if (suggestions.length > 0) {
            box.innerHTML = suggestions.map(s => `<div class="suggestion-item">${s}</div>`).join('');
            box.style.display = 'block';

            // เมื่อกดเลือกคำแนะนำ
            box.querySelectorAll('.suggestion-item').forEach(item => {
                item.addEventListener('click', () => {
                    input.value = item.innerText;
                    box.style.display = 'none';
                    currentSearch = input.value;
                    fetchPosts(); // ค้นหาทันที
                });
            });
        } else {
            box.style.display = 'none';
        }
    } catch (err) {
        console.error("Suggestions error:", err);
    }
}

// ── Main fetch function (โค้ดเดิมของคุณ) ──
async function fetchPosts() {
    const container = document.getElementById('post-feed-container');
    if (!container) return;

    const params = new URLSearchParams();
    if (currentSearch) params.set('searchQuery', currentSearch);
    if (currentCommunity) params.set('communityId', currentCommunity);
    if (currentSort) params.set('sortOrder', currentSort);

    const newUrl = params.toString()
        ? `${window.location.pathname}?${params.toString()}`
        : window.location.pathname;
    window.history.pushState({}, '', newUrl);

    showLoading(container);

    try {
        const res = await fetch(`/Post/Index?${params.toString()}`, {
            headers: { 'X-Requested-With': 'FetchAPI' }
        });
        if (!res.ok) throw new Error('Network error');
        const html = await res.text();

        container.style.opacity = '0';
        container.style.transform = 'translateY(6px)';
        container.style.transition = 'opacity 0.15s ease, transform 0.15s ease';

        setTimeout(() => {
            container.innerHTML = html;
            container.style.opacity = '1';
            container.style.transform = 'translateY(0)';
        }, 150);
    } catch (err) {
        console.error('Fetch error:', err);
        container.innerHTML = `<div class="empty-state"><p>Something went wrong</p></div>`;
    }
}

// ── Loading skeleton ──
function showLoading(container) {
    const skeletons = Array.from({ length: 3 }, () => `
        <div class="post-card skeleton-card">
            <div class="skeleton-meta"></div>
            <div class="skeleton-title"></div>
            <div class="skeleton-body"></div>
            <div class="skeleton-footer"></div>
        </div>
    `).join('');
    container.innerHTML = skeletons;
    container.style.opacity = '1';
    container.style.transform = 'none';
}

// ── Navbar dropdown helpers ──
function toggleNotif() {
    const d = document.getElementById('notifDropdown');
    const b = document.getElementById('backdrop');
    const open = d?.classList.contains('open');
    closeAll();
    if (!open && d) { d.classList.add('open'); b?.classList.add('active'); }
}

function toggleProfile() {
    const d = document.getElementById('profileDropdown');
    const b = document.getElementById('backdrop');
    const open = d?.classList.contains('open');
    closeAll();
    if (!open && d) { d.classList.add('open'); b?.classList.add('active'); }
}

function closeAll() {
    document.querySelectorAll('.notif-dropdown, .profile-dropdown')
        .forEach(el => el.classList.remove('open'));
    document.getElementById('backdrop')?.classList.remove('active');
}

function clearNotifs() {
    document.querySelectorAll('.notif-item').forEach(el => el.classList.remove('unread'));
    const badge = document.getElementById('notifBadge');
    if (badge) badge.style.display = 'none';
}